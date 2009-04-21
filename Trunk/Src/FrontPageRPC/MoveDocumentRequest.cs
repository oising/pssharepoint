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
    public class MoveDocumentRequest: RequestBase
    {
        private ITreeNode _node;
        private string _newUrl;
        private string _oldUrl;
        private RenameOptionEnum _renameOption = RenameOptionEnum.FindBacklinks;
        private PutOptionEnum _putOption = PutOptionEnum.Default | PutOptionEnum.MigrationSemantics;
        private bool _copy = false;
        private string _newWebUrl;
        private bool _ensureFolders;

        /// <summary>
        /// Provides a method to rename (move) or copy an existing file or folder.
        /// </summary>
        /// <param name="oldWebRelativeUrl">The web relative url of the document or folder to move or copy.</param>
        public MoveDocumentRequest(string oldWebRelativeUrl)
            : base()
        {
            this._oldUrl = oldWebRelativeUrl;
        }

        /// <summary>
        /// Provides a method to rename (move) or copy an existing file or folder.
        /// </summary>
        /// <param name="documentOrFolder">The document or folder to move or copy.</param>
        public MoveDocumentRequest(ITreeNode documentOrFolder)
            : base()
        {
            this._node = documentOrFolder;
            this.OldUrl = documentOrFolder.WebRelativeUrl;
            this.WebUrl = documentOrFolder.WebUrl;
        }

        /// <summary>
        /// Gets or sets a boolean value to ensure that all parent folders are created before the move request is made to the server. The createdirs putoption only creates the first parent folder if it does not exist.
        /// </summary>
        public bool EnsureFolders
        {
            get { return _ensureFolders; }
            set { _ensureFolders = value; }
        }

        /// <summary>
        /// Gets or sets the website URL of the destination document.
        /// </summary>
        public string NewWebUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_newWebUrl))
                    return WebUrl;
                return _newWebUrl;
            }
            set { _newWebUrl = value; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether the move is across webs and so will not use the 'move document' method.
        /// </summary>
        public bool IsCrossWebMove
        {
            get
            {
                return !NewWebUrl.Equals(WebUrl, StringComparison.OrdinalIgnoreCase);
            }
        }

        
        /// <summary>
        /// Gets or sets the web relative url to move or copy.
        /// </summary>
        public string OldUrl
        {
            get { return _oldUrl; }
            set { _oldUrl = value; }
        }

        /// <summary>
        /// Gets or sets the new web relative for the document.
        /// </summary>
        public string NewUrl
        {
            get { return _newUrl; }
            set { _newUrl = value; }
        }

        /// <summary>
        /// Gets or sets the document or folder to move or copy.
        /// </summary>
        public ITreeNode DocumentOrFolder
        {
            get { return _node; }
            set 
            {
                _oldUrl = value.WebRelativeUrl;
                _node = value; 
            }
        }

        /// <summary>
        /// Gets or sets a set of flags that tells the server extensions how to handle links to and from the new page. 
        /// </summary>
        public RenameOptionEnum RenameOption
        {
            get { return _renameOption; }
            set { _renameOption = value; }
        }

        /// <summary>
        /// Gets or sets a set of flags describing how the operation behaves. Enables the server to overwrite an existing file if the value is overwrite and disallows overwrites if the value edit.
        /// </summary>
        public PutOptionEnum PutOption
        {
            get { return _putOption; }
            set { _putOption = value; }
        }

        /// <summary>
        /// Gets or sets an optional parameter that specifies the action of the MoveDocument method. If the value is true, the method copies the file to the destination. If the value is false, the method moves the file to the destination. The default value is false.
        /// </summary>
        public bool Copy
        {
            get { return _copy; }
            set { _copy = value; }
        }

        public override string MethodName
        {
            get { return "move document"; }
        }

        internal override ParameterCollection Parameters
        {
            get 
            {
                _parameters = new ParameterCollection();
                _parameters.Add("method", string.Format("{0}:{1}", MethodName, Version));
                _parameters.Add("service_name", "/");

                _parameters.Add("oldUrl", _oldUrl);
                _parameters.Add("newUrl", _newUrl);
                _parameters.Add("put_option", Utils.GetPutOptionAsString(_putOption));
                _parameters.Add("rename_option", Utils.GetRenameOptionAsString(_renameOption));
                _parameters.Add("docopy", _copy);

                return _parameters; 
            }
        }

    }
}
