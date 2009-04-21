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
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace HubKey.Net.FrontPageRPC
{
    public class Folder : ITreeNode
    {
        string _webRelativeUrl;
        DocumentCollection _documents = new DocumentCollection();
        FolderCollection _folders = new FolderCollection();
        Folder _parent;
        string _displayName = null;
        string _webUrl;

        private MetaInfoCollection _metaInfo;

        public MetaInfoCollection MetaInfo
        {
            get
            {
                if (_metaInfo == null)
                    _metaInfo = new MetaInfoCollection();
                return _metaInfo;
            }
            set { _metaInfo = value; }
        }

        public string WebUrl
        {
            get 
            {
                if (IsChildWeb)
                    return Url;
                return _webUrl; 
            }
            set { _webUrl = value; }
        }

        public string SiteUrl
        {
            get {  return _webUrl; }
            set { _webUrl = value; }

        }

        public string Url
        {
            get { return Utils.CombineUrl( _webUrl, _webRelativeUrl); }
        }

        public string WebRelativeUrl
        {
            get 
            {
                if (IsChildWeb)
                    return "/";
                return _webRelativeUrl; 
            }
            set
            {
                value = _webRelativeUrl;
            }
        }

        public Folder Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public DocumentCollection Documents
        {
            get { return _documents; }
            set { _documents = value; }
        }

        public FolderCollection Folders
        {
            get { return _folders; }
            set { _folders = value; }
        }

        public string ParentFolderUrl
        {
            get
            {
                if (WebRelativeUrl.IndexOf('/') == -1)
                    return WebRelativeUrl == ""? null: "";
                return Utils.GetDirectoryName(WebRelativeUrl); // Path.GetDirectoryName(WebRelativeUrl).Replace('\\', '/');
            }
        }

        public string Name
        {
            get
            {
                return Utils.GetFileName(_webRelativeUrl ,false); //Path.GetFileName(WebRelativeUrl);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string DisplayName
        {
            get
            {
                if (_displayName != null)
                    return _displayName;
                return string.Format(ListTitle == null || ListTitle == Name ? "{0}" : "{0} ({1})", Name, ListTitle);
            }
            internal set
            {
                _displayName = value;
            }
        }

        public bool IsChildWeb
        {
            get
            {
                MetaInfo mi = this.MetaInfo["vti_ischildweb"];
                if (mi == null)
                    return false;
                return this.MetaInfo["vti_ischildweb"].GetValueAsBoolean();
            }
        }
        
        public ListTemplateType TemplateType
        {
            get
            {
                MetaInfo mi = this.MetaInfo["vti_listservertemplate"];
                if (mi == null)
                    return ListTemplateType.Folder;
                ListTemplateType result = (ListTemplateType)mi.GetValueAsInteger();
                if (!Enum.IsDefined(typeof(ListTemplateType), result))
                    return ListTemplateType.GenericList;
                return result;
            }
        }

        public string ListTitle
        {
            get
            {
                MetaInfo result = _metaInfo["vti_listtitle"];
                if (result == null)
                    return null;
                return result.GetValueAsString();
            }
        }

        public DateTime TimeLastModified
        {
            get
            {
                MetaInfo dt = _metaInfo["vti_timelastmodified"];
                if (dt == null)
                    return DateTime.MinValue;
                return dt.GetValueAsDateTime();
            }
        }

        public DateTime TimeCreated
        {
            get
            {
                MetaInfo dt = _metaInfo["vti_timecreated"];
                if (dt == null)
                    return DateTime.MinValue;
                return dt.GetValueAsDateTime();
            }
        }

        public Folder(string webRelativeUrl)
        {
            Load(webRelativeUrl, null);
        }

        public Folder(string webRelativeUrl, MetaInfoCollection metaInfo)
        {
            Load(webRelativeUrl, metaInfo);
        }

        internal Folder(XPathNavigator rpcResponseNav)
        {
            if (rpcResponseNav != null)
                Load(rpcResponseNav.SelectSingleNode("li[@name='url']").Value, new MetaInfoCollection(rpcResponseNav.SelectSingleNode("ul[@name='meta_info']")));
        }


        protected void Load(string webRelativeUrl, MetaInfoCollection metaInfo)
        {
            _webRelativeUrl = webRelativeUrl;
            MetaInfo = metaInfo;
        }

        public override string ToString()
        {
            return Utils.CombineUrl(Url, "");
        }

        public object _icon;
        public object Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        public string IconName
        {
            get { return this.IsChildWeb ? "SubWeb" : this.TemplateType.ToString(); }
        }
    }

    

}
