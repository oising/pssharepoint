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
        public class ServerInfo
        {
            static Regex infRegex = new Regex(@"FrontPage Configuration Information\s*\n\s*FPVersion=""(?<version>[^""]+)""\s*\n\s*FPShtmlScriptUrl=""(?<shtmlScriptUrl>[^""]+)""\s*\n\s*FPAuthorScriptUrl=""(?<authorScriptUrl>[^""]+)""\s*\n\s*FPAdminScriptUrl=""(?<adminScriptUrl>[^""]+)""\s*\n\s*TPScriptUrl=""(?<scriptUrl>[^""]+)""\s*\n\s*", RegexOptions.Compiled);

            public string _version;
            public string _shtmlScriptUrl;
            public string _authorScriptUrl;
            public string _adminScriptUrl;
            private string _scriptUrl;

            public ServerInfo() { }

            public ServerInfo(string version, string shtmlScriptUrl, string authorScriptUrl, string adminScriptUrl, string scriptUrl)
            {
                _version = version;
                _shtmlScriptUrl = shtmlScriptUrl;
                _authorScriptUrl = authorScriptUrl;
                _adminScriptUrl = adminScriptUrl;
                _scriptUrl = scriptUrl;
            }

            internal ServerInfo(string html)
            {
                Match match = infRegex.Match(html);
                if (match.Success)
                {
                    Version = match.Groups["version"].Value;
                    ShtmlScriptUrl = match.Groups["shtmlScriptUrl"].Value;
                    AuthorScriptUrl = match.Groups["authorScriptUrl"].Value;
                    AdminScriptUrl = match.Groups["adminScriptUrl"].Value;
                    ScriptUrl = match.Groups["scriptUrl"].Value;
                }
                else
                    throw new Exception("Could not determine FrontPage version info from _vti_inf.html");
            }

            /// <summary>
            /// Gets or sets the FrontPage version. Returns the version from _vti_inf.html if not set before a request is made.
            /// </summary>
            public string Version
            {
                get { return _version; }
                set { _version = value; }
            }

            /// <summary>
            /// Gets or sets FPShtmlScriptUrl. Returns FPShtmlScriptUrl from _vti_inf.html if not set before a request is made.
            /// </summary>
            public string ShtmlScriptUrl
            {
                get { return _shtmlScriptUrl; }
                set { _shtmlScriptUrl = value; }
            }

            /// <summary>
            /// Gets or sets FPAuthorScriptUrl. Returns FPAuthorScriptUrl from _vti_inf.html if not set before a request is made.
            /// </summary>
            public string AuthorScriptUrl
            {
                get { return _authorScriptUrl; }
                set { _authorScriptUrl = value; }
            }

            /// <summary>
            /// Gets or sets FPAdminScriptUrl. Returns FPAdminScriptUrl from _vti_inf.html if not set before a request is made.
            /// </summary>
            public string AdminScriptUrl
            {
                get { return _adminScriptUrl; }
                set { _adminScriptUrl = value; }
            }

            /// <summary>
            /// Gets or sets TPScriptUrl. Returns TPScriptUrl from _vti_inf.html if not set before a request is made.
            /// </summary>
            public string ScriptUrl
            {
                get { return _scriptUrl; }
                set { _scriptUrl = value; }
            }


        }
}
