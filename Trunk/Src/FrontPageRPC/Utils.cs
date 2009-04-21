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
using System.Xml;
using System.Xml.XPath;
using System.Web;

namespace HubKey.Net.FrontPageRPC
{
    public static class Utils
    {
        static readonly string HtmlRoot = "://";

        public static string SafeNodeValue(XPathNavigator nav, string xpath)
        {
            if (nav == null)
                return null;
            return nav.SelectSingleNode(xpath).Value;
        }
        public static XPathNavigator SafeNode(XPathNavigator nav, string xpath)
        {
            if (nav == null)
                return null;
            return nav.SelectSingleNode(xpath);
        }

        public static string GetUTCDateString(DateTime date)
        {
            return date.ToUniversalTime().ToString("dd MMM yyyy hh:mm:ss -0000");
        }

        public static string GetExceptionMessage(ResponseBase response)
        {
            if (response == null)
                return "Response was null";
            if (!string.IsNullOrEmpty(response.ErrorMessage))
                return response.ErrorMessage;
            return GetExceptionMessage(response.ErrorResponse.Exception);
        }

        public static string GetExceptionMessage(Exception ex)
        {
            if (ex == null)
                return "No exception was returned.";
            FrontPageRPCException rpcException = ex as FrontPageRPCException;
            if (rpcException != null)
            {
                if (rpcException.FrontPageRPCError != null && !string.IsNullOrEmpty(rpcException.FrontPageRPCError.Msg))
                    return rpcException.FrontPageRPCError.Msg;
                else
                    return rpcException.Message;
            }
            else if (ex.InnerException == null)
                return ex.ToString();
            else
                return ex.ToString() + "\r\n\r\n" + ex.InnerException.ToString();
        }

        public static string GetSiteUrl(string url)
        {
            Uri uri = new Uri(url);
            return string.Format("{0}://{1}", uri.Scheme, uri.Authority);
        }

        public static string GetFileName(string url, bool urlDecode)
        {
            string dirName;
            string fileName;
            ParseUrl(url, out dirName, out fileName);
            if (!urlDecode)
                return fileName;
            return HttpUtility.UrlDecode(fileName);
        }

        public static string GetDirectoryName(string url)
        {
            return GetDirectoryName(url, false);
        }

        public static string GetDirectoryName(string url, bool urlDecode)
        {
            string dirName;
            string fileName;
            ParseUrl(url, out dirName, out fileName);
            if (!urlDecode)
                return dirName;
            return HttpUtility.UrlDecode(dirName);
        }

