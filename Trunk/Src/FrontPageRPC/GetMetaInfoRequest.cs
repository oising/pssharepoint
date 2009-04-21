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
    public class GetMetaInfoRequest : RequestBase
    {
        private bool _listHiddenDocs = false;
        private bool _listLinkInfo = true;
        private List<ITreeNode> _nodes;

        /// <summary>
        /// Provides meta-information for the specified files in the current Web site.
        /// </summary>
        public GetMetaInfoRequest()
        {
        }

        /// <summary>
        /// Provides meta-information for the specified files in the current Web site.
        /// </summary>
        /// <param name="documentOrFolder">The document or folder to to obtain meta-information for.</param>
        public GetMetaInfoRequest(ITreeNode documentOrFolder)
        {
            TreeNodes.Add(documentOrFolder);
        }

        /// <summary>
        /// Provides meta-information for the specified files in the current Web site.
        /// </summary>
        /// <param name="document">The document to obtain meta-information for.</param>
        public GetMetaInfoRequest(Document document)
        {
            TreeNodes.Add(document);
        }

        /// <summary>
        /// Provides meta-information for the specified files in the current Web site.
        /// </summary>
        /// <param name="documents">The documents to obtain meta-information for.</param>
        public GetMetaInfoRequest(DocumentCollection documents)
        {
            foreach (Document document in documents)
            {
                TreeNodes.Add(document);
            }
        }

        /// <summary>
        /// Provides meta-information for the specified files in the current Web site.
        /// </summary>
        /// <param name="folder">The folder to obtain meta-information for.</param>
        public GetMetaInfoRequest(Folder folder)
        {
            TreeNodes.Add(folder);
        }

        /// <summary>
        /// Provides meta-information for the specified files in the current Web site.
        /// </summary>
        /// <param name="folders">The folders to obtain meta-information for.</param>
        public GetMetaInfoRequest(FolderCollection folders)
        {
            foreach (Folder folder in folders)
            {
                TreeNodes.Add(folder);
            }
        }

        /// <summary>
        /// Gets or sets the documents or folders (ITreeNode)s to obtain meta-information for.
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
        public override string MethodName
        {
            get { return "getDocsMetaInfo"; }
        }

        internal override ParameterCollection Parameters
        {
            get
            {
                _parameters = new ParameterCollection();
                
                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");

                List<string> webRelativeUrlList = new List<string>();
                foreach (ITreeNode node in _nodes)
                {
                    webRelativeUrlList.Add(node.WebRelativeUrl);
                }

                _parameters.Add("url_list", new StringVector(webRelativeUrlList));
                _parameters.Add("listHiddenDocs", _listHiddenDocs);
                _parameters.Add("listLinkInfo", _listLinkInfo);

                return _parameters;
            }
        }
    }
}
