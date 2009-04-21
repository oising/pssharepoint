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
    public class ListDocumentsRequest : RequestBase
    {
        private bool _listRecurse = true;
        private bool _listExplorerDocs = true;
        private bool _listHiddenDocs = true;
        private bool _listFiles = true;
        private bool _listFolders = true;
        private bool _listLinkInfo = true;
        private bool _listIncludeParent = true;
        private bool _listDerived = false;
        private bool _listBorders = false;
        private bool _listChildWebs = true;
        private bool _listThickets = false;
        private string _initialUrl = null;
        private string _platform = null;
        private DateTime _fileMetaInfoTimeStamp = DateTime.MinValue;

        /// <summary>
        /// Provides a method to request a list of files, folders, and Web sites contained in a given folder.
        /// </summary>
        public ListDocumentsRequest()
        {
        }

        public override string MethodName
        {
            get { return "list documents"; }
        }

        /// <summary>
        /// Gets or sets whehter to recursively list the subfolders of the web site in the return value.
        /// </summary>
        public bool ListRecurse
        {
            get { return _listRecurse; }
            set { _listRecurse = value; }
        }

        /// <summary>
        /// Gets or sets the listExplorerDocs parameter. True to generate a list of the task list files (_x_todo.xml and _x_todh.xml). No merging takes place with any task list on the destination Web site. The originating server replaces the existing task list on the destination site with its own task list. If false, no task list data is sent from the originating server.
        /// </summary>
        public bool ListExplorerDocs
        {
            get { return _listExplorerDocs; }
            set { _listExplorerDocs = value; }
        }

        /// <summary>
        /// Gets or sets whether to list hidden documents in a Web site. Note that hidden documents are files and folders whose URLs contain a path component that begins with an underscore.
        /// </summary>
        public bool ListHiddenDocs
        {
            get { return _listHiddenDocs; }
            set { _listHiddenDocs = value; }
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
        /// Gets or sets whether to list the names and metadata of the folders.
        /// </summary>
        public bool ListFolders
        {
            get { return _listFolders; }
            set { _listFolders = value; }
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
        /// Gets or sets the listIncludeParent parameter. If this parameter is set to true, an entry for the InitialUrl parameter is returned. If set to false, no entry is returned for the InitialUrl parameter. 
        /// </summary>
        public bool ListIncludeParent
        {
            get { return _listIncludeParent; }
            set { _listIncludeParent = value; }
        }

        /// <summary>
        /// Gets or sets the listDerived parameter. Sending this parameter generates a list of files in _derived folders. FrontPage Server Extensions from Microsoft generates _derived folders dynamically. These files can be regenerated at any time and include .htx and .gif files, such as files created by the search component, and composite text for use with GIF images, such as those used for theme buttons.
        /// </summary>
        public bool ListDerived
        {
            get { return _listDerived; }
            set { _listDerived = value; }
        }

        /// <summary>
        /// Gets or sets the listBorders parameter. True to generate a list of contents of the _borders directory that contains shared border pages; otherwise, false. 
        /// </summary>
        public bool ListBorders
        {
            get { return _listBorders; }
            set { _listBorders = value; }
        }

        /// <summary>
        /// Gets or sets the listChildWebs parameter. If the value is set true, the return value includes the names of folders for subsites.
        /// </summary>
        public bool ListChildWebs
        {
            get { return _listChildWebs; }
            set { _listChildWebs = value; }
        }

        /// <summary>
        /// Gets or sets a boolean value that specifies if thicket supporting files and folders should be included in the server response.
        /// </summary>
        public bool ListThickets
        {
            get { return _listThickets; }
            set { _listThickets = value; }
        }

        /// <summary>
        /// Gets or sets the URL of the folder from which to initially list documents or, if no folder is given, either "" or "/" to indicate the root folder of the Web site. 
        /// </summary>
        public string InitialUrl
        {
            get { return _initialUrl; }
            set { _initialUrl = value; }
        }

        /// <summary>
        /// Gets or sets the operating system of the client that controls the listing of open Microsoft FrontPage components (bots). If the parameter does not contain any data, the information about the components is not returned. If it contains data, information about any open components on the server is returned.
        /// </summary>
        public string Platform
        {
            get { return _platform; }
            set { _platform = value; }
        }

        /// <summary>
        /// Gets or sets the timestamp for the folders list. See http://msdn.microsoft.com/en-us/library/cc205167.aspx.
        /// </summary>
        public DateTime FileMetaInfoTimeStamp
        {
            get { return _fileMetaInfoTimeStamp; }
            set { _fileMetaInfoTimeStamp = value; }
        }

        internal override ParameterCollection Parameters
        {
            get
            {
                _parameters = new ParameterCollection();

                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");

                _parameters.Add("name", null);
                _parameters.Add("listHiddenDocs", _listHiddenDocs);
                _parameters.Add("listExplorerDocs", _listExplorerDocs);
                _parameters.Add("listRecurse", _listRecurse);
                _parameters.Add("listFiles", _listFiles);
                _parameters.Add("listFolders", _listFolders);
                _parameters.Add("listLinkInfo", _listLinkInfo);
                _parameters.Add("listIncludeParent", _listIncludeParent);
                _parameters.Add("listDerived", _listDerived);
                _parameters.Add("listBorders", _listBorders);
                _parameters.Add("listChildWebs", _listChildWebs);
                _parameters.Add("listThickets", _listThickets);
                _parameters.Add("initialUrl", _initialUrl);
                _parameters.Add("platform", _platform); // e.g. "WinI386"
                string metaTime = _fileMetaInfoTimeStamp == DateTime.MinValue? "": "TW|" + Utils.GetUTCDateString(_fileMetaInfoTimeStamp);
                StringVector folderList = new StringVector(_initialUrl, metaTime);
                _parameters.Add("folderList", folderList); 

                return _parameters;
            }
        }

    }
}
