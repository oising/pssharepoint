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
    public class UnCheckOutDocumentRequest : RequestBase
    {
        Document _document;
        bool _force = false;
        bool _releaseShortTermLock = false;
        DateTime _timeCheckedOut = DateTime.MinValue;

        /// <summary>
        /// Provides a method to undo a check-out of a file from a source control database. If the file has changed after it was checked out, this method causes those changes to be lost.
        /// </summary>
        /// <param name="webUrl">The web site URL.</param>
        public UnCheckOutDocumentRequest(string webUrl, string documentWebRelativeUrl)
        {
            this.WebUrl = webUrl;
            _document = new Document(documentWebRelativeUrl);
        }

        /// <summary>
        /// Provides a method to undo a check-out of a file from a source control database. If the file has changed after it was checked out, this method causes those changes to be lost.
        /// </summary>
        /// <param name="document">The document to un check out.</param>
        public UnCheckOutDocumentRequest(Document document)
        {
            _document = document;
        }

        /// <summary>
        /// Gets or sets the document to un-check out.
        /// </summary>
        public Document Document
        {
            get { return _document; }
            set { _document = value; }
        }

        /// <summary>
        /// Gets or sets a parameter used with source control to undo check out of a file that is checked out by some other user. The default value is false.
        /// </summary>
        public bool Force
        {
            get { return _force; }
            set { _force = value; }
        }

        /// <summary>
        /// The time and date at which the current object was last checked out of the source control service. 
        /// </summary>
        public DateTime TimeCheckedOut
        {
            get { return _timeCheckedOut; }
            set { _timeCheckedOut = value; }
        }

        /// <summary>
        /// Gets or sets the rlsshortterm parameter. True if there is a short-term lock on the file; otherwise, false. A short-term lock automatically expires after a set period of time. The default is false;
        /// </summary>
        public bool ReleaseShortTermLock
        {
            get { return _releaseShortTermLock; }
            set { _releaseShortTermLock = value; }
        }

        public override string MethodName
        {
            get { return "uncheckout document"; }
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
                if (_timeCheckedOut != DateTime.MaxValue)
                    _parameters.Add("time_checked_out", Utils.GetUTCDateString(_timeCheckedOut));
                else if (_document.TimeCheckedOut != DateTime.MinValue)
                    _parameters.Add("time_checked_out", Utils.GetUTCDateString(_document.TimeCheckedOut));
                _parameters.Add("rlsshortterm", _releaseShortTermLock);
                return _parameters;
            }
        }
    }
}