        public static string GetUrlKeyValue(string url, string keyName)
        {
            if (url == null)
                return null;
            int index = url.IndexOf("&" + keyName + "=", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                index = url.IndexOf("?" + keyName + "=", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return null;
            int end = url.IndexOf('&', index + 1);
            if (end < 0)
                end = url.Length;
            int start = index + keyName.Length + 2;
            return HttpUtility.UrlDecode(url.Substring(start, end - start));
        }

        public static string SplitAtFirstChar(string url, char splitChar)
        {
            int index = url.IndexOf(splitChar);
            if (index > -1)
            {
                return url.Substring(0, index);
            }
            else
            {
                return url;
            }
        }

        internal static void ParseUrl(string url, out string dirName, out string fileName)
        {
            url = SplitAtFirstChar(url, '?');
            if (url == null)
            {
                dirName = "";
                fileName = "";
                return;
            }
            url = url.TrimStart('/');
            int length = url.LastIndexOf('/');
            if (length != -1)
            {
                dirName = url.Substring(0, length);
                fileName = url.Substring(length + 1);
            }
            else
            {
                dirName = "";
                fileName = url.Length > 0? url: "";
            }
        }


        public static string ParseRootFolder(string url)
        {
            string parentFolder = GetDirectoryName(url);
            if (parentFolder.EndsWith("/Forms", StringComparison.OrdinalIgnoreCase))
                parentFolder = GetDirectoryName(parentFolder, true);
            string rootFolder = GetUrlKeyValue(url, "RootFolder");
            if (string.IsNullOrEmpty(rootFolder))
                return parentFolder;
            if (rootFolder.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return rootFolder;
            else
                return rootFolder.TrimStart('/');  //CombineUrl(parentFolder, rootFolder);
        }

        public static string CombineUrl(string path1, string path2)
        {
            return path1.TrimEnd('/') + '/' + path2.TrimStart('/');
        }

        public static string GetErrorFlagsAsString(ErrorFlagsEnum errorFlags)
        {
            if (errorFlags == ErrorFlagsEnum.StopOnFirst)
                return "stopOnFirst";
            if (errorFlags == ErrorFlagsEnum.KeepGoing)
                return "keepGoing";
            if (errorFlags == ErrorFlagsEnum.Atomic)
                return "atomic";
            return null;
        }

        public static string GetDepthAsString(DepthEnum depth)
        {
            if (depth == DepthEnum.Zero)
                return "0";
            if (depth == DepthEnum.One)
                return "1";
            if (depth == DepthEnum.OneNoRoot)
                return "1,noroot";
            if (depth == DepthEnum.Infinity)
                return "infinity";
            if (depth == DepthEnum.InfinityNoRoot)
                return "infinity,noroot";
            return null;
        }

        public static string GetPutOptionAsString(PutOptionEnum PutOption)
        {
            List<string> putOptions = new List<string>();
            if ((PutOption & PutOptionEnum.Overwrite) == PutOptionEnum.Overwrite)
                putOptions.Add("overwrite");
            if ((PutOption & PutOptionEnum.CreateDir) == PutOptionEnum.CreateDir)
                putOptions.Add("createdir");
            if ((PutOption & PutOptionEnum.MigrationSemantics) == PutOptionEnum.MigrationSemantics)
                putOptions.Add("migrationsemantics");
            if ((PutOption & PutOptionEnum.Atomic) == PutOptionEnum.Atomic)
                putOptions.Add("atomic");
            if ((PutOption & PutOptionEnum.Checkin) == PutOptionEnum.Checkin)
                putOptions.Add("checkin");
            if ((PutOption & PutOptionEnum.Checkout) == PutOptionEnum.Checkout)
                putOptions.Add("checkout");
            if ((PutOption & PutOptionEnum.Edit) == PutOptionEnum.Edit)
                putOptions.Add("edit");
            if ((PutOption & PutOptionEnum.ForceVersions) == PutOptionEnum.ForceVersions)
                putOptions.Add("forceversions");
            if ((PutOption & PutOptionEnum.ListThickets) == PutOptionEnum.ListThickets)
                putOptions.Add("listthickets");
            if ((PutOption & PutOptionEnum.Thicket) == PutOptionEnum.Thicket)
                putOptions.Add("thicket");
            return string.Join(",", putOptions.ToArray());
        }

        public static string GetRenameOptionAsString(RenameOptionEnum renameOption)
        {
            List<string> renameOptions = new List<string>();
            if ((renameOption & RenameOptionEnum.None) == RenameOptionEnum.None)
                return "none";
            if ((renameOption & RenameOptionEnum.CreateDir) == RenameOptionEnum.CreateDir)
                renameOptions.Add("createdir");
            if ((renameOption & RenameOptionEnum.FindBacklinks) == RenameOptionEnum.FindBacklinks)
                renameOptions.Add("findbacklinks");
            if ((renameOption & RenameOptionEnum.NoChangeAll) == RenameOptionEnum.NoChangeAll)
                renameOptions.Add("nochangeall");
            if ((renameOption & RenameOptionEnum.PatchPrefix) == RenameOptionEnum.PatchPrefix)
                renameOptions.Add("patchprefix");
            return string.Join(",", renameOptions.ToArray());
        }
        
    }
}
