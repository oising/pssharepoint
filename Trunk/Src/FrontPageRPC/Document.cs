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
    public class Document : ITreeNode, IDisposable
    {
        static Regex metaVectorRegex = new Regex(@"document=\[document_name=(?<name>.*);meta_info=\[(?<metaInfo>.*)\]\]", RegexOptions.Compiled);
        const string DefaultMetaExtension = ".metainfo.xml";
        private string _webRelativeUrl;
        private MetaInfoCollection _metaInfo;
        private byte[] _requestBytes;
        private FileInfo _fileInfo;
        private Folder _parent;
        private string _webUrl;
        private bool _isDisposed = false;

        public string WebRelativeUrl
        {
            get { return _webRelativeUrl; }
            set
            {
                if (value == null)
                    _webRelativeUrl = null;
                else
                    _webRelativeUrl = value.TrimStart('/');
            }
        }

        public string WebUrl
        {
            get { return _webUrl; }
            set { _webUrl = value; }
        }

        public string Url
        {
            get { return Utils.CombineUrl(WebUrl, WebRelativeUrl); }
        }


        public MetaInfoCollection MetaInfo
        {
            get
            {
                if (_metaInfo == null)
                    _metaInfo = new MetaInfoCollection();
                return _metaInfo;
            }
            set { _metaInfo = value; }
        }

        internal Byte[] RequestBytes
        {
            get
            {
                if (_requestBytes == null)
                {
                    ParameterCollection parameters = new ParameterCollection();
                    Parameter documents_name = new Parameter("document_name", this.WebRelativeUrl);
                    Parameter meta_info = new Parameter("meta_info", this.MetaInfo);
                    StringVector documents = new StringVector(documents_name, meta_info);
                    parameters.Add("document", documents);
                    _requestBytes = parameters.ToByteArray();
                }
                return _requestBytes;
            }
        }

        public Folder Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public string FileName
        {
            get { return FileInfo.FullName; }
            set
            {
                FileInfo = new FileInfo(value);
            }
        }

        public FileInfo FileInfo
        {
            get
            {
                if (_fileInfo == null)
                    _fileInfo = new FileInfo(Path.GetTempFileName());
                return _fileInfo;
            }
            set
            {
                _fileInfo = value;
            }
        }

        public Document(string fullWebRelativeName)
            : this(null, fullWebRelativeName)
        {
        }

        public Document(string webUrl, string fullWebRelativeName)
        {
            _webUrl = webUrl;
            Load(fullWebRelativeName, null, null);
        }

        public Document(string fullWebRelativeName, MetaInfoCollection metaInfo)
        {
            Load(fullWebRelativeName, null, metaInfo);
        }

        public Document(string fullWebRelativeName, byte[] file, MetaInfoCollection metaInfo)
        {
            string fileName = Path.GetTempFileName();
            System.IO.File.WriteAllBytes(fileName, file);
            _fileInfo = new FileInfo(fileName);
            Load(fullWebRelativeName, FileInfo, metaInfo);
        }

        public Document(string fullWebRelativeName, FileInfo fileInfo, MetaInfoCollection metaInfo)
        {
            Load(fullWebRelativeName, fileInfo, metaInfo);
        }

        internal Document(XPathNavigator rpcResponseNav, FileInfo fileInfo)
        {
            if (rpcResponseNav == null)
                this._fileInfo = fileInfo;
            else
                Load(rpcResponseNav.SelectSingleNode("li[@name='document_name']").Value, fileInfo, new MetaInfoCollection(rpcResponseNav.SelectSingleNode("ul[@name='meta_info']")));
        }

        internal Document(string metaString, bool isMetaString)
        {
            Match match = metaVectorRegex.Match(metaString);
            if (match.Success)
                Load(match.Groups["name"].Value, null, new MetaInfoCollection(match.Groups["metaInfo"].Value));
        }

        protected void Load(string fullWebRelativeName, FileInfo fileInfo, MetaInfoCollection metaInfo)
        {
            this._fileInfo = fileInfo;
            WebRelativeUrl = fullWebRelativeName;
            _metaInfo = metaInfo;
        }


        public string Name
        {
            get { return Path.GetFileName(WebRelativeUrl); }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string DisplayName
        {
            get
            {
                return Name;
            }
        }

        public object _icon;
        public object Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        public string IconName
        {
            get
            {
                string ext = Path.GetExtension(WebRelativeUrl);
                if (ext.Length < 1)
                    return "ICODC.GIF";
                return string.Format("IC{0}.GIF", ext.Remove(0, 1).ToUpper());
            }
        }

        public string WebRelativeFolderUrl
        {
            get { return Utils.GetDirectoryName(WebRelativeUrl); }
        }




        public bool ServerFileStatusIsOk
        {
            get
            {
                MetaInfo fileStatus = MetaInfo["mf-file-status"];
                if (fileStatus == null)
                    return true;
                return fileStatus.GetValueAsInteger() == 0;
            }
        }

        public long FileSize
        {
            get
            {
                if (_fileInfo == null || !_fileInfo.Exists)
                    return 0;
                return _fileInfo.Length;
            }
        }

        public int ServerFileSize
        {
            get
            {
                MetaInfo fileSize = MetaInfo["vti_filesize"];
                if (fileSize == null)
                    return 0;
                return fileSize.GetValueAsInteger();
            }
        }

        public string ServerFileSizeKB
        {
            get
            {
                return string.Format("{0}KB", ServerFileSize / 1024);
            }
        }

        public DateTime TimeCheckedOut
        {
            get
            {
                MetaInfo result = MetaInfo["vti_sourcecontroltimecheckedout"];
                if (result == null)
                    return DateTime.MinValue;
                return result.GetValueAsDateTime();
            }
        }

        public DateTime TimeLastModified
        {
            get
            {
                MetaInfo result = MetaInfo["vti_timelastmodified"];
                if (result == null)
                    return DateTime.MinValue;
                return result.GetValueAsDateTime();
            }
        }

        public DateTime TimeCreated
        {
            get
            {
                MetaInfo result = MetaInfo["vti_timecreated"];
                if (result == null)
                    return DateTime.MinValue;
                return result.GetValueAsDateTime();
            }
        }

        public string Author
        {
            get
            {
                MetaInfo result = MetaInfo["vti_author"];
                if (result == null)
                    return null;
                return result.GetValueAsString();
            }
        }

        public bool IsCheckedOut
        {
            get
            {
                return !string.IsNullOrEmpty(CheckedOutBy);
            }
        }


        public string CheckedOutBy
        {
            get
            {
                MetaInfo result = MetaInfo["vti_sourcecontrolcheckedoutby"];
                if (result == null)
                    return null;
                return result.GetValueAsString();
            }
        }

        public string Title
        {
            get
            {
                MetaInfo result = MetaInfo["vti_title"];
                if (result == null)
                    return null;
                return result.GetValueAsString();
            }
            set
            {
                MetaInfo result = MetaInfo["vti_title"];
                if (result == null)
                    MetaInfo.Add("vti_title", value);
                else
                    result.Value = value;
            }
        }

        public string ModifiedBy
        {
            get
            {
                MetaInfo result = MetaInfo["vti_modifiedby"];
                if (result == null)
                    return null;
                return result.GetValueAsString();
            }
        }

        public void SaveMetaInfo()
        {
            SaveMetaInfo(string.Empty);
        }

        public void SaveMetaInfo(DirectoryInfo directory)
        {
            SaveMetaInfo(directory == null ? string.Empty : Path.Combine(directory.FullName, Path.GetFileName(WebRelativeUrl) + DefaultMetaExtension));
        }

        public void SaveMetaInfo(string path)
        {
            if (string.IsNullOrEmpty(path)) path = Path.GetFileName(WebRelativeUrl) + DefaultMetaExtension;
            System.IO.File.WriteAllText(path, _metaInfo.Xml);
        }

        public override string ToString()
        {
            return this.Url;
        }


        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Document()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                if (disposing)
                {
                }
                if (_fileInfo != null && _fileInfo.Exists)
                {
                    if (string.Equals(Path.GetTempPath().TrimEnd('\\'), _fileInfo.DirectoryName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            _fileInfo.Delete();
                        }
                        catch { }
                    }
                }
            }
        }

        #endregion
    }
}
