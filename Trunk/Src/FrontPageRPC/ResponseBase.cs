#region license
// Copyright 2008 HubKey, LLC. (http://www.hubkey.com)
// 
// This software is provided under the Creative Commons Attribution-Noncommercial-Share Alike 3.0 license.
// For the full licence see http://creativecommons.org/licenses/by-nc-sa/3.0

//  You are free:
//    * to Share — to copy, distribute, display, and perform the work
//    * to Remix — to make derivative works
//  Under the following conditions:
//    * Attribution. You must attribute the work in the manner specified by the author or licensor (but not in any way that suggests that they endorse you or your use of the work).
//    * Noncommercial. You may not use this work for commercial purposes.
//    * Share Alike. If you alter, transform, or build upon this work, you may distribute the resulting work only under the same or similar license to this one.
//    * Any of the above conditions can be waived if you get permission from the copyright holder.
//    * Apart from the remix rights granted under this license, nothing in this license impairs or restricts the author's moral rights.
// 
// The above copyright notice and permission notice shall be included in all copies or derivatives of this software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using HubKey.Net;

namespace HubKey.Net.FrontPageRPC
{
    public abstract class ResponseBase : IResponse
    {
        static Regex fixPLIRegex = new Regex(@"<(?<tag>(p|li))>(?<value>.*)", RegexOptions.Compiled);
        static Regex fixNestedULRegex = new Regex(@"<(?<tag>(p|li))>(?<name>.*?)=</\k<tag>>\n<ul>", RegexOptions.Compiled);
        static Regex fixNameValueRegex = new Regex(@"<(?<tag>(p|li))>(?<name>.*?)=(?<value>.*?)</\k<tag>>", RegexOptions.Compiled);
        static readonly string NeverFindString = Encoding.UTF8.GetString(new byte[] { 160, 96, 130, 129, 176, 186, 42, 173, 213, 168 }) + Guid.NewGuid().ToString();
        const string StartOfHtml = "<html>";
        const string EndOfHtml = "\n</html>";
        protected string _xhtml;
        protected string _html;
        protected XmlDocument _doc;
        protected XPathNavigator _nav;
        private string _method = "";
        private FrontPageRPCError _errorResponse;
        protected long _responseLength;
        protected long _bytesRecieved;
        protected ProcessingType _processingType;
        protected WebClient _wc;
        protected int _length;
        protected long _running;
        protected Stream _response;
        protected MemoryStream _bufferStream;

        internal ResponseBase(WebClient wc)
        {
            this._wc = wc;
        }

        /// <summary>
        /// Gets the method response from the server.
        /// </summary>
        public string Method
        {
            get { return _method; }
            protected set { _method = value; }
        }

        /// <summary>
        /// Gets the error returned by the server if sent.
        /// </summary>
        public FrontPageRPCError ErrorResponse
        {
            get { return _errorResponse; }
            internal set { _errorResponse = value; }
        }

        /// <summary>
        /// Gets whether an error response was sent by the server.
        /// </summary>
        public bool HasError
        {
            get { return _errorResponse != null; }
        }

        /// <summary>
        /// Gets the error message if sent by the server.
        /// </summary>
        public string ErrorMessage
        {
            get 
            {
                if (!HasError)
                    return null;
                if (_errorResponse.Exception == null)
                    return _errorResponse.Msg;
                else
                    return _errorResponse.Msg + "\r\n\r\n" + _errorResponse.Exception.ToString(); 
            }
        }


        
        internal virtual void ReadResponse(Stream response, long responseLength)
        {
            Init(response, responseLength);
            ReadHtml();
        }

        protected void Init(Stream response, long responseLength)
        {
            _response = response;
            _responseLength = responseLength;
            _bytesRecieved = 0;
            _running = 0;
            int iResponseLength = (int)this._responseLength;
            _length = (iResponseLength < 0 || iResponseLength > 32768) ? 32768 : iResponseLength;
        }

        protected Folder GetRootFolder(FolderCollection _folders, DocumentCollection _documents)
        {
            return GetRootFolder(_folders, _documents, true);
        }

        protected Folder GetRootFolder(FolderCollection _folders, DocumentCollection _documents, bool rootFolderMustExist)
        {
            FolderCollection folders = _folders.ShallowClone();
            DocumentCollection documents = _documents.ShallowClone();
            foreach (Folder folder in _folders)
            {
                RemoveItems<Document>.Execute(documents, delegate(Document document)
                    {
                        if (folder.WebRelativeUrl == document.WebRelativeFolderUrl)
                        {
                            document.Parent = folder;
                            folder.Documents.Add(document);
                            return true;
                        }
                        return false;
                    }
                );
                RemoveItems<Folder>.Execute(folders, delegate(Folder subFolder)
                    {
                        if (folder.WebRelativeUrl == subFolder.ParentFolderUrl)
                        {
                            subFolder.Parent = folder;
                            folder.Folders.Add(subFolder);
                            return true;
                        }
                        if (folder.ParentFolderUrl == subFolder.WebRelativeUrl)
                        {
                            folder.Parent = subFolder;
                            return false;
                        }
                        return false;
                    }
                );
            }
            if (folders.Count == 0)
            {
                if (rootFolderMustExist)
                    throw new FrontPageRPCException("No root folder was found in the response.");
                return null;
            }
            return folders[0];
        }

