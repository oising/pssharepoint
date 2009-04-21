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

    public class MetaInfo : ISerializable
    {
        static Regex unescapeRegex = new Regex(@"\\[\\r\\n\\b\\f\\t\\v]", RegexOptions.Compiled);
        static Regex metaRegex = new Regex(@"^(?<name>[A-Za-z0-9\-:#_\{\}\s]+);(?<type>[B|D|E|F|I|L|S|T|U|V])(?<access>[R|X|W])\|(?<value>.*)?", RegexOptions.Singleline | RegexOptions.Compiled);
        private MetaAccessEnum _access;
        private object _value;
        private string _name;
        private MetaTypeEnum _type;
        protected string _rawString;
        bool _valueChanged = false;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", this._name);
            info.AddValue("Type", this._type.ToString());
            info.AddValue("Access", this._access.ToString());
            info.AddValue("Value", this.Value);
        }

        public MetaInfo()
        {
            _access = MetaAccessEnum.ReadWrite;
            this._type = MetaTypeEnum.String;
        }

        public MetaInfo(string metaString)
            : this(metaRegex.Match(metaString))
        {
            _rawString = metaString;
            if (this.Value == null)
            {
                this._name = "Unknown";
                this.Value = _rawString;
            }
        }

        public MetaInfo(Match match)
            : this()
        {
            if (match.Success)
            {
                this._name = match.Groups["name"].Value;
                this._type = (MetaTypeEnum)match.Groups["type"].Value[0];
                this._access = (MetaAccessEnum)match.Groups["access"].Value[0];
                string s = match.Groups["value"].Value;
                s = UnescapeString(s);
                this.Value = s;

            }
        }

        public MetaInfo(string name, object value)
            : this(name, MetaTypeEnum.String, value)
        {
        }

        public MetaInfo(string name, MetaTypeEnum type, object value)
            : this()
        {
            this._name = name;
            this.Value = value;
            this._type = type;
        }

        public bool ValueChanged
        {
            get { return _valueChanged; }
            internal set { _valueChanged = value; }
        }

        public MetaAccessEnum Access
        {
            get { return _access; }
            set { _access = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public MetaTypeEnum Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public object Value
        {
            get { return GetValueAsString(); }
            set
            {
                this._valueChanged = true;
                this._value = value;
                try
                {
                    switch (this._type)
                    {
                        case MetaTypeEnum.Double:
                            if (!(this._value is double)) this._value = Double.Parse(value.ToString());
                            break;
                        case MetaTypeEnum.Boolean:
                            if (!(this._value is bool)) this._value = Boolean.Parse(value.ToString());
                            break;
                        case MetaTypeEnum.Integer:
                            if (!(this._value is int)) this._value = Int32.Parse(value.ToString());
                            break;
                        case MetaTypeEnum.Time:
                            if (!(this._value is DateTime)) this._value = DateTime.Parse(value.ToString());
                            break;
                        case MetaTypeEnum.StringVector:
                            if (!(this._value is StringVector)) this._value = StringVector.Parse(value.ToString());
                            break;
                        case MetaTypeEnum.IntegerVector:
                            if (!(this._value is IntegerVector)) this._value = new IntegerVector(value.ToString());
                            break;
                        case MetaTypeEnum.LongText:
                        case MetaTypeEnum.String:
                            this._value = value.ToString();
                            break;
                    }
                }
                catch { }
                if ((this._value is int) || (this._value is short) || (this._value is long))
                    this._type = MetaTypeEnum.Integer;
                else if (this._value is double)
                    this._type = MetaTypeEnum.Double;
                else if (this._value is bool)
                    this._type = MetaTypeEnum.Boolean;
                else if (this._value is DateTime)
                    this._type = MetaTypeEnum.Time;
                else if (this._value is StringVector)
                    this._type = MetaTypeEnum.StringVector;
                else if (this._value is DateTime)
                    this._type = MetaTypeEnum.IntegerVector;
            }
        }

        static string EscapeVectorChars(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                int i = (int)c;
                switch (c)
                {
                    case ';':
                    case '=':
                    case '[':
                    case ']':
                    case '\\':
                        sb.Append("\\");
                        break;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        public string GetValueAsString()
        {
            return GetValueAsString(EscapeOption.Unescape);
        }

        public string GetValueAsString(EscapeOption escape)
        {
            if (_value == null)
                return ((char)0).ToString();
            if ((this._type == MetaTypeEnum.Time) || (this._value is DateTime))
                return ((DateTime)_value).ToString("s") + "Z";
            string s = _value.ToString();
            if (escape == EscapeOption.Escape)
            {
                s = EscapeVectorChars(s);
                return s;
            }
            return s;
        }

        internal string UnescapeString(string s)
        {
            byte[] b = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
                b[i] = (byte)s[i];
            s = Encoding.UTF8.GetString(b);
            return unescapeRegex.Replace(s, delegate(Match m)
            {
                switch (m.Groups[0].Value)
                {
                    case "\\r":
                        return "\r";
                    case "\\n":
                        return "\n";
                    case "\\b":
                        return "\b";
                    case "\\f":
                        return "\f";
                    case "\\t":
                        return "\t";
                    case "\\v":
                        return "\v";
                    default:
                        return "";
                };
            });
        }

        public double GetValueAsDouble()
        {
            if (this._value is double)
                return Double.Parse(_value.ToString());
            if (this._value is string)
            {
                double result;
                double.TryParse(this._value.ToString(), out result);
                return result;
            }
            throw new ArithmeticException(string.Format("Could not get value {0} as Double. {1} is data type {2}.", this._value, this._name, this._type));
        }

        public bool GetValueAsBoolean()
        {
            if (this._value is bool)
                return Boolean.Parse(_value.ToString());
            if (this._value is string)
            {
                bool result;
                bool.TryParse(this._value.ToString(), out result);
                return result;
            }
            throw new ArithmeticException(string.Format("Could not get value {0} as Boolean. {1} is data type {2}.", this._value, this._name, this._type));
        }

        public DateTime GetValueAsDateTime()
        {
            if (this._value is DateTime)
                return DateTime.Parse(_value.ToString());
            if (this._value is string)
            {
                DateTime result;
                DateTime.TryParse(this._value.ToString(), out result);
                return result;
            }
            throw new ArithmeticException(string.Format("Could not get value {0} as DateTime. {1} is data type {2}.", this._value, this._name, this._type));
        }

        public int GetValueAsInteger()
        {
            if ((this._value is int) || (this._value is short) || (this._value is long))
                return Int32.Parse(_value.ToString()); ;
            if (this._value is string)
            {
                int result;
                int.TryParse(this._value.ToString(), out result);
                return result;
            }
            throw new ArithmeticException(string.Format("Could not get value {0} as Integer. {1} is data type {2}.", this._value, this._name, this._type));
        }

        public string ToString(EscapeOption escape)
        {
            return string.Format("{0};{1}{2}|{3}", this._name, (char)this._type, (char)this._access, GetValueAsString(escape));
        }

        public override string ToString()
        {
            return this.ToString(EscapeOption.None);
        }

        public static explicit operator string(MetaInfo m)
        {
            return m.ToString(EscapeOption.None);
        }

        
    }

   
}
