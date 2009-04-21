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
    public class RemoveDocumentsRequest : RequestBase
    {
        private List<ITreeNode> _nodes;

        /// <summary>
        /// Provides a method to delete a specific documents or folders from the Web site.
        /// </summary>
        public RemoveDocumentsRequest()
        {
        }

        /// <summary>
        /// Provides a method to delete a specific document from the Web site.
        /// </summary>
        /// <param name="document">The document to be removed from the server.</param>
        public RemoveDocumentsRequest(Document document)
        {
            TreeNodes.Add(document);
        }

        /// <summary>
        /// Provides a method to delete specific documents from the Web site.
        /// </summary>
        /// <param name="documents">A collection of documents to be removed from the server.</param>
        public RemoveDocumentsRequest(DocumentCollection documents)
        {
            foreach (Document document in documents)
            {
                TreeNodes.Add(document);
            }
        }

        /// <summary>
        /// Provides a method to delete a specific folder from the Web site.
        /// </summary>
        /// <param name="folder">The folder to be removed from the server.</param>
        public RemoveDocumentsRequest(Folder folder)
        {
            TreeNodes.Add(folder);
        }

        /// <summary>
        /// Provides a method to delete specific folders from the Web site.
        /// </summary>
        /// <param name="folders">A collection of folders to be removed from the server.</param>
        public RemoveDocumentsRequest(FolderCollection folders)
        {
            foreach (Folder folder in folders)
            {
                TreeNodes.Add(folder);
            }
        }

        /// <summary>
        /// Provides a method to delete specific documents or folders from the Web site.
        /// </summary>
        /// <param name="fullWebRelativeNames">A list of web relative file or folder names.</param>
        public RemoveDocumentsRequest(IEnumerable<string> fullWebRelativeNames)
        {
            foreach (string url in fullWebRelativeNames)
            {
                if (Path.GetExtension(url.TrimEnd('/')) == "")
                    TreeNodes.Add(new Folder(url));
                else
                    TreeNodes.Add(new Document(url));
            }
            
        }

        /// <summary>
        /// Provides a method to delete specific documents or folders from the Web site.
        /// </summary>
        /// <param name="fullWebRelativeNames">A list of web relative file or folder names.</param>
        public RemoveDocumentsRequest(params string[] fullWebRelativeNames)
        {
            foreach (string url in fullWebRelativeNames)
            {
                if (Path.GetExtension(url.TrimEnd('/')) == "")
                    TreeNodes.Add(new Folder(url));
                else
                    TreeNodes.Add(new Document(url));
            }

        }

        /// <summary>
        /// Provides a method to delete specific documents or folders from the Web site.
        /// </summary>
        /// <param name="treeNodes">A list of ITreeNode (document or folder) objects.</param>
        public RemoveDocumentsRequest(List<ITreeNode> treeNodes)
        {
            _nodes = treeNodes;
        }

        /// <summary>
        /// Gets or sets the documents or folders (ITreeNode)s to be deleted.
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
            get { return "remove documents"; }
        }

        internal override ParameterCollection Parameters
        {
            get
            {
                _parameters = new ParameterCollection();
                
                
                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");

                List<string> webRelativeUrls = new List<string>();
                foreach (ITreeNode node in _nodes)
                {
                    webRelativeUrls.Add(node.WebRelativeUrl);
                }

                _parameters.Add("url_list", new StringVector(webRelativeUrls));

                return _parameters;
            }
        }
    }
}
