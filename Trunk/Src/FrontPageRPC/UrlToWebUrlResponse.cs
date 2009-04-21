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
    public class UrlToWebUrlResponse : ResponseBase
    {
        private string _webUrl;
        private string _siteUrl;
        private string _subWebUrl;
        private string _fileUrl;
        private string _url;
        private string _rootFolder;
        private string _fileName;

        UrlToWebUrlRequest _request;

        internal UrlToWebUrlResponse(WebClient wc, UrlToWebUrlRequest request)
            : base(wc)
        {
            _request = request;
        }

        /// <summary>
        /// Gets the web site url.
        /// </summary>
        public string WebUrl
        {
            get { return _webUrl; }
        }

        /// <summary>
        /// Gets the top most web site url.
        /// </summary>
        public string SiteUrl
        {
            get { return _siteUrl; }
        }

        /// <summary>
        /// Gets the web site url relative to the site url.
        /// </summary>
        public string SubWebUrl
        {
            get { return _subWebUrl; }
        }

        /// <summary>
        /// Gets the file url relative to the web url.
        /// </summary>
        public string FileUrl
        {
            get { return _fileUrl; }
        }

        /// <summary>
        /// Gets the full file url.
        /// </summary>
        public string Url
        {
            get { return _url; }
        }

        /// <summary>
        /// Gets the root folder value of the url if available, otherwise the folder of a file url, not including the 'Forms' folder.
        /// </summary>
        public string RootFolder
        {
            get { return _rootFolder; }
        }

        /// <summary>
        /// Gets the full root folder (including the web url) value of the url if available, otherwise the folder of a file url, not including the 'Forms' folder.
        /// </summary>
        public string FullRootFolder
        {
            get { return Utils.CombineUrl(_siteUrl, _rootFolder); }
        }

        /// <summary>
        /// Gets the file name of the url if available.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
        }

        /// <summary>
        /// Gets whether the url is a file url.
        /// </summary>
        public bool IsFile
        {
            get { return !string.IsNullOrEmpty(_fileName); }
        }

        internal override void ReadResponse(Stream response, long responseLength)
        {
            base.ReadResponse(response, responseLength);

            _subWebUrl = base._nav.SelectSingleNode("//p[@name='webUrl']").Value;
            _fileUrl = base._nav.SelectSingleNode("//p[@name='fileUrl']").Value;
            _siteUrl = _request.WebUrl;
            _webUrl = _siteUrl + _subWebUrl;
            _url = Utils.CombineUrl(_webUrl, _fileUrl);
            _rootFolder = Utils.ParseRootFolder(_fileUrl);
            if (_rootFolder.StartsWith(_webUrl))
                _rootFolder = _rootFolder.Remove(0, _webUrl.Length);
            _fileName = Utils.GetFileName(_fileUrl, true);

        }
    }
}

