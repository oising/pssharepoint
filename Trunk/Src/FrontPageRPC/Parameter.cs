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
using System.Web;
using System.Text.RegularExpressions;

namespace HubKey.Net.FrontPageRPC
{
    internal class ParameterCollection : List<Parameter>
    {
        
        public ParameterCollection()
        {
        }

        public void Add(string name, object value)
        {
            this.Add(new Parameter(name, value));
        }

        public override string ToString()
        {
            List<string> strings = new List<string>();
            foreach (Parameter parameter in this)
            {
                strings.Add(parameter.ToString(true)); 
            }
            return string.Join("&", strings.ToArray());
        }

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(this.ToString());
        }

    }

    internal class Parameter
    {
        static Regex escapeRegex = new Regex(@"[=;\|\[\]\{\}\]]", RegexOptions.Compiled);
        public string Name;
        private object _value;

        public Parameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public object Value
        {
            get { return _value; }
            set 
            {
                if (value is string && value != null)
                    value = escapeRegex.Replace(value.ToString(), delegate(Match match) { return @"\" + match.Groups[0].Value; });
                _value = value; 
            }
        }

        public string ValueAsString()
        {
            if (_value == null) return "";
            if (_value is bool) return _value.ToString().ToLower();
            string result = _value.ToString();
            return result;
        }

        public override string ToString()
        {
            return this.ToString(false);
        }

        public string ToString(bool urlEncode)
        {
            string name = Name;
            string value = ValueAsString();
            if (urlEncode)
            {
                name = HttpUtility.UrlEncode(name);
                value = HttpUtility.UrlEncode(value);
            }
            string result = string.Format("{0}={1}", name, value);
            return result;
        }

        public static implicit operator string(Parameter parameter)
        {
            return parameter.ToString();
        }
    }
}
