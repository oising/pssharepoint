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
    public class FolderCollection : List<Folder>
    {
        public FolderCollection()
        { }

        internal FolderCollection(XPathNavigator rpcResponseNav)
            : this(rpcResponseNav, null)
        {
        }

        internal FolderCollection(XPathNavigator rpcResponseNav, string webUrl)
            : this(rpcResponseNav, "urldirs", webUrl)
        {

        }

        internal FolderCollection(XPathNavigator rpcResponseNav, string elementName, string webUrl)
        {
            if (rpcResponseNav != null)
            {
                XPathNodeIterator nodes = rpcResponseNav.Select("//ul[@name='" + elementName + "']/ul");
                while (nodes.MoveNext())
                {
                    Folder folder = new Folder(nodes.Current);
                    folder.WebUrl = webUrl;
                    this.Add(folder);
                }
            }
        }

        public Folder this[string webRelativeUrl]
        {
            get
            {
                return base.Find(delegate(Folder folder) { return folder.WebRelativeUrl == webRelativeUrl; });
            }
        }

        public FolderCollection ShallowClone()
        {
            FolderCollection clone = new FolderCollection();
            foreach (Folder folder in this)
            {
                clone.Add(folder);
            }
            return clone;
        }


    }
}