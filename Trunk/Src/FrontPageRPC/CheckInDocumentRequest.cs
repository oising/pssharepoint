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
    public class CheckInDocumentRequest : RequestBase
    {
        Document _document;
        bool _keepCheckedOut = false;
        string _comment;
        DateTime _timeCheckedOut = DateTime.MinValue;

        /// <summary>
        /// Returns a file to source control.
        /// </summary>
        /// <param name="webUrl">The web site URL.</param>
        public CheckInDocumentRequest(string webUrl, string documentWebRelativeUrl)
        {
            this.WebUrl = webUrl;
            _document = new Document(documentWebRelativeUrl);
        }

        /// <summary>
        /// Returns a file to source control.
        /// </summary>
        /// <param name="document">The document to checkin.</param>
        public CheckInDocumentRequest(Document document)
        {
            _document = document;
        }

        /// <summary>
        /// Gets or sets the document to checkin.
        /// </summary>
        public Document Document
        {
            get { return _document; }
            set { _document = value; }
        }

        /// <summary>
        /// Gets or sets a parameter used when source control is in use. True to check in the specified document to source control and immediately check it back out. False to only check the document in to source control.
        /// </summary>
        public bool KeepCheckedOut
        {
            get { return _keepCheckedOut; }
            set { _keepCheckedOut = value; }
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
        /// Gets or sets a comment to store in the source control system. 
        /// </summary>
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        public override string MethodName
        {
            get { return "checkin document"; }
        }

        internal override ParameterCollection Parameters
        {
            get
            {
                _parameters = new ParameterCollection();
                
                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");
                _parameters.Add("document_name", _document.WebRelativeUrl);
                _parameters.Add("keep_checked_out", _keepCheckedOut);
                if (_timeCheckedOut != DateTime.MaxValue)
                    _parameters.Add("time_checked_out", Utils.GetUTCDateString(_timeCheckedOut));
                else if (_document.TimeCheckedOut != DateTime.MinValue)
                    _parameters.Add("time_checked_out", Utils.GetUTCDateString(_document.TimeCheckedOut));
                _parameters.Add("comment", _comment);
                return _parameters;
            }
        }
    }
}
