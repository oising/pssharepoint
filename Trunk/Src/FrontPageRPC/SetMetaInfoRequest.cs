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
    public class SetMetaInfoRequest : RequestBase
    {
        private bool _listHiddenDocs = false;
        private bool _listLinkInfo = true;
        private bool _listFiles = true;
        private bool _updateAllMetaInfo = false;
        private ErrorFlagsEnum _errorFlags = ErrorFlagsEnum.StopOnFirst;
        private List<ITreeNode> _nodes;

        /// <summary>
        /// Provides a method to provide the server with the standard meta-information concerning the designated files or folders.
        /// </summary>
        public SetMetaInfoRequest()
        {
        }

        /// <summary>
        /// Provides a method to provide the server with the standard meta-information concerning the designated files or folders.
        /// </summary>
        /// <param name="documentOrFolder">The document or folder to update.</param>
        public SetMetaInfoRequest(ITreeNode documentOrFolder)
        {
            TreeNodes.Add(documentOrFolder);
        }

        /// <summary>
        /// Provides a method to provide the server with the standard meta-information concerning the designated files or folders.
        /// </summary>
        /// <param name="document">The document to update.</param>
        public SetMetaInfoRequest(Document document)
        {
            TreeNodes.Add(document);
        }

        /// <summary>
        /// Provides a method to provide the server with the standard meta-information concerning the designated files or folders.
        /// </summary>
        /// <param name="documents">The documents to update.</param>
        public SetMetaInfoRequest(DocumentCollection documents)
        {
            foreach (Document document in documents)
            {
                TreeNodes.Add(document);
            }
        }

        /// <summary>
        /// Provides a method to provide the server with the standard meta-information concerning the designated files or folders.
        /// </summary>
        /// <param name="folder">The folder to update.</param>
        public SetMetaInfoRequest(Folder folder)
        {
            TreeNodes.Add(folder);
        }

        /// <summary>
        /// Provides a method to provide the server with the standard meta-information concerning the designated files or folders.
        /// </summary>
        /// <param name="folders">The folders to update.</param>
        public SetMetaInfoRequest(FolderCollection folders)
        {
            foreach (Folder folder in folders)
            {
                TreeNodes.Add(folder);
            }
        }

        /// <summary>
        /// Gets or sets the documents or folders (ITreeNode)s to be updated.
        /// </summary>
        public List<ITreeNode> TreeNodes
        {
            get
            {
                if (_nodes == null)
                    _nodes = new List<ITreeNode>();
                return _nodes;
            }
            set { _nodes = value; }
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
        /// Gets or sets whether or not the return value of the method contains information about the links from the current page(s). Set true to include link information in the return value. 
        /// </summary>
        public bool ListLinkInfo
        {
            get { return _listLinkInfo; }
            set { _listLinkInfo = value; }
        }

        /// <summary>
        /// Gets or sets whether list the metadata of files contained in each directory represented in the return code. 
        /// </summary>
        public bool ListFiles
        {
            get { return _listFiles; }
            set { _listFiles = value; }
        }

        /// <summary>
        /// Gets or sets the action to take when an error occurs.
        /// </summary>
        public ErrorFlagsEnum ErrorFlags
        {
            get { return _errorFlags; }
            set { _errorFlags = value; }
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
            get { return "setDocsMetaInfo"; }
        }

        internal override ParameterCollection Parameters
        {
            get
            {
                _parameters = new ParameterCollection();

                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");

                List<string> fullWebRelativeNames = new List<string>();

                StringVector metaInfo = new StringVector();
                
                foreach (ITreeNode node in TreeNodes)
                {
                    if (_updateAllMetaInfo)
                        node.MetaInfo.SetValueChanged(true);

                    metaInfo.Add(node.MetaInfo.ToString(true));
                    fullWebRelativeNames.Add(node.WebRelativeUrl);
                }

                _parameters.Add("url_list", new StringVector(fullWebRelativeNames));
                _parameters.Add("metaInfoList", metaInfo);

                _parameters.Add("listHiddenDocs", _listHiddenDocs);
                _parameters.Add("listLinkInfo", _listLinkInfo);
                _parameters.Add("listFiles", _listFiles);
                
                _parameters.Add("errorFlags", Utils.GetErrorFlagsAsString(_errorFlags));
                
                return _parameters;
            }
        }

    }
}
