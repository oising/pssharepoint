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
using System.IO;
using System.Web;
using System.Threading;

namespace HubKey.Net.FrontPageRPC
{
    public class DownloadDocumentsResponse : ResponseBase
    {
        private DocumentCollection _documents;
        DownloadDocumentsRequest _request;
        private string _message = "";

        internal DownloadDocumentsResponse(WebClient wc, DownloadDocumentsRequest downloadRequest)
            : base(wc)
        {
            _request = downloadRequest;
        }

        /// <summary>
        /// Gets the documents downloaded from the server.
        /// </summary>
        public DocumentCollection Documents
        {
            get
            {
                if (_documents == null)
                    _documents = new DocumentCollection();
                return _documents;
            }
        }

        /// <summary>
        /// Gets the message response from the server.
        /// </summary>
        public string Message
        {
            get { return _message; }
        }


        internal override void ReadResponse(Stream responseStream, long responseLength)
        {
            FileStream f = null;
            try
            {
                Init(responseStream, responseLength);

                byte[] boundary = new byte[62];
                byte[] formHeader = new byte[51];
                byte[] binHeader = new byte[44];
                this._responseLength = responseLength;


                _documents = new DocumentCollection();
                MemoryStream resStream = new MemoryStream(_length);
                string docVector = null;

                responseStream.Read(boundary, 0, 62);
                string sboundary = Encoding.UTF8.GetString(boundary);
                responseStream.ReadByte();
                responseStream.ReadByte();
                responseStream.Read(formHeader, 0, 51);

                ReadUntil(responseStream, resStream, sboundary, _length, 0);
                Read(responseStream, boundary);
                this.Method = WebClient.GetStringFromStream(resStream);

                while (Read(responseStream, formHeader) != 0)
                {
                    Thread.Sleep(1);

                    resStream = new MemoryStream(_length);

                    ReadUntil(responseStream, resStream, sboundary, _length, 0);
                    docVector = WebClient.GetStringFromStream(resStream).Trim();
                    if (!docVector.StartsWith("document="))
                        break;
                    docVector = HttpUtility.UrlDecode(docVector);
                    Document document = new Document(docVector, true);
                    document.WebUrl = _request.WebUrl;
                    document.FileInfo = _request.Documents[document.WebRelativeUrl].FileInfo;

                    f = new FileStream(document.FileName, FileMode.Create);
                    Read(responseStream, boundary);
                    Read(responseStream, binHeader);
                    string sbinHeader = Encoding.UTF8.GetString(binHeader);
                    if (!sbinHeader.StartsWith("\r\nContent-Type: application/octet-stream"))
                        break;
                    resStream = new MemoryStream(_length);
                    ReadUntil(responseStream, f, sboundary, _length, 0);
                    string file = WebClient.GetStringFromStream(resStream);
                    f.Close();
                    f = null;
                    _documents.Add(document);
                    Read(responseStream, boundary);
                }
            }
            finally
            {
                if (f != null)
                    f.Close();
            }
        }
    }
}
