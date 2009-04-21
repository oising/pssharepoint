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
    public class DownloadDocumentRequest: RequestBase
    {
        private Document _document;
        private int _timeOut = 0;
        private GetOptionEnum _getOption = GetOptionEnum.none;
        private bool _force = false;
        private string _documentVersion = "";

        /// <summary>
        /// Provides a method to retrieve the specified document for viewing on the client computer.
        /// </summary>
        /// <param name="fullWebRelativeName">The web relative URL of the document.</param>
        public DownloadDocumentRequest(string fullWebRelativeName)
        {
            this._document = new Document(fullWebRelativeName);
        }

        /// <summary>
        /// Provides a method to retrieve the specified document for viewing on the client computer.
        /// </summary>
        /// <param name="document">The document to download. The file is streamed to the path specified in the FileInfo property.
        /// If no file is specified, a temporary file is created.</param>
        public DownloadDocumentRequest(Document document)
        {
            this._document = document;
        }

        /// <summary>
        /// Gets or sets document to download. The file is streamed to the path specified in the FileInfo property.
        /// If no file is specified, a temporary file is created.
        /// </summary>
        public Document Document
        {
            get { return _document; }
            set { _document = value; }
        }

        /// <summary>
        /// Gets or sets a parameter that specifies how documents are checked out from source control.
        /// </summary>
        public GetOptionEnum GetOption
        {
            get { return _getOption; }
            set { _getOption = value; }
        }

        /// <summary>
        /// Gets or sets a parameter used with source control to undo checkout of a file that is checked out by some other user. The default value is false.
        /// </summary>
        public bool Force
        {
            get { return _force; }
            set { _force = value; }
        }

        /// <summary>
        /// Gets or sets the number of seconds a short-term lock is reserved. Within this time, the client computer must renew its lock to retain the lock. Default is 0.
        /// </summary>
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        /// <summary>
        /// Gets or sets an optional version number so that an earlier version of the document may be downloaded.
        /// </summary>
        public string DocumentVersion
        {
            get { return _documentVersion; }
            set { _documentVersion = value; }
        }

        public override string MethodName
        {
            get { return "get document"; }
        }

        internal override ParameterCollection Parameters
        {
            get 
            {
                _parameters = new ParameterCollection();

                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");
                _parameters.Add("document_name", _document.WebRelativeUrl);
                _parameters.Add("force", _force);
                _parameters.Add("get_option", _getOption.ToString());
                if (!string.IsNullOrEmpty(_documentVersion))
                    _parameters.Add("doc_version", _documentVersion);
                _parameters.Add("timeout", _timeOut);

                return _parameters; 
            }
        }

    }
}
