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
    public class DocumentCollection : List<Document>
    {
        public DocumentCollection()
        { }

        public DocumentCollection(Document document)
        {
            this.Add(document);
        }

        internal DocumentCollection(XPathNavigator rpcResponseNav)
            : this(rpcResponseNav, "document_list", null)
        {
        }

        internal DocumentCollection(XPathNavigator rpcResponseNav, string webUrl)
            : this(rpcResponseNav, "document_list", webUrl)
        {
        }

        internal DocumentCollection(XPathNavigator rpcResponseNav, string elementName, string webUrl)
        {
            if (rpcResponseNav != null)
            {
                XPathNodeIterator nodes = rpcResponseNav.Select("//ul[@name='" + elementName + "']/ul");
                while (nodes.MoveNext())
                {
                    Document document = new Document(nodes.Current, null);
                    document.WebUrl = webUrl;
                    this.Add(document);
                }
            }
        }

        public DocumentCollection ShallowClone()
        {
            DocumentCollection clone = new DocumentCollection();
            foreach (Document document in this)
            {
                clone.Add(document);
            }
            return clone;
        }

        public DocumentCollection(IEnumerable<string> fullWebRelativeNames)
        {
            foreach (string fullWebRelativeName in fullWebRelativeNames)
            {
                this.Add(new Document(fullWebRelativeName));
            }
        }

        public List<string> FullWebRelativeNames
        {
            get
            {
                List<string> result = new List<string>();
                this.ForEach(delegate(Document document) { result.Add(document.WebRelativeUrl); });
                return result;
            }
        }

        public Document this[string name]
        {
            get
            {
                return base.Find(delegate(Document document) { return document.WebRelativeUrl == name; });
            }
        }

        public void SaveMetaInfo()
        {
            SaveMetaInfo(null);
        }

        public void SaveMetaInfo(DirectoryInfo directory)
        {
            this.ForEach(delegate(Document document) { document.SaveMetaInfo(directory); });
        }

        public int OkFilesCount
        {
            get
            {
                int i = 0;
                foreach (Document document in this)
                    i += (document.ServerFileStatusIsOk ? 1 : 0);
                return i;
            }
        }

    }

    public static class RemoveItems<T>
    {

        public delegate bool Decide(T item);

        public static void Execute(ICollection<T> collection, Decide decide)
        {
            List<T> removed = new List<T>();
            foreach (T item in collection)
                if (decide(item)) removed.Add(item);
            foreach (T item in removed)
                collection.Remove(item);
            removed.Clear();
        }
    } 


}