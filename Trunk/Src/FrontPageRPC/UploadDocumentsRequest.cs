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
using System.Threading;

namespace HubKey.Net.FrontPageRPC
{
    public class UploadDocumentsRequest: RequestBase
    {
        private DocumentCollection _documents;
        private bool _listLinkInfo = true;
        private PutOptionEnum _putOption = PutOptionEnum.Default;
        private bool _listFiles = true;
        private bool _updateAllMetaInfo = false;

        byte[] _boundaryBytes;
        static readonly byte[] _crlf = Encoding.UTF8.GetBytes("\r\n");
        static readonly byte[] _lf = Encoding.UTF8.GetBytes("\n");
        static readonly byte[] _end = Encoding.UTF8.GetBytes("--");
        static readonly byte[] _urlencoded = Encoding.UTF8.GetBytes("Content-Type: application/x-www-form-urlencoded\r\n\r\n");
        static readonly byte[] _octetstream = Encoding.UTF8.GetBytes("Content-Type: application/octet-stream\r\n\r\n");

        public UploadDocumentsRequest(DocumentCollection documents)
            : base()
        {
            this._documents = documents;
            this.UseFrontPageRPCWebRequest = true;
            _isMultiPart = true;
        }

        /// <summary>
        /// Gets or sets the documents to upload.
        /// </summary>
        public DocumentCollection Documents
        {
            get { return _documents; }
            set { _documents = value; }
        }

        /// <summary>
        /// Gets or sets flags describing how the operation should behave.
        /// </summary>
        public PutOptionEnum PutOption
        {
            get { return _putOption; }
            set { _putOption = value; }
        }

        /// <summary>
        /// Gets or set whether to list the metadata of files contained in each directory.
        /// </summary>
        public bool ListFiles
        {
            get { return _listFiles; }
            set { _listFiles = value; }
        }

        /// <summary>
        /// Gets or sets whether or not the return value of the method contains information about the links from the current page(s). Set true to include link information in the return value. 
        /// </summary>
        public bool ListLinkInfo
        {
            get { return _listLinkInfo; }
            set { _listLinkInfo = value; }
        }

        /// <summary>
        /// Gets or sets a boolean value used to determine whether only changed meta info is updated on the server.
        /// </summary>
        public bool UpdateAllMetaInfo
        {
            get { return _updateAllMetaInfo; }
            set { _updateAllMetaInfo = value; }
        }

        public override string MethodName
        {
            get { return "put documents"; }
        }

        internal override ParameterCollection Parameters
        {
            get 
            {
                _parameters = new ParameterCollection();
                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");

                _parameters.Add("put_option", Utils.GetPutOptionAsString(_putOption));
                _parameters.Add("listFiles", _listFiles);
                _parameters.Add("listLinkInfo", _listLinkInfo);

                return _parameters; 
            }
        }

        public override long ContentLength
        {
            get
            {
                if (_contentLength == 0)
                {
                    _contentLength += BoundaryBytes.Length + RequestBytes.Length + (_crlf.Length * 2) + _urlencoded.Length + _lf.Length;
                    foreach (Document document in _documents)
                    {
                        document.FileInfo.Refresh();

                        if (_updateAllMetaInfo)
                            document.MetaInfo.SetValueChanged(true);

                        _contentLength += (BoundaryBytes.Length * 2) + (_crlf.Length * 4) + _octetstream.Length + _urlencoded.Length + _lf.Length;
                        _contentLength += document.RequestBytes.Length;
                        _contentLength += document.FileInfo.Length;
                    }
                    _contentLength += BoundaryBytes.Length + _end.Length;
                }
                return _contentLength;
            }
        }

        byte[] BoundaryBytes
        {
            get 
            { 
                if (_boundaryBytes == null)
                    _boundaryBytes = Encoding.UTF8.GetBytes(WebClient.Boundary);
                return _boundaryBytes;
            }
        }

        internal override void WriteRequest(Stream requestStream)
        {
            try
            {
                requestStream.Write(BoundaryBytes, 0, BoundaryBytes.Length);
                requestStream.Write(_crlf, 0, _crlf.Length);
                requestStream.Write(_urlencoded, 0, _urlencoded.Length);
                requestStream.Write(RequestBytes, 0, RequestBytes.Length);
                requestStream.Write(_lf, 0, _lf.Length);
                requestStream.Write(_crlf, 0, _crlf.Length);
                long running = 0;
                int headerCount = BoundaryBytes.Length + RequestBytes.Length + (_crlf.Length * 2) + _urlencoded.Length + _lf.Length;
                _wc.UpdateUploadProgress(ref running, headerCount, headerCount);
                headerCount = (BoundaryBytes.Length * 2) + (_crlf.Length * 4) + _octetstream.Length + _urlencoded.Length + _lf.Length;
                foreach (Document document in _documents)
                {
                    Thread.Sleep(1);
                    requestStream.Write(BoundaryBytes, 0, BoundaryBytes.Length);
                    requestStream.Write(_crlf, 0, _crlf.Length);
                    requestStream.Write(_urlencoded, 0, _urlencoded.Length);
                    requestStream.Write(document.RequestBytes, 0, document.RequestBytes.Length);
                    requestStream.Write(_lf, 0, _lf.Length);
                    requestStream.Write(_crlf, 0, _crlf.Length);

                    requestStream.Write(BoundaryBytes, 0, BoundaryBytes.Length);
                    requestStream.Write(_crlf, 0, _crlf.Length);
                    requestStream.Write(_octetstream, 0, _octetstream.Length);

                    _wc.UpdateUploadProgress(ref running, headerCount, headerCount);

                    FileStream fStream = null;
                    try
                    {
                        fStream = File.OpenRead(document.FileName);
                        int nread;
                        byte[] buffer = new byte[4096];
                        while ((nread = fStream.Read(buffer, 0, 4096)) != 0)
                        {
                            _wc.UpdateUploadProgress(ref running, _contentLength, nread);
                            requestStream.Write(buffer, 0, nread);
                        }

                        requestStream.Flush();
                    }
                    finally
                    {
                        if (fStream != null)
                            fStream.Close();
                    }
                    requestStream.Write(_crlf, 0, _crlf.Length);
                }
                requestStream.Write(BoundaryBytes, 0, BoundaryBytes.Length);
                requestStream.Write(_end, 0, _end.Length);
                requestStream.Flush();
            }
            finally
            {
                RequestBytes = null;
            }
        }

    }
}