        protected void ReadHtml()
        {
            MemoryStream resStream = new MemoryStream(_length);
            if (-1 == ReadUntil(_response, resStream, EndOfHtml, _length, 0))
            {
                ReadUntil(_response, resStream, null, _length, 0);
                _html = WebClient.GetStringFromStream(resStream);
                throw new FrontPageRPCException(string.Format("No html was found in the response: {0}", _html));
            }
            Read(_response, new byte[EndOfHtml.Length + 1]);
            _html = WebClient.GetStringFromStream(resStream) + EndOfHtml;
            _xhtml = fixPLIRegex.Replace(_html, delegate(Match m) { return string.Format("{0}</{1}>", m.Value, m.Groups["tag"].Value); });
            _xhtml = fixNestedULRegex.Replace(_xhtml, delegate(Match m) { return string.Format("<ul name=\"{0}\">", m.Groups["name"].Value); });
            _xhtml = fixNameValueRegex.Replace(_xhtml, delegate(Match m) { return string.Format("<{0} name=\"{1}\">{2}</{3}>", m.Groups["tag"].Value, m.Groups["name"].Value, m.Groups["value"].Value, m.Groups["tag"].Value); });
            _xhtml = "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" + _xhtml;
            _doc = new XmlDocument();
            _doc.LoadXml(_xhtml);
            _nav = _doc.CreateNavigator();
            if (_nav == null)
                throw new FrontPageRPCException(string.Format("Html could not be parsed: {0}", this._html), null);
            FrontPageRPCError error = new FrontPageRPCError(_nav);
            if (error.Status != 0)
                throw new FrontPageRPCException("An error was returned by the server.", null, error);
            Method = _nav.SelectSingleNode("//p[@name='method']").Value;
        }

        protected int ReadUntil(Stream stIn, Stream stOut, string searchString, int bufferLength, int startIndex)
        {
            if (string.IsNullOrEmpty(searchString))
                searchString = NeverFindString;
            byte[] buffer2 = new byte[bufferLength];
            int nread;
            if (_bufferStream == null)
                _bufferStream = new MemoryStream(bufferLength);
            while (true)
            {
                byte[] buffer = new byte[(int)_bufferStream.Length];
                while ((nread = _bufferStream.Read(buffer, 0, (int)_bufferStream.Length)) != 0)
                {
                    string s = Encoding.UTF8.GetString(buffer, 0, nread);
                    int i = ByteArrayFirstIndexOf(buffer, Encoding.UTF8.GetBytes(searchString), 0);
                    if (i == -1)
                    {
                        if (searchString == null || _bufferStream.Length > bufferLength + searchString.Length)
                        {
                            byte[] outBuffer = _bufferStream.GetBuffer();
                            int pos = (int)_bufferStream.Position;
                            stOut.Write(outBuffer, 0, bufferLength);
                            _wc.UpdateDownloadProgress(ref _running, _responseLength, bufferLength);
                            _bufferStream = new MemoryStream(bufferLength);
                            _bufferStream.Write(outBuffer, bufferLength, pos - bufferLength);
                            _bufferStream.Position = 0;
                        }
                        else
                            break;
                    }
                    else
                    {
                        stOut.Write(buffer, 0, i);
                        _wc.UpdateDownloadProgress(ref _running, _responseLength, i);
                        _bufferStream = new MemoryStream(bufferLength);
                        _bufferStream.Write(buffer, i, nread - i);
                        _bufferStream.Position = 0;
                        return i;
                    }
                    buffer = new byte[(int)_bufferStream.Length];
                }
                if ((nread = stIn.Read(buffer2, 0, bufferLength)) != 0)
                {
                    _bufferStream.Write(buffer2, 0, nread);

                }
                else
                {
                    _bufferStream.Position = 0;
                    byte[] b = _bufferStream.GetBuffer();
                    int l = (int)_bufferStream.Length;
                    stOut.Write(b, 0, l);
                    _wc.UpdateDownloadProgress(ref _running, _responseLength, l);
                    return -1;
                }
                _bufferStream.Position = 0;
            }
        }


        protected static int ByteArrayFirstIndexOf(byte[] a, byte[] b, int startIndex)
        {
            for (int i = startIndex; i <= a.Length - b.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < b.Length; j++)
                {
                    found = found && (a[i + j] == b[j]);
                    if (!found) break;
                }
                if (found) return i;
            }
            return -1;
        }

        protected int Read(Stream stIn, byte[] buffer)
        {
            int bufferLength = buffer.Length;
            int nread = 0;
            if (_bufferStream == null)
                _bufferStream = new MemoryStream(bufferLength);
            int pos = (int)_bufferStream.Length;
            int readFromBuffer = Math.Min(pos, bufferLength);
            int readFromStream = bufferLength - readFromBuffer;
            if (readFromBuffer > 0)
            {
                byte[] outBuffer = _bufferStream.GetBuffer();
                Buffer.BlockCopy(outBuffer, 0, buffer, 0, readFromBuffer);
                _bufferStream = new MemoryStream(bufferLength);
                _bufferStream.Write(outBuffer, bufferLength, pos - readFromBuffer);
                _bufferStream.Position = 0;
            }
            if (readFromStream > 0)
            {
                nread = stIn.Read(buffer, readFromBuffer, readFromStream);
            }
            return readFromBuffer + nread;
        }
    }

}
