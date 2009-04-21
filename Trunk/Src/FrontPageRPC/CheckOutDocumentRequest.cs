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

namespace HubKey.Net.FrontPageRPC
{
    public class CheckOutDocumentRequest : RequestBase
    {
        Document _document;
        bool _force = false;
        int _timeout = 0;

        /// <summary>
        /// Provides a method to enable a user who is currently authenticated to make changes to a document under source control.
        /// </summary>
        /// <param name="webUrl">The web site URL.</param>
        public CheckOutDocumentRequest(string webUrl, string documentWebRelativeUrl)
        {
            this.WebUrl = webUrl;
            _document = new Document(documentWebRelativeUrl);
        }


        /// <summary>
        /// Provides a method to enable a user who is currently authenticated to make changes to a document under source control.
        /// </summary>
        /// <param name="document">The document to checkout.</param>
        public CheckOutDocumentRequest(Document document)
        {
            _document = document;
        }

        /// <summary>
        /// Gets or sets the document to checkout.
        /// </summary>
        public Document Document
        {
            get { return _document; }
            set { _document = value; }
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
        /// Gets or sets the number of seconds a short-term lock is reserved. Within this time, the client computer must renew its lock to retain the lock.
        /// </summary>
        public int TimeOut
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        public override string MethodName
        {
            get { return "checkout document"; }
        }

        internal override ParameterCollection Parameters
        {
            get
            {
                _parameters = new ParameterCollection();
                
                
                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");
                _parameters.Add("document_name", _document.WebRelativeUrl);
                _parameters.Add("force", _force? 1:0);
                _parameters.Add("timeout", _timeout);
                return _parameters;
            }
        }
    }
}
