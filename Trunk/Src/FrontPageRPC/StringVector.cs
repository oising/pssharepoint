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
using System.Text.RegularExpressions;

namespace HubKey.Net.FrontPageRPC
{
    public class StringVector: List<string>
    {
        static Regex escapeRegex = new Regex(@"[=#;\|\[\]\{\}\]]", RegexOptions.Compiled);

        public StringVector()
        {
        }

        public StringVector(params string[] strings)
        {
            Load(strings);
        }

        internal StringVector(params Parameter[] parameters)
        {
            foreach (Parameter p in parameters)
            {
                this.Add(p.ToString());
            }
        }

        public StringVector(IEnumerable<string> strings)
        {
            Load(strings);
        }

        protected void Load(IEnumerable<string> strings)
        {
            foreach (string s in strings)
            {
                string value = string.IsNullOrEmpty(s)? s: escapeRegex.Replace(s, delegate(Match match) { return @"\" + match.Groups[0].Value; });
                this.Add(value);
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}]", string.Join(";", this));
        }

        public static implicit operator string[](StringVector vector)
        {
            List<string> strings = new List<string>();
            vector.ForEach(delegate(string s) 
            { 
                strings.Add(s); 
            });
            return strings.ToArray();
        }

        public static StringVector Parse(string vectorString)
        {
            try
            {
                List<string> strings = new List<string>();
                StringBuilder sb = new StringBuilder();
                int j = 0;
                char lastChar = (char)0;
                if (vectorString.StartsWith("["))
                {
                    if (!vectorString.EndsWith("]")) throw new Exception();
                    vectorString = vectorString.Remove(0, 1).Remove(vectorString.Length - 2, 1);
                }
                for (int i = 0; i < vectorString.Length; i++)
                {
                    if (vectorString[i] == '[' && lastChar != '\\') j++;
                    if (vectorString[i] == ']' && vectorString[i - 1] != '\\') j--;
                    if (vectorString[i] == ';' && j == 0 && lastChar != '\\')
                    {
                        strings.Add(sb.ToString());
                        sb = new StringBuilder();
                    }
                    else
                        sb.Append(vectorString[i]);
                    lastChar = vectorString[i];
                }
                if (j != 0) throw new Exception();
                strings.Add(sb.ToString());
                return new StringVector(strings);
            }
            catch
            {
                throw new Exception("Invalid vector");
            }
        }
    }

    public class IntegerVector : List<int>
    {
        public IntegerVector(params int[] integers)
        {
            Load(integers);
        }

        public IntegerVector(string integerVector)
        {
            Load(Parse(integerVector));
        }

        public IntegerVector(IEnumerable<int> integers)
        {
            Load(integers);
        }

        public IntegerVector(IEnumerable<string> strings)
        {
            foreach (string s in strings)
                this.Add(Convert.ToInt32(s));
        }

        protected void Load(IEnumerable<int> integers)
        {
            foreach (int i in integers)
                this.Add(i);
        }

        public override string ToString()
        {
            List<string> strings = new List<string>();
            this.ForEach(delegate(int i) { strings.Add(i.ToString()); });
            return string.Format("[{0}]", string.Join(";", strings.ToArray()));
        }

        public static implicit operator string(IntegerVector vector)
        {
            return vector.ToString();
        }

        public static implicit operator int[](IntegerVector vector)
        {
            List<int> integers = new List<int>();
            vector.ForEach(delegate(int i) { integers.Add(i); });
            return integers.ToArray();
        }

        public static IntegerVector Parse(string vectorString)
        {
            try
            {
                if (vectorString.StartsWith("["))
                {
                    if (!vectorString.EndsWith("]")) throw new Exception();
                    vectorString = vectorString.Remove(0, 1).Remove(vectorString.Length - 2, 1);
                }
                return new IntegerVector(vectorString.Split(';'));
            }
            catch
            {
                throw new Exception("Invalid vector");
            }
        }

    }

}
