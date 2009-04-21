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
    public class DownloadDocumentsRequest: RequestBase
    {
        private DocumentCollection _documents;

        /// <summary>
        /// Provides a method to retrieve the specified document for viewing on the client computer.
        /// </summary>
        /// <param name="document">The document to download. The file is streamed to the path specified in the FileInfo property.
        /// If no file is specified, a temporary file is created.</param>
        public DownloadDocumentsRequest(Document document)
        {
            this._documents = new DocumentCollection(document);
        }
        
        /// <summary>
        /// Provides a method to retrieve the specified documents for viewing on the client computer.
        /// </summary>
        /// <param name="documents">The documents to be downloaded. The files are streamed to the paths specified in the FileInfo properties.
        /// If no file is specified, a temporary file is created.</param>
        public DownloadDocumentsRequest(DocumentCollection documents)
        {
            this._documents = documents;
        }

        /// <summary>
        /// Provides a method to retrieve the specified documents for viewing on the client computer.
        /// </summary>
        /// <param name="fullWebRelativeNames"></param>
        public DownloadDocumentsRequest(IEnumerable<string> fullWebRelativeNames)
        {
            this._documents = new DocumentCollection(fullWebRelativeNames);
        }

        /// <summary>
        /// Provides a method to retrieve the specified documents for viewing on the client computer.
        /// </summary>
        /// <param name="fullWebRelativeNames"></param>
        public DownloadDocumentsRequest(params string[] fullWebRelativeNames)
        {
            this._documents = new DocumentCollection(fullWebRelativeNames);
        }

        /// <summary>
        /// Gets or sets the documents to be downloaded. The files are streamed to the paths specified in the FileInfo properties.
        /// If no file is specified, a temporary file is created.
        /// </summary>
        public DocumentCollection Documents
        {
            get { return _documents; }
            set { _documents = value; }
        }

        public override string MethodName
        {
            get { return "get documents"; }
        }

        internal override ParameterCollection Parameters
        {
            get 
            {
                _parameters = new ParameterCollection();

                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");
                _parameters.Add("url_list", new StringVector(this._documents.FullWebRelativeNames));

                return _parameters; 
            }
        }

    }
}
