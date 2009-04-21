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
    [Serializable]
    public class MetaInfoCollection : List<MetaInfo>
    {
        static Regex metaVectorRegex = new Regex(@"(?<name>[A-Za-z0-9\-:#_\{\}\s]+);(?<type>[B|D|E|F|I|L|S|T|U|V])(?<access>[R|X|W])\|(?<value>[^;]*)", RegexOptions.Compiled);
        static XmlSerializer _serializer;

        public MetaInfoCollection()
        {
        }

        internal MetaInfoCollection(XPathNavigator rpcResponseNav)
        {
            if (rpcResponseNav != null)
            {
                XPathNodeIterator nodes = rpcResponseNav.Select("li");
                while (nodes.MoveNext())
                {
                    string name = nodes.Current.Value;
                    nodes.MoveNext();
                    this.Add(new MetaInfo(name + ";" + nodes.Current.Value));
                }
            }
            SetValueChanged(false);
        }

        public MetaInfoCollection(string metaString)
        {
            foreach (Match match in metaVectorRegex.Matches(metaString))
                this.Add(new MetaInfo(match));
        }

        static XmlSerializer Serializer
        {
            get
            {
                if (_serializer == null)
                    _serializer = new XmlSerializer(typeof(MetaInfoCollection));
                return _serializer;
            }
        }

        public void Add(string name)
        {
            Add(name, null);
        }

        public void Add(string name, object value)
        {
            this.Add(new MetaInfo(name, value));
        }

        public void Add(string name, MetaTypeEnum type, object value)
        {
            this.Add(new MetaInfo(name, type, value));
        }

        public bool Remove(string name)
        {
            MetaInfo mi = base.Find(delegate(MetaInfo metaInfo) { return metaInfo.Name == name; });
            if (mi == null)
                throw new ArgumentOutOfRangeException("name", string.Format("MetaInfo name '{0}' was not found.", name));
            return base.Remove(mi);
        }

        public MetaInfo this[string name]
        {
            get
            {
                return base.Find(delegate(MetaInfo metaInfo) { return metaInfo.Name == name; });
            }
            set
            {
                MetaInfo result = this[name];
                if (result == null)
                    this.Add(value);
                else
                    result = value;
            }
        }

        public string Xml
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter w = XmlWriter.Create(sb, settings);
                Serializer.Serialize(w, this);
                return sb.ToString();
            }
        }

        internal void SetValueChanged(bool valueChanged)
        {
            foreach (MetaInfo property in this)
                property.ValueChanged = valueChanged;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        public string ToString(bool changedOnly)
        {
            List<string> strings = new List<string>();
            foreach (MetaInfo property in this)
            {
                if (!changedOnly || property.ValueChanged)
                    strings.Add(property.ToString(EscapeOption.Escape));
            }
            return string.Format("[{0}]", string.Join(";", strings.ToArray()));
        }

    }
}