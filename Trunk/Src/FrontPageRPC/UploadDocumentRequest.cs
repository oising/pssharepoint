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

namespace HubKey.Net.FrontPageRPC
{
    public class UploadDocumentRequest: RequestBase
    {
        private Document _document;
        private PutOptionEnum _putOption = PutOptionEnum.Default;
        private string _comment = "";
        private bool _keepCheckedOut = false;
        private bool _updateAllMetaInfo = false;

        /// <summary>
        /// Provides a method to write a single file to a directory in an existing Web site.
        /// </summary>
        /// <param name="document">The document to upload, including the System.IO.FileInfo path and meta data as required.</param>
        public UploadDocumentRequest(Document document)
            : base()
        {
            this._document = document;
            this.UseFrontPageRPCWebRequest = true;
        }

        /// <summary>
        /// Gets or sets the document to upload, including the System.IO.FileInfo path and meta data as required.
        /// </summary>
        public Document Document
        {
            get { return _document; }
            set { _document = value; }
        }

        /// <summary>
        /// A set of flags describing how the operation behaves.
        /// </summary>
        public PutOptionEnum PutOption
        {
            get { return _putOption; }
            set { _putOption = value; }
        }

        /// <summary>
        /// Gets or sets a string value that provides a checkin comment for the file being uploaded. 
        /// The server MUST ignore this parameter unless the "checkin" PUT-OPTION-VAL is specified in the put_option parameter. 
        /// Because clients conforming to the FrontPage Server Extensions Remote Protocol MUST NOT send the "checkin" PUT_OPTION_VAL, 
        /// as specified in Put-Option, servers MAY ignore this parameter.
        /// </summary>
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Gets or sets a boolean value used to determine a specified document's behavior in source control. If true, 
        /// the document SHOULD be checked in to source control and immediately checked back out; if false, 
        /// the document SHOULD be checked in. The server MUST treat this as equivalent to the "checkout" PUT-OPTION-VAL, 
        /// as specified in Put-Option. Clients conforming to the FrontPage Server Extensions Remote Protocol MUST send false, either 
        /// explicitly or by omitting this parameter.
        /// </summary>
        public bool KeepCheckedOut
        {
            get { return _keepCheckedOut; }
            set { _keepCheckedOut = value; }
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
            get { return "put document"; }
        }

        public override long ContentLength
        {
            get
            {
                if (_contentLength == 0)
                {
                    _document.FileInfo.Refresh();
                    _contentLength = base.ContentLength + 1 + _document.FileInfo.Length;
                }
                return _contentLength;
            }
        }

        internal override ParameterCollection Parameters
        {
            get 
            {
                _parameters = new ParameterCollection();
                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/testsite1");

                _parameters.Add("put_option", Utils.GetPutOptionAsString(_putOption));
                _parameters.Add("comment", _comment);
                _parameters.Add("keep_checked_out", _keepCheckedOut);

                Parameter document_name = new Parameter("document_name", _document.WebRelativeUrl);

                if (_updateAllMetaInfo)
                    _document.MetaInfo.SetValueChanged(true);

                Parameter meta_info = new Parameter("meta_info", _document.MetaInfo);
                StringVector document = new StringVector(document_name, meta_info);

                _parameters.Add("document", document);


                return _parameters; 
            }
        }

        internal override void  WriteRequest(Stream requestStream)
        {
            FileStream fStream = null;
            try
            {
                fStream = File.OpenRead(_document.FileInfo.FullName);
                int nread;
                byte[] buffer = new byte[4096];

                MemoryStream methodStream = new MemoryStream(RequestBytes);
                long running = 0;
                while ((nread = methodStream.Read(buffer, 0, 4096)) != 0)
                {
                    _wc.UpdateUploadProgress(ref running, _contentLength, nread);
                    requestStream.Write(buffer, 0, nread);
                }

                requestStream.WriteByte(10);
                requestStream.Flush();
                while ((nread = fStream.Read(buffer, 0, 4096)) != 0)
                {
                    _wc.UpdateUploadProgress(ref running, _contentLength, nread);
                    requestStream.Write(buffer, 0, nread);
                    requestStream.Flush();
                }
                requestStream.Flush();
            }
            finally
            {
                if (fStream != null)
                    fStream.Close();
                fStream = null;
                RequestBytes = null;
            }
        }

        
    }
}
