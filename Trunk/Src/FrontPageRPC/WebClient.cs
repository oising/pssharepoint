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
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Net.Sockets;
using System.Xml;
using System.Text.RegularExpressions;

namespace HubKey.Net.FrontPageRPC
{
    /// <summary>
    /// A class to enable communication with a web server through the FrontPage Server Extensions protocol.
    /// </summary>
    public class WebClient
    {
        public event RemoveDocumentsCompletedEventHandler RemoveDocumentsCompleted;
        public event MoveDocumentsCompletedEventHandler MoveDocumentsCompleted;
        public event DownloadDocumentCompletedEventHandler DownloadDocumentCompleted;
        public event UploadDocumentCompletedEventHandler UploadDocumentCompleted;
        public event DownloadDocumentsCompletedEventHandler DownloadDocumentsCompleted;
        public event UploadDocumentsCompletedEventHandler UploadDocumentsCompleted;
        public event MoveProgressChangedEventHandler MoveProgressChanged;
        public event RemoveProgressChangedEventHandler RemoveProgressChanged; 
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event UploadProgressChangedEventHandler UploadProgressChanged;
        public event AuthenticateEventHandler Authenticate;
        const string FormURLEncoded = "Content-Type: application/x-www-form-urlencoded";
        const string OctetStream = "Content-Type: application/octet-stream\r\n";
        ICredentials _credentials;
        protected bool _cancelled = false;
        protected bool _ignoreBusy = false;
        protected bool _isBusy;
        protected Thread _asyncThread;
        protected bool _isAsync;
        IWebProxy _proxy;
        bool _useDefaultCredentials = true;
        string _boundary;
        string _baseString;
        Uri _baseAddress;
        long _notifyCount = 0;
        long _notifyEveryBytes = 4096 * 2;
        bool _rethrow = false;
        int _timeout = 0;
        static Dictionary<string, ServerInfo> _serverInfos = new Dictionary<string, ServerInfo>();
        static ServerInfo _defaultServerInfo;
        ServerInfo _serverInfo;

        #region Public Constants

        /// <summary>
        /// Returns the default connection timeout.
        /// </summary>
        public static readonly int DefaultTimeout = 1000 * 60 * 5;

        /// <summary>
        /// Returns the default script paths and version number for FPSE Version 5.
        /// </summary>
        public static readonly ServerInfo ServerInfoVersion5 = new ServerInfo("5.0.2.6790", "_vti_bin/shtml.dll/_vti_rpc", "_vti_bin/_vti_aut/author.dll", "_vti_bin/_vti_adm/admin.dll", "_vti_bin/owssvr.dll");

        /// <summary>
        /// Returns the default script paths and version number for FPSE Version 6.
        /// </summary>
        public static readonly ServerInfo ServerInfoVersion6 = new ServerInfo("6.0.2.6353", "_vti_bin/shtml.dll/_vti_rpc", "_vti_bin/_vti_aut/author.dll", "_vti_bin/_vti_adm/admin.dll", "_vti_bin/owssvr.dll");

        /// <summary>
        /// Returns the default script paths and version number for FPSE Version 12.
        /// </summary>
        public static readonly ServerInfo ServerInfoVersion12 = new ServerInfo("12.0.0.4518", "_vti_bin/shtml.dll/_vti_rpc", "_vti_bin/_vti_aut/author.dll", "_vti_bin/_vti_adm/admin.dll", "_vti_bin/owssvr.dll");

        #endregion

        #region Constructors

        static WebClient()
        {
            FrontPageRPCWebRequestCreate creator = new FrontPageRPCWebRequestCreate();
            WebRequest.RegisterPrefix("rpc", creator);
        }

        /// <summary>
        /// Creates an instance of a WebClient object to enable communication with a web server through the FrontPage Server Extensions protocol.
        /// </summary>
        public WebClient()
        {

        }

        /// <summary>
        /// Creates an instance of a WebClient object to enable communication with a web server through the FrontPage Server Extensions protocol.
        /// </summary>
        /// <param name="webUrl">The root web url. If this is not set each request method must set its own WebUrl property.</param>
        public WebClient(string webUrl)
        {
            BaseAddress = webUrl;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the root web url. If this is not set each request method must set its own WebUrl property.
        /// </summary>
        public string BaseAddress
        {
            get
            {
                if (_baseString == null)
                {
                    if (_baseAddress == null)
                        return "";
                }

                _baseString = _baseAddress.ToString();
                return _baseString;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _baseAddress = null;
                }
                else
                {
                    _baseAddress = new Uri(value);
                }
            }
        }

        /// <summary>
        /// Gets or gets the credentials to use. 
        /// </summary>
        public ICredentials Credentials
        {
            get
            {
                if (_useDefaultCredentials && _credentials == null)
                    _credentials = CredentialCache.DefaultCredentials;
                return _credentials;
            }
            set { _credentials = value; }
        }

        /// <summary>
        /// Gets or sets the global default FrontPage server information (script paths and version number).
        /// </summary>
        /// <example>WebClient.DefaultServerInfo = WebClient.ServerInfoVersion12;</example>
        public static ServerInfo DefaultServerInfo
        {
            get
            {
                return _defaultServerInfo;
            }
            set
            {
                _defaultServerInfo = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the FrontPage server information (script paths and version number). Gets the WebClient.DefaultServerInfo if this is set. Is automatically discovered on the first request if not set.
        /// </summary>
        /// <seealso cref="DefaultServerInfo"/>
        public ServerInfo ServerInfo
        {
            get 
            {
                if (_serverInfo == null)
                {
                    if (_defaultServerInfo == null)
                        GetServerInfo();
                    else
                        _serverInfo = _defaultServerInfo;
                }
                return _serverInfo; 
            }
            set { _serverInfo = value; }
        }

        /// <summary>
        /// Gets or sets whether to use DefaultCredentials if no credentials are supplied.
        /// </summary>
        public bool UseDefaultCredentials
        {
            get
            {
                return _useDefaultCredentials;
            }
            set
            {
                _useDefaultCredentials = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to rethrow request exceptions.
        /// </summary>
        public bool Rethrow
        {
            get { return _rethrow; }
            set { _rethrow = value; }
        }
        
        /// <summary>
        /// Gets whether a request is currently being processed.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            internal set
            {
                lock (this)
                {
                    if (_isBusy)
                        throw new NotSupportedException("WebClient is already busy.");
                    _isBusy = true;
                }
            }
        }

        /// <summary>
        /// Gets whether an asynchronous request is currently being processed.
        /// </summary>
        public bool IsAsyncRunning
        {
            get { return _isAsync && _isBusy; }
        }

        /// <summary>
        /// Gets whether the request was cancelled.
        /// </summary>
        public bool Cancelled
        {
            get { return _cancelled; }
        }

        /// <summary>
        /// Gets or sets the request timeout.
        /// </summary>
        public int Timeout
        {
            get
            {
                if (_timeout == 0)
                    return DefaultTimeout;
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the web proxy to use.
        /// </summary>
        public IWebProxy Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Provides a method to cancel asynchronous requests. For UploadDocumentsAsync and DownloadDocumentsAsync, the request will be canceled after the current document has been processed. Use the abort override to cancel immediately.
        /// </summary>
        public void CancelAsync()
        {
            CancelAsync(false);
        }

        /// <summary>
        /// Provides a method to cancel asynchronous requests.
        /// </summary>
        /// <param name="abort">Set true to immediately stop the request. Set false to stop the request after each discrete operation, e.g. uploading an individual file, has completed.</param>
        public void CancelAsync(bool abort)
        {
            lock (this)
            {
                if (_asyncThread == null)
                    return;
                Thread thread = _asyncThread;
                FinishAsync();
                _cancelled = true;
                if (abort)
                    thread.Abort();
                else
                    thread.Interrupt();
            }
        }

        #region Open Service

        /// <summary>
        /// Provides meta-information for a Web site to the client application.
        /// </summary>
        /// <param name="webUrl">The url of the web site.</param>
        /// <returns></returns>
        public OpenServiceResponse OpenService(string webUrl)
        {
            if (string.IsNullOrEmpty(webUrl))
                throw new ArgumentNullException("webUrl");
            return OpenService(new OpenServiceRequest(webUrl));
        }

        /// <summary>
        /// Provides meta-information for a Web site to the client application.
        /// </summary>
        /// <param name="openServiceRequest"></param>
        /// <returns></returns>
        public OpenServiceResponse OpenService(OpenServiceRequest openServiceRequest)
        {
            if (openServiceRequest == null)
                throw new ArgumentNullException("openServiceRequest");
            try
            {
                Busy();
                _isAsync = false;
                return (OpenServiceResponse)GetResponseInternal(openServiceRequest, new OpenServiceResponse(this), null); ;
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion

        #region Url To Web Url

        /// <summary>
        /// Given a URL for a file, returns the URL of the Web site to which the file belongs, and the subsite, if applicable.
        /// </summary>
        /// <param name="url">The full url of the file.</param>
        /// <returns></returns>
        public UrlToWebUrlResponse UrlToWebUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            return UrlToWebUrl(new UrlToWebUrlRequest(url));
        }

        /// <summary>
        /// Given a URL for a file, returns the URL of the Web site to which the file belongs, and the subsite, if applicable.
        /// </summary>
        /// <param name="urlToWebUrlRequest"></param>
        /// <returns></returns>
        public UrlToWebUrlResponse UrlToWebUrl(UrlToWebUrlRequest urlToWebUrlRequest)
        {
            if (urlToWebUrlRequest == null)
                throw new ArgumentNullException("urlToWebUrlRequest");
            try
            {
                Busy();
                _isAsync = false;
                return (UrlToWebUrlResponse)GetResponseInternal(urlToWebUrlRequest, new UrlToWebUrlResponse(this, urlToWebUrlRequest), null); ;
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion

        #region Remove Documents

        /// <summary>
        /// Deletes a document or folder from the server.
        /// </summary>
        /// <param name="url">The full folder or document URL.</param>
        /// <returns></returns>
        public RemoveDocumentsResponse RemoveDocument(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;
            RemoveDocumentsRequest request = new RemoveDocumentsRequest(urlResponse.FileUrl);
            request.WebUrl = urlResponse.WebUrl;
            return RemoveDocuments(request);
        }

        /// <summary>
        /// Deletes a document or folder from the server.
        /// </summary>
        /// <param name="url">The full folder or document URL.</param>
        /// <returns></returns>
        public RemoveDocumentsResponse RemoveDocument(ITreeNode documentOrFolder)
        {
            RemoveDocumentsRequest request;
            if (documentOrFolder == null)
                throw new ArgumentNullException("documentOrFolder");
            if (documentOrFolder is Folder)
                request = new RemoveDocumentsRequest((Folder)documentOrFolder);
            else
                request = new RemoveDocumentsRequest((Document)documentOrFolder);
            return RemoveDocuments(request);
        }

        /// <summary>
        /// Deletes a document or folder from the server.
        /// </summary>
        /// <param name="webUrl">The website URL.</param>
        /// <param name="webRelativeUrls">A list of website relative document or folder URLs.</param>
        /// <returns></returns>
        public RemoveDocumentsResponse RemoveDocuments(string webUrl, IEnumerable<string> webRelativeUrls)
        {
            if (string.IsNullOrEmpty(webUrl))
                throw new ArgumentNullException("webUrl");
            if (webRelativeUrls == null)
                throw new ArgumentNullException("webRelativeUrls");
            RemoveDocumentsRequest request = new RemoveDocumentsRequest(webRelativeUrls);
            request.WebUrl = webUrl;
            return RemoveDocuments(request);
        }

        /// <summary>
        /// Deletes a document or folder from the server.
        /// </summary>
        /// <param name="removeDocumentsRequest"></param>
        /// <returns></returns>
        public RemoveDocumentsResponse RemoveDocuments(RemoveDocumentsRequest removeDocumentsRequest)
        {
            if (removeDocumentsRequest == null)
                throw new ArgumentNullException("removeDocumentsRequest");
            try
            {
                Busy();
                _isAsync = false;
                return (RemoveDocumentsResponse)GetResponseInternal(removeDocumentsRequest, new RemoveDocumentsResponse(this, removeDocumentsRequest), null); ;
            }
            finally
            {
                _isBusy = false;
            }
        }

        public void RemoveDocumentsAsync(RemoveDocumentsRequest removeDocumentsRequest)
        {
            if (removeDocumentsRequest == null)
                throw new ArgumentNullException("removeDocumentsRequest");
            RemoveDocumentsAsync(removeDocumentsRequest, null);
        }

        void RemoveDocumentsAsync(RemoveDocumentsRequest request, object userToken)
        {
            lock (this)
            {
                Busy();
                _isAsync = true;
                
                _asyncThread = new Thread(delegate(object oArgs)
                {
                    object[] args = (object[])oArgs;
                    RemoveDocumentsResponse response;
                    try
                    {
                        response = (RemoveDocumentsResponse)GetResponseInternal((RequestBase)args[0], (ResponseBase)args[1], args[2]);
                        OnRemoveDocumentsCompleted(new RemoveDocumentsCompletedEventArgs(response, (RemoveDocumentsRequest)args[0], null, false, args[2]));
                    }
                    catch (ThreadAbortException)
                    {
                        throw new ThreadInterruptedException();
                    }
                    catch (ThreadInterruptedException)
                    {
                        OnRemoveDocumentsCompleted(new RemoveDocumentsCompletedEventArgs(null, (RemoveDocumentsRequest)args[0], null, true, args[2]));
                    }
                    catch (Exception e)
                    {
                        OnRemoveDocumentsCompleted(new RemoveDocumentsCompletedEventArgs(null, (RemoveDocumentsRequest)args[0], e, false, args[2]));
                    }
                });
                object[] startArgs = new object[] { request, new RemoveDocumentsResponse(this, request), userToken };
                _asyncThread.Start(startArgs);
            }
        }

        protected virtual void OnRemoveDocumentsCompleted(RemoveDocumentsCompletedEventArgs args)
        {
            FinishAsync();
            if (RemoveDocumentsCompleted != null)
                RemoveDocumentsCompleted(this, args);
        }

        #endregion

        #region WebDav

        /// <summary>
        /// Creates folders on the server.
        /// </summary>
        /// <param name="rootUrl">The full url of the first folder known to exist.</param>
        /// <param name="folderUrl">The rootUrl relative path of folders to create.</param>
        public void CreateFolders(string rootUrl, string folderUrl)
        {
            StringBuilder sb = new StringBuilder(rootUrl.TrimEnd('/'));
            string[] segments = folderUrl.Split('/');
            for (int i = 0; i < segments.Length - 1; i++)
            {
                sb.Append("/");
                sb.Append(segments[i]);
                CreateFolder(sb.ToString());
            }
        }

        /// <summary>
        /// Creates a folder on the server.
        /// </summary>
        /// <param name="folderURL">The full url of the new folder.</param>
        /// <returns>True if the folder was created.</returns>
        public bool CreateFolder(string folderURL)
        {
            try
            {
                WebRequest request = WebRequest.Create(folderURL);
                request.Credentials = this.Credentials;
                request.Method = "MKCOL";
                WebResponse response = request.GetResponse();
                response.Close();
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns resource properties for a url using the WebDAV PROPFIND method.
        /// </summary>
        /// <param name="url">The full resource url.</param>
        /// <param name="depth">The level to apply the PROPFIND method to. For a recursive list of all properties for example, use DepthEnum.Infinity.</param>
        /// <param name="allProperties">A flag indicating whether to return all properties. If false, a minimum set of properties is returned, suitable for building a directory structure.</param>
        /// <returns></returns>
        public XmlDocument GetProperties(string url, DepthEnum depth, bool allProperties)
        {
            Stream requestStream = null;
            Stream responseStream = null;
            XmlDocument result = null;
            try
            {
                if (string.IsNullOrEmpty(url))
                    throw new ArgumentNullException("url");

                WebRequest request = GetRequest(new Uri(url), false, "PROPFIND");
                request.Headers.Add("Depth", Utils.GetDepthAsString(depth));
                request.Headers.Add("Translate", "F");

                requestStream = request.GetRequestStream();

                StringBuilder requestString = new StringBuilder();

                requestString.AppendLine("<?xml version=\"1.0\"?>");
                requestString.AppendLine("<D:propfind xmlns:D=\"DAV:\">");
                if (allProperties)
                {
                    requestString.AppendLine("<D:allprop/>");
                }
                else
                {
                    requestString.AppendLine("<D:prop>");
                    requestString.AppendLine("  <D:displayname/>");
                    requestString.AppendLine("  <D:resourcetype/>");
                    requestString.AppendLine("  <D:checked-in/>");
                    requestString.AppendLine("  <D:checked-out/>");
                    requestString.AppendLine("  <D:supported-method-set/>");
                    requestString.AppendLine("</D:prop>");
                }
                requestString.AppendLine("</D:propfind>");

                byte[] requestBytes = Encoding.UTF8.GetBytes(requestString.ToString());
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Close();
                requestStream = null;
                
                WebResponse response = request.GetResponse();
                responseStream = response.GetResponseStream();
                
                StringBuilder responseString = new StringBuilder();
                byte[] buffer = new byte[4096];
                int nread = 0;
                while ((nread = responseStream.Read(buffer, 0, 4096)) != 0)
                {
                    responseString.Append(Encoding.UTF8.GetString(buffer, 0, nread));
                }
                
                response.Close();
                result = new XmlDocument();
                result.LoadXml(responseString.ToString());
            }
            catch (Exception)
            {
                if (this.Rethrow)
                    throw;
            }
            finally
            {
                if (requestStream != null)
                    requestStream.Close();
                if (responseStream != null)
                    responseStream.Close();
            }
            return result;
        }


        #endregion

        #region Move Document or Folder


        /// <summary>
        /// Moves a document or folder, overwriting the destination if it exists.
        /// </summary>
        /// <param name="oldUrl">The full url of the document or folder to move.</param>
        /// <param name="newUrl">The full url of the new location.</param>
        /// <returns></returns>
        public MoveDocumentResponse MoveDocument(string oldUrl, string newUrl)
        {
            return MoveDocument(oldUrl, newUrl, false, false, PutOptionEnum.Default);
        }

        /// <summary>
        /// Moves, renames or copies a document or folder.
        /// </summary>
        /// <param name="oldUrl">The full url of the document or folder to move.</param>
        /// <param name="newUrl">The full url of the new location.</param>
        /// <param name="copy">If the value is true, the method copies the source to the destination.</param>
        /// <param name="ensureFolders">Pass true to ensure that all parent folders are created before the move request is made to the server. The createdirs putoption only creates the first parent folder if it does not exist.</param>
        /// <returns></returns>
        public MoveDocumentResponse MoveDocument(string oldUrl, string newUrl, bool ensureFolders, bool copy, PutOptionEnum putOption)
        {
            MoveDocumentRequest request = GetMoveDocumentRequest(oldUrl, newUrl, ensureFolders, copy, putOption);
            return MoveDocument(request);
        }

         /// <summary>
        /// Gets a MoveDocumentRequest object.
        /// </summary>
        /// <param name="oldUrl">The full url of the document or folder to move.</param>
        /// <param name="newUrl">The full url of the new location.</param>
        /// <param name="copy">If the value is true, the method copies the source to the destination.</param>
        /// <param name="ensureFolders">Pass true to ensure that all parent folders are created before the move request is made to the server. The createdirs putoption only creates the first parent folder if it does not exist.</param>
        /// <returns></returns>
        public MoveDocumentRequest GetMoveDocumentRequest(string oldUrl, string newUrl, bool ensureFolders, bool copy, PutOptionEnum putOption)
        {
            if (string.IsNullOrEmpty(oldUrl))
                throw new ArgumentNullException("oldUrl");
            if (string.IsNullOrEmpty(newUrl))
                throw new ArgumentNullException("newUrl");

            UrlToWebUrlRequest oldUrlRequest = new UrlToWebUrlRequest(oldUrl);
            UrlToWebUrlResponse oldUrlResponse = UrlToWebUrl(oldUrlRequest);
            if (oldUrlResponse.HasError)
                throw oldUrlResponse.ErrorResponse.Exception;

            UrlToWebUrlRequest newUrlRequest = new UrlToWebUrlRequest(newUrl);
            UrlToWebUrlResponse newUrlResponse = UrlToWebUrl(newUrlRequest);
            if (newUrlResponse.HasError)
                throw newUrlResponse.ErrorResponse.Exception;

            MoveDocumentRequest request = new MoveDocumentRequest(oldUrlResponse.FileUrl);
            request.WebUrl = oldUrlResponse.WebUrl;
            request.NewUrl = newUrlResponse.FileUrl;
            request.Copy = copy;
            request.PutOption = putOption;
            request.EnsureFolders = ensureFolders;

            if (oldUrlResponse.WebUrl != newUrlResponse.WebUrl)
                request.NewWebUrl = newUrlResponse.WebUrl;

            return request;
        }

        /// <summary>
        /// Changes the name of a document or folder to the new name provided by the user. Optionally can copy the document or folder to a new location.
        /// </summary>
        /// <param name="moveDocumentRequest"></param>
        /// <returns></returns>
        public MoveDocumentResponse MoveDocument(MoveDocumentRequest moveDocumentRequest)
        {
            if (moveDocumentRequest == null)
                throw new ArgumentNullException("moveDocumentRequest");
            try
            {
                Busy();
                _isAsync = false;

                return GetMoveDocumentResponse(moveDocumentRequest);
            }
            finally
            {
                _isBusy = false;
            }
        }

        internal MoveDocumentResponse GetMoveDocumentResponse(MoveDocumentRequest moveDocumentRequest)
        {
            if (moveDocumentRequest.EnsureFolders)
                CreateFolders(moveDocumentRequest.NewWebUrl, moveDocumentRequest.NewUrl);
            
            if (moveDocumentRequest.IsCrossWebMove)
            {
                if (moveDocumentRequest.DocumentOrFolder is Folder)
                {
                    throw new NotSupportedException("Cross web moves of folders is not supported.");
                }
                MoveDocumentResponse response = new MoveDocumentResponse(this, moveDocumentRequest);
                Document document = null;
                bool rethrow = _rethrow;
                try
                {
                    _ignoreBusy = true;
                    this.Rethrow = true;
                    DownloadDocumentRequest downloadDocumentRequest = new DownloadDocumentRequest(moveDocumentRequest.OldUrl);
                    downloadDocumentRequest.WebUrl = moveDocumentRequest.WebUrl;

                    DownloadDocumentResponse downloadDocumentResponse = this.DownloadDocument(downloadDocumentRequest);

                    document = downloadDocumentResponse.Document;

                    UploadDocumentRequest uploadDocumentRequest = new UploadDocumentRequest(document);
                    uploadDocumentRequest.WebUrl = moveDocumentRequest.NewWebUrl;
                    document.WebRelativeUrl = moveDocumentRequest.NewUrl;
                    uploadDocumentRequest.UpdateAllMetaInfo = true;

                    UploadDocumentResponse uploadDocumentResponse = this.UploadDocument(uploadDocumentRequest);

                    if (!moveDocumentRequest.Copy)
                    {
                        RemoveDocumentsResponse removeDocumentsResponse = this.RemoveDocuments(moveDocumentRequest.WebUrl, new string[] { moveDocumentRequest.OldUrl });
                    }

                    response.OldUrl = moveDocumentRequest.OldUrl;
                    response.NewUrl = moveDocumentRequest.NewUrl;
                    response.Message = string.Format("successfully renamed URL '{0}' as '{1}'", response.OldUrl, response.NewUrl);
                    response.Documents.Add(document);
                    
                }
                catch (Exception ex)
                {
                    if (rethrow)
                        throw ex;
                    response.ErrorResponse = new FrontPageRPCError(ex);
                }
                finally
                {
                    _rethrow = rethrow;
                    _ignoreBusy = false;
                    if (document != null && document.FileInfo.Exists)
                        document.FileInfo.Delete();
                }
                return response;
            }
            else
                return (MoveDocumentResponse)GetResponseInternal(moveDocumentRequest, new MoveDocumentResponse(this, moveDocumentRequest), null); ;
        }


        /// <summary>
        /// Moves or copies a series of files or folders.
        /// </summary>
        /// <param name="moveDocumentsRequest"></param>
        /// <returns></returns>
        public MoveDocumentsResponse MoveDocuments(MoveDocumentsRequest moveDocumentsRequest)
        {
            if (moveDocumentsRequest == null)
                throw new ArgumentNullException("moveDocumentsRequest");
            try
            {
                Busy();
                _isAsync = false;
                MoveDocumentsResponse moveDocumentsResponse = new MoveDocumentsResponse(moveDocumentsRequest);
                foreach (MoveDocumentRequest moveDocumentRequest in moveDocumentsRequest.MoveDocumentRequests)
                {
                    MoveDocumentResponse response = GetMoveDocumentResponse(moveDocumentRequest);
                    moveDocumentsResponse.MoveDocumentResponses.Add(response);
                }
                return moveDocumentsResponse;
            }
            finally
            {
                _isBusy = false;
            }
        }

        /// <summary>
        /// Moves or copies a series of files or folders.
        /// </summary>
        /// <param name="downloadRequest"></param>
        /// <returns></returns>
        public void MoveDocumentsAsync(MoveDocumentsRequest moveDocumentsRequest)
        {
            if (moveDocumentsRequest == null)
                throw new ArgumentNullException("moveDocumentsRequest");
            MoveDocumentsAsync(moveDocumentsRequest, null);
        }

        void MoveDocumentsAsync(MoveDocumentsRequest request, object userToken)
        {
            lock (this)
            {
                Busy();
                _isAsync = true;

                _asyncThread = new Thread(delegate(object oArgs)
                {
                    object[] args = (object[])oArgs;
                    MoveDocumentsRequest moveDocumentsRequest;
                    MoveDocumentsResponse moveDocumentsResponse;
                    long running = 0;
                    try
                    {
                        moveDocumentsRequest = (MoveDocumentsRequest)args[0];
                        moveDocumentsResponse = (MoveDocumentsResponse)args[1];
                        long total = moveDocumentsRequest.MoveDocumentRequests.Count;
                        foreach (MoveDocumentRequest moveDocumentRequest in moveDocumentsRequest.MoveDocumentRequests)
                        {
                            MoveDocumentResponse response = GetMoveDocumentResponse(moveDocumentRequest);
                            moveDocumentsResponse.MoveDocumentResponses.Add(response);
                            UpdateMoveProgress(ref running, total);
                        }

                        OnMoveDocumentsCompleted(new MoveDocumentsCompletedEventArgs(moveDocumentsResponse, (MoveDocumentsRequest)args[0], null, false, args[2]));
                    }
                    catch (ThreadAbortException)
                    {
                        throw new ThreadInterruptedException();
                    }
                    catch (ThreadInterruptedException)
                    {
                        OnMoveDocumentsCompleted(new MoveDocumentsCompletedEventArgs(null, (MoveDocumentsRequest)args[0], null, true, args[2]));
                    }
                    catch (Exception e)
                    {
                        OnMoveDocumentsCompleted(new MoveDocumentsCompletedEventArgs(null, (MoveDocumentsRequest)args[0], e, false, args[2]));
                    }
                });
                object[] startArgs = new object[] { request, new MoveDocumentsResponse(request), userToken };
                _asyncThread.Start(startArgs);
            }
        }

        protected virtual void OnMoveDocumentsCompleted(MoveDocumentsCompletedEventArgs args)
        {
            FinishAsync();
            if (MoveDocumentsCompleted != null)
                MoveDocumentsCompleted(this, args);
        }

        #endregion

        #region Check in Document

        /// <summary>
        /// Checks a document in from source control.
        /// </summary>
        /// <param name="url">The full document URL.</param>
        /// <returns></returns>
        public CheckInDocumentResponse CheckInDocument(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;
            CheckInDocumentRequest request = new CheckInDocumentRequest(urlResponse.WebUrl, urlResponse.FileUrl);
            return CheckInDocument(request);
        }

        /// <summary>
        /// Checks a document in from source control.
        /// </summary>
        /// <param name="document">The document to check in.</param>
        /// <returns></returns>
        public CheckInDocumentResponse CheckInDocument(Document document)
        {
            return CheckInDocument(document, null);
        }

        /// <summary>
        /// Checks a document in from source control.
        /// </summary>
        /// <param name="document">The document to check in.</param>
        /// <param name="comment">A comment to store in the source control system.</param>
        /// <returns></returns>
        public CheckInDocumentResponse CheckInDocument(Document document, string comment)
        {
            CheckInDocumentRequest checkInDocumentRequest = new CheckInDocumentRequest(document);
            checkInDocumentRequest.Comment = comment;
            return CheckInDocument(checkInDocumentRequest);
        }

        /// <summary>
        /// Checks a document in from source control.
        /// </summary>
        /// <param name="checkinDocumentRequest"></param>
        /// <returns></returns>
        public CheckInDocumentResponse CheckInDocument(CheckInDocumentRequest checkinDocumentRequest)
        {
            if (checkinDocumentRequest == null)
                throw new ArgumentNullException("checkinDocumentRequest");
            try
            {
                Busy();
                _isAsync = false;
                return (CheckInDocumentResponse)GetResponseInternal(checkinDocumentRequest, new CheckInDocumentResponse(this, checkinDocumentRequest), null); ;
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion

        #region Check out Document

        /// <summary>
        /// Checks a document out from source control.
        /// </summary>
        /// <param name="url">The full document URL.</param>
        /// <returns></returns>
        public CheckOutDocumentResponse CheckOutDocument(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;
            CheckOutDocumentRequest request = new CheckOutDocumentRequest(urlResponse.WebUrl, urlResponse.FileUrl);
            return CheckOutDocument(request);
        }

        /// <summary>
        /// Checks a document out from source control.
        /// </summary>
        /// <param name="document">The document to check out.</param>
        /// <returns></returns>
        public CheckOutDocumentResponse CheckOutDocument(Document document)
        {
            return CheckOutDocument(new CheckOutDocumentRequest(document));
        }

        /// <summary>
        /// Checks a document out from source control.
        /// </summary>
        /// <param name="checkoutDocumentRequest"></param>
        /// <returns></returns>
        public CheckOutDocumentResponse CheckOutDocument(CheckOutDocumentRequest checkoutDocumentRequest)
        {
            if (checkoutDocumentRequest == null)
                throw new ArgumentNullException("checkoutDocumentRequest");
            try
            {
                Busy();
                _isAsync = false;
                return (CheckOutDocumentResponse)GetResponseInternal(checkoutDocumentRequest, new CheckOutDocumentResponse(this, checkoutDocumentRequest), null); ;
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion

        #region Un Check Out Document

        /// <summary>
        /// Undoes a check-out of a file from a source control database. If the file has changed after it was checked out, this method causes those changes to be lost.
        /// </summary>
        /// <param name="url">The full document URL.</param>
        /// <returns></returns>
        public UnCheckOutDocumentResponse UnCheckOutDocument(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;
            UnCheckOutDocumentRequest request = new UnCheckOutDocumentRequest(urlResponse.WebUrl, urlResponse.FileUrl);
            return UnCheckOutDocument(request);
        }

        /// <summary>
        /// Undoes a check-out of a file from a source control database. If the file has changed after it was checked out, this method causes those changes to be lost.
        /// </summary>
        /// <param name="document">The document to un-check out.</param>
        /// <returns></returns>
        public UnCheckOutDocumentResponse UnCheckOutDocument(Document document)
        {
            return UnCheckOutDocument(new UnCheckOutDocumentRequest(document));
        }

        /// <summary>
        /// Undoes a check-out of a file from a source control database. If the file has changed after it was checked out, this method causes those changes to be lost.
        /// </summary>
        /// <param name="checkoutDocumentRequest"></param>
        /// <returns></returns>
        public UnCheckOutDocumentResponse UnCheckOutDocument(UnCheckOutDocumentRequest checkoutDocumentRequest)
        {
            if (checkoutDocumentRequest == null)
                throw new ArgumentNullException("checkoutDocumentRequest");
            try
            {
                Busy();
                _isAsync = false;
                return (UnCheckOutDocumentResponse)GetResponseInternal(checkoutDocumentRequest, new UnCheckOutDocumentResponse(this, checkoutDocumentRequest), null); ;
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion

        #region Get Meta Info

        /// <summary>
        /// Provides meta-information for a file or folder.
        /// </summary>
        /// <param name="url">The full document URL.</param>
        /// <returns></returns>
        public GetMetaInfoResponse GetMetaInfo(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;

            ITreeNode node;
            if (urlResponse.IsFile)
                node = new Document(urlResponse.FileUrl);
            else
                node = new Folder(urlResponse.FileUrl);

            GetMetaInfoRequest request = new GetMetaInfoRequest(node);
            request.WebUrl = urlResponse.WebUrl;
            return GetMetaInfo(request);
        }

        /// <summary>
        /// Provides meta-information for a file.
        /// </summary>
        /// <param name="document">The document to retrieve meta-information for.</param>
        /// <returns>GetMetaInfoResponse</returns>
        public GetMetaInfoResponse GetMetaInfo(Document document)
        {
            GetMetaInfoRequest request = new GetMetaInfoRequest(document);
            return GetMetaInfo(request);
        }

        /// <summary>
        /// Provides meta-information for a folder.
        /// </summary>
        /// <param name="folder">The folder to retrieve meta-information for.</param>
        /// <returns>GetMetaInfoResponse</returns>
        public GetMetaInfoResponse GetMetaInfo(Folder folder)
        {
            GetMetaInfoRequest request = new GetMetaInfoRequest(folder);
            return GetMetaInfo(request);
        }

        /// <summary>
        /// Provides meta-information for a file or folder.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>GetMetaInfoResponse</returns>
        public GetMetaInfoResponse GetMetaInfo(GetMetaInfoRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            try
            {
                Busy();
                _isAsync = false;
                return (GetMetaInfoResponse)GetResponseInternal(request, new GetMetaInfoResponse(this, request), null); ;
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion

        #region Remove Meta Info

        /// <summary>
        /// Provides a (potentially very slow - depending on file size for a document or number of files for a folder) workaround method of removing metainfo from a file or folder. To efficiently remove meta-keys use the API (see example).
        /// <remarks>Metainfo is stored as an UTF8 encoded binary image in the AllDocs table (MetaInfo column) in the content database. 
        /// <example><code>SPFile file = web.GetFile("http://localhost/Docs/testFile.txt");
        /// Hashtable props = file.Properties;
        /// file.Properties.Remove("MyMetaInfoColumnName");
        /// file.Update();</code></example>
        /// </remarks>
        /// </summary>
        /// <param name="url">The full url of the document or folder to remove keys from.</param>
        /// <param name="metaInfoName">The meta info key to be removed.</param>
        /// <returns></returns>
        public GetMetaInfoResponse RemoveMetaInfo(string url, string metaInfoName)
        {
            return RemoveMetaInfo(url, new string[] { metaInfoName });
        }

        /// <summary>
        /// Provides a (potentially very slow - depending on file size for a document or number of files for a folder) workaround method of removing metainfo from a file or folder. To efficiently remove meta-keys use the API (see example).
        /// <remarks>Metainfo is stored as an UTF8 encoded binary image in the AllDocs table (MetaInfo column) in the content database. 
        /// <example><code>SPFile file = web.GetFile("http://localhost/Docs/testFile.txt");
        /// Hashtable props = file.Properties;
        /// file.Properties.Remove("MyMetaInfoColumnName");
        /// file.Update();</code></example>
        /// </remarks>
        /// </summary>
        /// <param name="url">The full url of the document or folder to remove keys from.</param>
        /// <param name="metaInfoNames">A list of meta info keys to be removed.</param>
        /// <returns></returns>
        public GetMetaInfoResponse RemoveMetaInfo(string url, IEnumerable<string> metaInfoNames)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;

            ITreeNode node;
            if (urlResponse.IsFile)
                node = new Document(urlResponse.FileUrl);
            else
                node = new Folder(urlResponse.FileUrl);

            return RemoveMetaInfo(node, metaInfoNames);
        }

        /// <summary>
        /// Provides a (potentially very slow - depending on file size for a document or number of files for a folder) workaround method of removing metainfo from a file or folder. To efficiently remove meta-keys use the API (see example).
        /// <remarks>Metainfo is stored as an UTF8 encoded binary image in the AllDocs table (MetaInfo column) in the content database. 
        /// <example><code>SPFile file = web.GetFile("http://localhost/Docs/testFile.txt");
        /// Hashtable props = file.Properties;
        /// file.Properties.Remove("MyMetaInfoColumnName");
        /// file.Update();</code></example>
        /// </remarks>
        /// </summary>
        /// <param name="documentOrFolder">The document or folder to remove keys from.</param>
        /// <param name="metaInfoName">The meta info key to be removed.</param>
        /// <returns></returns>
        public GetMetaInfoResponse RemoveMetaInfo(ITreeNode documentOrFolder, string metaInfoName)
        {
            return RemoveMetaInfo(documentOrFolder, new string[] { metaInfoName });
        }

        /// <summary>
        /// Provides a (potentially very slow - depending on file size for a document or number of files for a folder) workaround method of removing metainfo from a file or folder. To efficiently remove meta-keys use the API (see example).
        /// <remarks>Metainfo is stored as an UTF8 encoded binary image in the AllDocs table (MetaInfo column) in the content database. 
        /// <example><code>SPFile file = web.GetFile("http://localhost/Docs/testFile.txt");
        /// Hashtable props = file.Properties;
        /// file.Properties.Remove("MyMetaInfoColumnName");
        /// file.Update();</code></example>
        /// </remarks>
        /// </summary>
        /// <param name="documentOrFolder">The document or folder to remove keys from.</param>
        /// <param name="metaInfoNames">A list of meta info keys to be removed.</param>
        /// <returns></returns>
        public GetMetaInfoResponse RemoveMetaInfo(ITreeNode documentOrFolder, IEnumerable<string> metaInfoNames)
        {
            GetMetaInfoResponse response = new GetMetaInfoResponse(this, null);
            MoveDocumentResponse moveDocumentResponse;
            MoveDocumentRequest moveDocumentRequest;
            string originalUrl = documentOrFolder.WebRelativeUrl;
            string webUrl = documentOrFolder.WebUrl;
            Document document = documentOrFolder as Document;
            bool rethrow = _rethrow;
            try
            {
                _ignoreBusy = true;
                this.Rethrow = true;

                if (document != null)
                {
                    DownloadDocumentResponse downloadDocumentResponse = this.DownloadDocument(document);

                    document = downloadDocumentResponse.Document;

                    document.WebRelativeUrl = originalUrl + ".tmp";

                    foreach (string metaInfoName in metaInfoNames)
                        document.MetaInfo.Remove(metaInfoName);

                    UploadDocumentRequest uploadDocumentRequest = new UploadDocumentRequest(document);
                    uploadDocumentRequest.UpdateAllMetaInfo = true;

                    UploadDocumentResponse uploadDocumentResponse = this.UploadDocument(uploadDocumentRequest);

                    moveDocumentRequest = new MoveDocumentRequest(document.WebRelativeUrl);
                    moveDocumentRequest.WebUrl = webUrl;
                    moveDocumentRequest.NewUrl = originalUrl;

                    moveDocumentResponse = this.MoveDocument(moveDocumentRequest);

                    document.WebRelativeUrl = originalUrl;

                    return this.GetMetaInfo(document);
                }
                
                Folder folder = (Folder)documentOrFolder;
                
                string tempFolderUrl = folder.WebRelativeUrl + ".tmp";
                

                response = this.GetMetaInfo(folder);

                folder = response.Folders[0];
                folder.WebRelativeUrl = originalUrl;
                foreach (string metaInfoName in metaInfoNames)
                    folder.MetaInfo.Remove(metaInfoName);

                ListDocumentsRequest listDocumentsRequest = new ListDocumentsRequest();
                listDocumentsRequest.InitialUrl = originalUrl;
                listDocumentsRequest.WebUrl = webUrl;
                listDocumentsRequest.ListRecurse = false;
                listDocumentsRequest.ListIncludeParent = false;

                ListDocumentsResponse listDocumentsResponse = this.ListDocuments(listDocumentsRequest);

                MoveDocumentsRequest moveDocumentsRequest = new MoveDocumentsRequest();
                foreach (ITreeNode node in listDocumentsResponse.TreeNodes)
                {
                    moveDocumentRequest = new MoveDocumentRequest(node);
                    moveDocumentRequest.NewUrl = Utils.CombineUrl(tempFolderUrl, node.Name);
                    moveDocumentRequest.Copy = true;
                    moveDocumentsRequest.MoveDocumentRequests.Add(moveDocumentRequest);
                }

                CreateFolder(Utils.CombineUrl(folder.WebUrl, tempFolderUrl));

                MoveDocumentsResponse moveDocumentsResponse = this.MoveDocuments(moveDocumentsRequest);

                RemoveDocumentsResponse removeDocumentsResponse  = this.RemoveDocument(folder);

                moveDocumentRequest = new MoveDocumentRequest(tempFolderUrl);
                moveDocumentRequest.NewUrl = originalUrl;
                moveDocumentRequest.WebUrl = webUrl;
                moveDocumentRequest.Copy = false;

                moveDocumentResponse = this.MoveDocument(moveDocumentRequest);

                SetMetaInfoRequest setMetaInfoRequest = new SetMetaInfoRequest(folder);
                setMetaInfoRequest.UpdateAllMetaInfo = true;
                SetMetaInfoResponse setMetaInfoResponse = SetMetaInfo(setMetaInfoRequest);
                
                return this.GetMetaInfo(folder);
            }
            catch (Exception ex)
            {
                if (rethrow)
                    throw ex;
                response.ErrorResponse = new FrontPageRPCError(ex);
            }
            finally
            {
                _rethrow = rethrow;
                _ignoreBusy = false;
                if (document != null && document.FileInfo.Exists)
                    document.FileInfo.Delete();
            }
            return response;
        }

        #endregion

        #region Set Meta Info

        /// <summary>
        /// Updates or adds new meta-information to the designated file.
        /// </summary>
        /// <param name="url">The full document URL.</param>
        /// <param name="fieldName">The MetaInfo field name.</param>
        /// <param name="value">The new value for the field.</param>
        /// <returns></returns>
        public SetMetaInfoResponse SetMetaInfo(string url, string fieldName, object value)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("fieldName");
            MetaInfoCollection metaInfo = new MetaInfoCollection();
            metaInfo.Add(fieldName, value);
            return SetMetaInfo(url, metaInfo);
        }

        /// <summary>
        /// Updates or adds new meta-information to the designated file or folder.
        /// </summary>
        /// <param name="url">The full document URL.</param>
        /// <param name="metaInfo">A collection of MetaInfo data.</param>
        /// <returns></returns>
        public SetMetaInfoResponse SetMetaInfo(string url, MetaInfoCollection metaInfo)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            if (metaInfo == null || metaInfo.Count == 0)
                throw new ArgumentNullException("metaInfo");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;

            ITreeNode node;
            if (urlResponse.IsFile)
                node = new Document(urlResponse.FileUrl);
            else
                node = new Folder(urlResponse.FileUrl);
            node.MetaInfo = metaInfo;

            SetMetaInfoRequest request = new SetMetaInfoRequest(node);

            request.WebUrl = urlResponse.WebUrl;
            return SetMetaInfo(request);
        }

        /// <summary>
        /// Updates or adds new meta-information to the designated file or folder.
        /// </summary>
        /// <param name="document">The document or folder to update.</param>
        /// <returns>SetMetaInfoResponse</returns>
        public SetMetaInfoResponse SetMetaInfo(ITreeNode documentOrFolder)
        {
            SetMetaInfoRequest request = new SetMetaInfoRequest(documentOrFolder);
            return SetMetaInfo(request);
        }

        /// <summary>
        /// Updates or adds new meta-information to the designated file.
        /// </summary>
        /// <param name="document">The document to update.</param>
        /// <returns>SetMetaInfoResponse</returns>
        public SetMetaInfoResponse SetMetaInfo(Document document)
        {
            SetMetaInfoRequest request = new SetMetaInfoRequest(document);
            return SetMetaInfo(request);
        }

        /// <summary>
        /// Updates or adds new meta-information to the designated files.
        /// </summary>
        /// <param name="document">The document to update.</param>
        /// <returns>SetMetaInfoResponse</returns>
        public SetMetaInfoResponse SetMetaInfo(DocumentCollection documents)
        {
            SetMetaInfoRequest request = new SetMetaInfoRequest(documents);
            return SetMetaInfo(request);
        }

        /// <summary>
        /// Updates or adds new meta-information to the designated file or folder.
        /// </summary>
        /// <param name="setMetaInfoRequest">A SetMetaInfoRequest object.</param>
        /// <returns></returns>
        public SetMetaInfoResponse SetMetaInfo(SetMetaInfoRequest setMetaInfoRequest)
        {
            if (setMetaInfoRequest == null)
                throw new ArgumentNullException("request");
            try
            {
                Busy();
                _isAsync = false;
                return (SetMetaInfoResponse)GetResponseInternal(setMetaInfoRequest, new SetMetaInfoResponse(this, setMetaInfoRequest), null); ;
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion

        #region List Documents

        /// <summary>
        /// Provides a method to request a list of files, folders, and Web sites contained in a given folder.
        /// </summary>
        /// <param name="url">The full root url.</param>
        /// <returns></returns>
        public ListDocumentsResponse ListDocuments(string url)
        {
            return ListDocuments(url, true);
        }

        /// <summary>
        /// Provides a method to request a list of files, folders, and Web sites contained in a given folder.
        /// </summary>
        /// <param name="url">The full root url.</param>
        /// <param name="recursive">Set true to list all folders and documents.</param>
        /// <returns></returns>
        public ListDocumentsResponse ListDocuments(string url, bool recursive)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;
            ListDocumentsRequest request = new ListDocumentsRequest();
            request.InitialUrl = urlResponse.FileUrl;
            request.WebUrl = urlResponse.WebUrl;
            return ListDocuments(request);
        }

        /// <summary>
        /// Provides a method to request a list of files, folders, and Web sites contained in a given folder.
        /// </summary>
        /// <param name="listDocumentsRequest"></param>
        /// <returns></returns>
        public ListDocumentsResponse ListDocuments(ListDocumentsRequest listDocumentsRequest)
        {
            if (listDocumentsRequest == null)
                throw new ArgumentNullException("listDocumentsRequest");
            try
            {
                Busy();
                _isAsync = false;
                ListDocumentsResponse result = (ListDocumentsResponse)GetResponseInternal(listDocumentsRequest, new ListDocumentsResponse(this, listDocumentsRequest), null);
                if (result.RootFolder != null)
                    result.RootFolder.DisplayName = listDocumentsRequest.WebUrl;
                return result;
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion

        #region List Versions

        /// <summary>
        /// Retrieves metadata for all the versions of the specified document. This method does not return metadata for the current document.
        /// </summary>
        /// <param name="url">The full root url.</param>
        /// <returns></returns>
        public ListVersionsResponse ListVersions(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;
            Document document = new Document(urlResponse.WebUrl, urlResponse.FileUrl);
            ListVersionsRequest request = new ListVersionsRequest(document);
            return ListVersions(request);
        }

        /// <summary>
        /// Retrieves metadata for all the versions of the specified document. This method does not return metadata for the current document.
        /// </summary>
        /// <param name="document">The document to retrieve metadata for.</param>
        /// <returns></returns>
        public ListVersionsResponse ListVersions(Document document)
        {
            ListVersionsRequest request = new ListVersionsRequest(document);
            return ListVersions(request);
        }

        /// <summary>
        /// Retrieves metadata for all the versions of the specified document. This method does not return metadata for the current document.
        /// </summary>
        /// <param name="listVersionsRequest"></param>
        /// <returns></returns>
        public ListVersionsResponse ListVersions(ListVersionsRequest listVersionsRequest)
        {
            if (listVersionsRequest == null)
                throw new ArgumentNullException("listVersionsRequest");
            try
            {
                Busy();
                _isAsync = false;
                ListVersionsResponse result = (ListVersionsResponse)GetResponseInternal(listVersionsRequest, new ListVersionsResponse(this, listVersionsRequest), null);
                return result;
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion

        #region Server Version

        internal void EnsureWebUrl()
        {
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(this.BaseAddress);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (!urlResponse.HasError)
                this.BaseAddress = urlResponse.WebUrl;
        }

        /// <summary>
        /// Requests the version of Windows SharePoint Services in use on the Web server.
        /// </summary>
        internal GetServerVersionResponse GetServerVersion()
        {
            EnsureWebUrl();
            return GetServerVersion(new GetServerVersionRequest());
        }

        /// <summary>
        /// Requests the version of Windows SharePoint Services in use on the Web server.
        /// </summary>
        /// <param name="getServerVersionRequest"></param>
        /// <returns></returns>
        internal GetServerVersionResponse GetServerVersion(GetServerVersionRequest getServerVersionRequest)
        {
            if (getServerVersionRequest == null)
                throw new ArgumentNullException("getServerVersionRequest");
            try
            {
                Busy();
                _isAsync = false;
                GetServerVersionResponse result = (GetServerVersionResponse)GetResponseInternal(getServerVersionRequest, new GetServerVersionResponse(this, getServerVersionRequest), null);
                return result;
            }
            finally
            {
                _isBusy = false;
            }
        }

        /// <summary>
        /// Requests the version url and version information from the Web server.
        /// </summary>
        internal void GetServerInfo()
        {

            string address = Utils.GetSiteUrl(this.BaseAddress);
            lock (_serverInfos)
            {
                if (_serverInfos.ContainsKey(address))
                {
                    _serverInfo = _serverInfos[address];
                    return;
                }
            }

            WebClient serverVersion = new WebClient(address);
            serverVersion.ServerInfo = _serverInfo;
            serverVersion.Credentials = this.Credentials;
            serverVersion.Authenticate = this.Authenticate;
            GetServerInfoRequest request = new GetServerInfoRequest();
            request.WebClient = serverVersion;

            GetServerInfoResponse response = serverVersion.GetServerInfo(request);
            
            _serverInfo = response.ServerInfo;

            lock (_serverInfos)
            {
                if (!_serverInfos.ContainsKey(address))
                    _serverInfos.Add(address, _serverInfo);

            }

            GetServerVersionResponse versionResponse = serverVersion.GetServerVersion();
            
            _serverInfo.Version = versionResponse.Version;

            this.Credentials = serverVersion.Credentials;
            
        }

        /// <summary>
        /// Requests the version of Windows SharePoint Services in use on the Web server.
        /// </summary>
        /// <param name="getServerInfoRequest"></param>
        /// <returns></returns>
        internal GetServerInfoResponse GetServerInfo(GetServerInfoRequest getServerInfoRequest)
        {
            if (getServerInfoRequest == null)
                throw new ArgumentNullException("getServerInfoRequest");
            try
            {
                Busy();
                _isAsync = false;
                GetServerInfoResponse result = (GetServerInfoResponse)GetResponseInternal(getServerInfoRequest, new GetServerInfoResponse(this, getServerInfoRequest), null);
                return result;
            }
            finally
            {
                _isBusy = false;
            }
        }


        #endregion

        #region DownloadDocument

        /// <summary>
        /// Provides a method to retrieve a document and its meta-information for viewing or editing on a client.
        /// </summary>
        /// <param name="url">The full document url.</param>
        /// <returns></returns>
        public DownloadDocumentResponse DownloadDocument(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");
            return DownloadDocument(url, Path.GetTempFileName());
        }

        /// <summary>
        /// Provides a method to retrieve a document and its meta-information for viewing or editing on a client.
        /// </summary>
        /// <param name="url">The full document url.</param>
        /// <param name="path">The path where the downloaded document should be saved.</param>
        /// <returns></returns>
        public DownloadDocumentResponse DownloadDocument(string url, string path)
        {
            if (url == null)
                throw new ArgumentNullException("url");
            if (path == null)
                throw new ArgumentNullException("path");
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;
            Document document = new Document(urlResponse.FileUrl, new FileInfo(path), null);
            DownloadDocumentRequest request = new DownloadDocumentRequest(document);
            request.WebUrl = urlResponse.WebUrl;
            return DownloadDocument(request);
        }

        /// <summary>
        /// Provides a method to retrieve a document and its meta-information for viewing or editing on a client.
        /// </summary>
        /// <param name="document">The document to download.</param>
        /// <returns></returns>
        public DownloadDocumentResponse DownloadDocument(Document document)
        {
            DownloadDocumentRequest request = new DownloadDocumentRequest(document);
            return DownloadDocument(request);
        }

        /// <summary>
        /// Provides a method to retrieve a document and its meta-information for viewing or editing on a client.
        /// </summary>
        /// <param name="downloadRequest"></param>
        /// <returns></returns>
        public DownloadDocumentResponse DownloadDocument(DownloadDocumentRequest downloadRequest)
        {
            if (downloadRequest == null)
                throw new ArgumentNullException("downloadRequest");
            try
            {
                Busy();
                _isAsync = false;
                return (DownloadDocumentResponse)GetResponseInternal(downloadRequest, new DownloadDocumentResponse(this, downloadRequest), null);
            }
            finally
            {
                _isBusy = false;
            }
        }

        /// <summary>
        /// Provides a method to retrieve a document and its meta-information for viewing or editing on a client.
        /// </summary>
        /// <param name="downloadRequest"></param>
        /// <returns></returns>
        public void DownloadDocumentAsync(DownloadDocumentRequest downloadRequest)
        {
            if (downloadRequest == null)
                throw new ArgumentNullException("downloadRequest");
            DownloadDocumentAsync(downloadRequest, null);
        }

        void DownloadDocumentAsync(DownloadDocumentRequest downloadRequest, object userToken)
        {
            lock (this)
            {
                Busy();
                _isAsync = true;

                _asyncThread = new Thread(delegate(object oArgs)
                {
                    object[] args = (object[])oArgs;
                    DownloadDocumentResponse response;
                    try
                    {
                        response = (DownloadDocumentResponse)GetResponseInternal((RequestBase)args[0], (ResponseBase)args[1], args[2]); 
                        OnDownloadDocumentCompleted(new DownloadDocumentCompletedEventArgs(response, (DownloadDocumentRequest)args[0], null, false, args[2]));
                    }
                    catch (ThreadAbortException)
                    {
                        throw new ThreadInterruptedException();
                    }
                    catch (ThreadInterruptedException)
                    {
                        OnDownloadDocumentCompleted(new DownloadDocumentCompletedEventArgs(null, (DownloadDocumentRequest)args[0], null, true, args[2]));
                    }
                    catch (Exception e)
                    {
                        OnDownloadDocumentCompleted(new DownloadDocumentCompletedEventArgs(null, (DownloadDocumentRequest)args[0], e, false, args[2]));
                    }
                });
                object[] startArgs = new object[] { downloadRequest, new DownloadDocumentResponse(this, downloadRequest), userToken };
                _asyncThread.Start(startArgs);
            }
        }

        protected virtual void OnDownloadDocumentCompleted(DownloadDocumentCompletedEventArgs args)
        {
            FinishAsync();
            if (DownloadDocumentCompleted != null)
                DownloadDocumentCompleted(this, args);
        }

        #endregion

        #region DownloadDocuments

        public DownloadDocumentsResponse DownloadDocuments(DownloadDocumentsRequest downloadRequest)
        {
            if (downloadRequest == null)
                throw new ArgumentNullException("downloadRequest");
            try
            {
                Busy();
                _isAsync = false;
                return (DownloadDocumentsResponse)GetResponseInternal(downloadRequest, new DownloadDocumentsResponse(this, downloadRequest), null);
            }
            finally
            {
                _isBusy = false;
            }
        }

        public void DownloadDocumentsAsync(DownloadDocumentsRequest downloadRequest)
        {
            if (downloadRequest == null)
                throw new ArgumentNullException("downloadRequest");
            DownloadDocumentsAsync(downloadRequest, null);
        }

        void DownloadDocumentsAsync(DownloadDocumentsRequest downloadRequest, object userToken)
        {
            lock (this)
            {
                Busy();
                _isAsync = true;

                _asyncThread = new Thread(delegate(object oArgs)
                {
                    object[] args = (object[])oArgs;
                    DownloadDocumentsResponse response;
                    try
                    {
                        response = (DownloadDocumentsResponse)GetResponseInternal((RequestBase)args[0], (ResponseBase)args[1], args[2]);
                        OnDownloadDocumentsCompleted(new DownloadDocumentsCompletedEventArgs(response, (DownloadDocumentsRequest)args[0], null, false, args[2]));
                    }
                    catch (ThreadAbortException)
                    {
                        throw new ThreadInterruptedException();
                    }
                    catch (ThreadInterruptedException)
                    {
                        OnDownloadDocumentsCompleted(new DownloadDocumentsCompletedEventArgs(null, (DownloadDocumentsRequest)args[0], null, true, args[2]));
                    }
                    catch (Exception e)
                    {
                        OnDownloadDocumentsCompleted(new DownloadDocumentsCompletedEventArgs(null, (DownloadDocumentsRequest)args[0], e, false, args[2]));
                    }
                });
                object[] startArgs = new object[] { downloadRequest, new DownloadDocumentsResponse(this, downloadRequest), userToken };
                _asyncThread.Start(startArgs);
            }
        }

        protected virtual void OnDownloadDocumentsCompleted(DownloadDocumentsCompletedEventArgs args)
        {
            FinishAsync();
            if (DownloadDocumentsCompleted != null)
                DownloadDocumentsCompleted(this, args);
        }

        #endregion

        #region Upload Document

        /// <summary>
        /// Uploads a document to the server.
        /// </summary>
        /// <param name="url">The full URL of the document.</param>
        /// <param name="path">The local path of the file to upload.</param>
        /// <returns></returns>
        public UploadDocumentResponse UploadDocument(string url, string path)
        {
            return UploadDocument(url, path, null);
        }
        
        /// <summary>
        /// Uploads a document to the server.
        /// </summary>
        /// <param name="url">The full URL of the document.</param>
        /// <param name="path">The local path of the file to upload.</param>
        /// <param name="metaInfo">A collection of meta data to upload.</param>
        /// <returns></returns>
        public UploadDocumentResponse UploadDocument(string url, string path, MetaInfoCollection metaInfo)
        {
            return UploadDocument(url, path, metaInfo, PutOptionEnum.Default, false);
        }

        /// <summary>
        /// Uploads a document to the server.
        /// </summary>
        /// <param name="url">The full URL of the document.</param>
        /// <param name="path">The local path of the file to upload.</param>
        /// <param name="metaInfo">A collection of meta data to upload.</param>
        /// <param name="putOption">The server put option flags.</param>
        /// <param name="ensureFolders">Pass true to ensure that all parent folders are created before the upload request is made to the server. The createdirs putoption only creates the first parent folder if it does not exist.</param>
        /// <returns></returns>
        public UploadDocumentResponse UploadDocument(string url, string path, MetaInfoCollection metaInfo, PutOptionEnum putOption, bool ensureFolders)
        {
            UrlToWebUrlRequest urlRequest = new UrlToWebUrlRequest(url);
            UrlToWebUrlResponse urlResponse = UrlToWebUrl(urlRequest);
            if (urlResponse.HasError)
                throw urlResponse.ErrorResponse.Exception;
            Document document = new Document(urlResponse.FileUrl, new FileInfo(path), metaInfo);
            UploadDocumentRequest request = new UploadDocumentRequest(document);
            request.PutOption = putOption;
            request.WebUrl = urlResponse.WebUrl;

            if (ensureFolders)
                CreateFolders(urlResponse.WebUrl, urlResponse.FileUrl);

            return UploadDocument(request);
        }

        public UploadDocumentResponse UploadDocument(UploadDocumentRequest uploadRequest)
        {
            if (uploadRequest == null)
                throw new ArgumentNullException("uploadRequest");

            try
            {
                Busy();
                _isAsync = false;
                return (UploadDocumentResponse)GetResponseInternal(uploadRequest, new UploadDocumentResponse(this, uploadRequest), null);
            }
            finally
            {
                _isBusy = false;
            }
        }

        public void UploadDocumentAsync(UploadDocumentRequest uploadRequest)
        {
            UploadDocumentAsync(uploadRequest, null);
        }

        void UploadDocumentAsync(UploadDocumentRequest uploadRequest, object userToken)
        {
            lock (this)
            {
                Busy();
                _isAsync = true;
                _asyncThread = new Thread(delegate(object oArgs)
                {
                    object[] args = (object[])oArgs;
                    UploadDocumentResponse response;
                    try
                    {
                        response = (UploadDocumentResponse)GetResponseInternal((RequestBase)args[0], (ResponseBase)args[1], args[2]);
                        OnUploadDocumentCompleted(new UploadDocumentCompletedEventArgs(response, (UploadDocumentRequest)args[0], null, false, args[2]));
                    }
                    catch (ThreadAbortException)
                    {
                        throw new ThreadInterruptedException();
                    }
                    catch (ThreadInterruptedException)
                    {
                        OnUploadDocumentCompleted(new UploadDocumentCompletedEventArgs(null, (UploadDocumentRequest)args[0], null, true, args[2]));
                    }
                    catch (Exception e)
                    {
                        OnUploadDocumentCompleted(new UploadDocumentCompletedEventArgs(null, (UploadDocumentRequest)args[0], e, false, args[2]));
                    }
                });
                object[] startArgs = new object[] { uploadRequest, new UploadDocumentResponse(this, uploadRequest), userToken };
                _asyncThread.Start(startArgs);
            }
        }

        protected virtual void OnUploadDocumentCompleted(UploadDocumentCompletedEventArgs args)
        {
            FinishAsync();
            if (UploadDocumentCompleted != null)
                UploadDocumentCompleted(this, args);
        }

        #endregion

        #region Upload Documents

        public UploadDocumentsResponse UploadDocuments(UploadDocumentsRequest uploadRequest)
        {
            if (uploadRequest == null)
                throw new ArgumentNullException("uploadRequest");
            try
            {
                Busy();
                _isAsync = false;
                return (UploadDocumentsResponse)GetResponseInternal(uploadRequest, new UploadDocumentsResponse(this, uploadRequest), null);
            }
            finally
            {
                _isBusy = false;
            }
        }

        public void UploadDocumentsAsync(UploadDocumentsRequest uploadRequest)
        {
            if (uploadRequest == null)
                throw new ArgumentNullException("uploadRequest");
            UploadDocumentsAsync(uploadRequest, null);
        }

        void UploadDocumentsAsync(UploadDocumentsRequest uploadRequest, object userToken)
        {
            lock (this)
            {
                Busy();
                _isAsync = true;
                _rethrow = true;
                _asyncThread = new Thread(delegate(object oArgs)
                {
                    object[] args = (object[])oArgs;
                    UploadDocumentsResponse response;
                    try
                    {
                        response = (UploadDocumentsResponse)GetResponseInternal((RequestBase)args[0], (ResponseBase)args[1], args[2]);
                        OnUploadDocumentsCompleted(new UploadDocumentsCompletedEventArgs(response, (UploadDocumentsRequest)args[0], null, false, args[2]));
                    }
                    catch (ThreadAbortException)
                    {
                        throw new ThreadInterruptedException();
                    }
                    catch (ThreadInterruptedException)
                    {
                        OnUploadDocumentsCompleted(new UploadDocumentsCompletedEventArgs(null, (UploadDocumentsRequest)args[0], null, true, args[2]));
                    }
                    catch (Exception e)
                    {
                        OnUploadDocumentsCompleted(new UploadDocumentsCompletedEventArgs(null, (UploadDocumentsRequest)args[0], e, false, args[2]));
                    }
                });
                object[] startArgs = new object[] { uploadRequest, new UploadDocumentsResponse(this, uploadRequest), userToken };
                _asyncThread.Start(startArgs);
            }
        }

        protected virtual void OnUploadDocumentsCompleted(UploadDocumentsCompletedEventArgs args)
        {
            FinishAsync();
            if (UploadDocumentsCompleted != null)
                UploadDocumentsCompleted(this, args);
        }

        #endregion

        #endregion

        #region Private Implementation

        protected void FinishAsync()
        {
            lock (this)
            {
                _asyncThread = null;
                _isBusy = false;
            }
        }

        internal string Boundary
        {
            get { return _boundary; }
        }

        protected void Busy()
        {
            lock (this)
            {
                if (_isBusy && !_ignoreBusy)
                    throw new NotSupportedException("WebClient is already busy.");
                _isBusy = true;
            }
        }

        protected void NotBusy()
        {
            _isBusy = false;
        }

        protected WebRequest GetRequest(RequestBase request)
        {
            return GetRequest(request.Address, request.IsMultiPart, request.HttpMethod);
        }

        protected WebRequest GetRequest(Uri uri, bool isMultiPart)
        {
            return GetRequest(uri, isMultiPart, "POST");
        }

        protected WebRequest GetRequest(Uri uri, bool isMultiPart, string method)
        {
            _cancelled = false;
            WebRequest request = WebRequest.Create(uri);
            request.Credentials = Credentials;
            request.Timeout = this.Timeout;
            if (Proxy != null)
                request.Proxy = Proxy;

            if (!isMultiPart)
            {
                _boundary = "";
                if (method == "POST")
                {
                    request.ContentType = "application/x-vermeer-urlencoded";
                    request.Headers["X-Vermeer-Content-Type"] = "application/x-vermeer-urlencoded";
                }
            }
            else
            {
                string boundary = string.Format("MIMEboundary-{0}-{1}", DateTime.Now.ToString("dd MMM yyyy hh:mm:ss -0000"), DateTime.Now.Ticks.ToString("x"));
                request.ContentType = "multipart/mixed; boundary=\"" + boundary + "\"";
                request.Headers["X-Vermeer-Content-Type"] = "multipart/mixed; boundary=\"" + boundary + "\"";
                _boundary = "--" + boundary;
            }
            request.Method = method;
            return request;
        }

        internal void UpdateMoveProgress(ref long running, long totalFiles)
        {
            if (_isAsync && MoveProgressChanged != null)
            {
                running += 1;
                if (running > totalFiles) running = totalFiles;
                MoveProgressChanged(this, new MoveProgressChangedEventArgs(running, totalFiles, null));
            }
        }

        internal void UpdateRemoveProgress(ref long running, long totalFiles)
        {
            if (_isAsync && RemoveProgressChanged != null)
            {
                running += 1;
                if (running > totalFiles) running = totalFiles;
                RemoveProgressChanged(this, new RemoveProgressChangedEventArgs(running, totalFiles, null));
            }
        }

        internal void UpdateDownloadProgress(ref long running, long total, int increment)
        {
            if (_isAsync && DownloadProgressChanged != null)
            {
                running += increment;
                _notifyCount += increment;
                if (_notifyCount >= _notifyEveryBytes)
                {
                    if (running > total) running = total;
                    DownloadProgressChanged(this, new DownloadProgressChangedEventArgs(running, total, null));
                    _notifyCount = 0;
                }
            }
        }

        internal void UpdateUploadProgress(ref long running, long total, int increment)
        {
            if (_isAsync && UploadProgressChanged != null)
            {
                running += increment;
                _notifyCount += increment;
                if (_notifyCount >= _notifyEveryBytes)
                {
                    if (running > total) running = total;
                    UploadProgressChanged(this, new UploadProgressChangedEventArgs(running, total, null));
                    _notifyCount = 0;
                }
            }
        }

        internal static string GetStringFromStream(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        ResponseBase GetResponseInternal(RequestBase rpcRequest, ResponseBase rpcResponse, object userToken)
        {


            WebRequest request = null;
            Stream requestStream = null;
            Stream responseStream = null;

            try
            {
                if (string.IsNullOrEmpty(this.BaseAddress))
                    BaseAddress = rpcRequest.WebUrl;

                rpcRequest.WebClient = this;

                request = GetRequest(rpcRequest);

                request.ContentLength = rpcRequest.ContentLength;

                if (rpcRequest.HttpMethod != "GET")
                {
                    requestStream = request.GetRequestStream();

                    rpcRequest.WriteRequest(requestStream);
                }

                WebResponse response = request.GetResponse();

                responseStream = response.GetResponseStream();

                rpcResponse.ReadResponse(responseStream, response.ContentLength);

                Thread.Sleep(1);

                return rpcResponse;

            }
            catch (ThreadAbortException)
            {
                throw new ThreadInterruptedException();
            }
            catch (ThreadInterruptedException)
            {
                if (request != null)
                    request.Abort();
                throw;
            }
            catch (WebException webEx)
            {
                HttpWebResponse errorResponse = (HttpWebResponse)webEx.Response;
                if (errorResponse == null)
                    throw new FrontPageRPCException(webEx.Message, new ArgumentException());
                switch (errorResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        if (rpcRequest is GetServerInfoRequest)
                            throw new FrontPageRPCException(webEx.Message, null, new FrontPageRPCError(webEx));
                        throw;
                    case HttpStatusCode.Unauthorized:
                        if (Authenticate == null)
                            throw new FrontPageRPCException(webEx.Message, null, new FrontPageRPCError(webEx));

                        AuthenticateEventArgs authenticateEventArgs = new AuthenticateEventArgs();
                        authenticateEventArgs.WebUrl = rpcRequest.WebUrl;

                        Authenticate(this, authenticateEventArgs);
                        if (!authenticateEventArgs.Handled)
                            throw new FrontPageRPCException(webEx.Message, new ArgumentException());

                        this.Credentials = authenticateEventArgs.Credentials;
                        this.BaseAddress = authenticateEventArgs.WebUrl;

                        if (requestStream != null)
                            requestStream.Close();
                        if (responseStream != null)
                            responseStream.Close();

                        return GetResponseInternal(rpcRequest, rpcResponse, userToken);
                    default:
                        throw;
                }
            }
            catch (Exception ex)
            {
                if (ex is FrontPageRPCException)
                {
                    if (_rethrow)
                        throw ex;
                    rpcResponse.ErrorResponse = ((FrontPageRPCException)ex).FrontPageRPCError;
                    return rpcResponse;
                }
                if (ex.InnerException != null && ex.InnerException is ThreadAbortException)
                    throw new ThreadInterruptedException();
                else
                    throw;
            }
            finally
            {
                if (requestStream != null)
                    requestStream.Close();
                if (responseStream != null)
                    responseStream.Close();
            }

        }

        #endregion
    }

    # region delegates and eventArgs

    public delegate void MoveDocumentsCompletedEventHandler(object sender, MoveDocumentsCompletedEventArgs e);
    public delegate void MoveProgressChangedEventHandler(object sender, MoveProgressChangedEventArgs e);
    public delegate void RemoveDocumentsCompletedEventHandler(object sender, RemoveDocumentsCompletedEventArgs e);
    public delegate void RemoveProgressChangedEventHandler(object sender, RemoveProgressChangedEventArgs e);
    public delegate void DownloadDocumentCompletedEventHandler(object sender, DownloadDocumentCompletedEventArgs e);
    public delegate void UploadDocumentCompletedEventHandler(object sender, UploadDocumentCompletedEventArgs e);
    public delegate void DownloadDocumentsCompletedEventHandler(object sender, DownloadDocumentsCompletedEventArgs e);
    public delegate void UploadDocumentsCompletedEventHandler(object sender, UploadDocumentsCompletedEventArgs e);
    public delegate void UploadDocumentRequestedEventHandler(object sender, UploadDocumentRequestedEventArgs e);
    public delegate void UploadDocumentsRequestedEventHandler(object sender, UploadDocumentsRequestedEventArgs e);
    public delegate void DownloadDocumentRequestedEventHandler(object sender, DownloadDocumentRequestedEventArgs e);
    public delegate void DownloadDocumentsRequestedEventHandler(object sender, DownloadDocumentsRequestedEventArgs e);
    public delegate void UploadProgressChangedEventHandler(object sender, UploadProgressChangedEventArgs e);
    public delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);
    public delegate void AuthenticateEventHandler(object sender, AuthenticateEventArgs e);

    public class AuthenticateEventArgs : System.EventArgs
    {
        private System.Net.ICredentials _credentials;
        private bool _handled;
        private string _webUrl;

        public AuthenticateEventArgs()
        {
            this._handled = false;
        }

        public string WebUrl
        {
            get { return _webUrl; }
            set { _webUrl = value; }
        }

        public bool Handled
        {
            get { return _handled; }
            set { _handled = value; }
        }

        public System.Net.ICredentials Credentials
        {
            get { return _credentials; }
            set { _credentials = value; }
        }
    }


    public class MoveProgressChangedEventArgs : ProgressChangedEventArgs
    {
        internal MoveProgressChangedEventArgs(long filesMoved, long totalFilesToMove, object userState)
            : base(totalFilesToMove != -1 ? ((int)(filesMoved * 100 / totalFilesToMove)) : 0, userState)
        {
            this.filesMoved = filesMoved;
            this.totalFilesToMove = totalFilesToMove;
        }

        long filesMoved, totalFilesToMove;

        public long FilesMoved
        {
            get { return filesMoved; }
        }

        public long TotalFilesToMove
        {
            get { return totalFilesToMove; }
        }
    }

    public class MoveDocumentsCompletedEventArgs : AsyncCompletedEventArgs
    {
        internal MoveDocumentsCompletedEventArgs(MoveDocumentsResponse result,
            MoveDocumentsRequest request,
            Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            this.result = result;
            this.request = request;
        }

        MoveDocumentsResponse result;

        public MoveDocumentsResponse Result
        {
            get { return result; }
        }

        MoveDocumentsRequest request;

        public MoveDocumentsRequest Request
        {
            get { return request; }
        }
    }


    public class RemoveProgressChangedEventArgs : ProgressChangedEventArgs
    {
        internal RemoveProgressChangedEventArgs(long filesRemoved, long totalFilesToRemove, object userState)
            : base(totalFilesToRemove != -1 ? ((int)(filesRemoved * 100 / totalFilesToRemove)) : 0, userState)
        {
            this.filesRemoved = filesRemoved;
            this.totalFilesToRemove = totalFilesToRemove;
        }

        long filesRemoved, totalFilesToRemove;

        public long FilesRemoved
        {
            get { return filesRemoved; }
        }

        public long TotalFilesToRemove
        {
            get { return totalFilesToRemove; }
        }
    }

    public class RemoveDocumentsCompletedEventArgs : AsyncCompletedEventArgs
    {
        internal RemoveDocumentsCompletedEventArgs(RemoveDocumentsResponse result,
            RemoveDocumentsRequest request,
            Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            this.result = result;
            this.request = request;
        }

        RemoveDocumentsResponse result;

        public RemoveDocumentsResponse Result
        {
            get { return result; }
        }

        RemoveDocumentsRequest request;

        public RemoveDocumentsRequest Request
        {
            get { return request; }
        }
    }



    public class DownloadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        internal DownloadProgressChangedEventArgs(long bytesReceived, long totalBytesToReceive, object userState)
            : base(totalBytesToReceive != -1 ? ((int)(bytesReceived * 100 / totalBytesToReceive)) : 0, userState)
        {
            this.received = bytesReceived;
            this.total = totalBytesToReceive;
        }

        long received, total;

        public long BytesReceived
        {
            get { return received; }
        }

        public long TotalBytesToReceive
        {
            get { return total; }
        }
    }

    public class UploadProgressChangedEventArgs : ProgressChangedEventArgs
    {
        internal UploadProgressChangedEventArgs(long bytesReceived, long totalBytesToReceive, object userState)
            : base(totalBytesToReceive != -1 ? ((int)(bytesReceived * 100 / totalBytesToReceive)) : 0, userState)
        {
            this.received = bytesReceived;
            this.total = totalBytesToReceive;
        }

        long received, total;

        public long BytesReceived
        {
            get { return received; }
        }

        public long TotalBytesToReceive
        {
            get { return total; }
        }
    }

    public class UploadDocumentRequestedEventArgs : EventArgs
    {
        internal UploadDocumentRequestedEventArgs(UploadDocumentRequest request)
            : base()
        {
            this.request = request;
        }

        UploadDocumentRequest request;

        public UploadDocumentRequest Request
        {
            get { return request; }
        }
    }

    public class UploadDocumentsRequestedEventArgs : EventArgs
    {
        internal UploadDocumentsRequestedEventArgs(UploadDocumentsRequest request)
            : base()
        {
            this.request = request;
        }

        UploadDocumentsRequest request;

        public UploadDocumentsRequest Request
        {
            get { return request; }
        }
    }

    public class DownloadDocumentRequestedEventArgs : EventArgs
    {
        internal DownloadDocumentRequestedEventArgs(DownloadDocumentRequest request)
            : base()
        {
            this.request = request;
        }

        DownloadDocumentRequest request;

        public DownloadDocumentRequest Request
        {
            get { return request; }
        }
    }

    public class DownloadDocumentsRequestedEventArgs : EventArgs
    {
        internal DownloadDocumentsRequestedEventArgs(DownloadDocumentsRequest request)
            : base()
        {
            this.request = request;
        }

        DownloadDocumentsRequest request;

        public DownloadDocumentsRequest Request
        {
            get { return request; }
        }
    }

    public class DownloadDocumentCompletedEventArgs : AsyncCompletedEventArgs
    {
        internal DownloadDocumentCompletedEventArgs(DownloadDocumentResponse result,
            DownloadDocumentRequest request,
            Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            this.result = result;
            this.request = request;
        }

        DownloadDocumentResponse result;

        public DownloadDocumentResponse Result
        {
            get { return result; }
        }

        DownloadDocumentRequest request;

        public DownloadDocumentRequest Request
        {
            get { return request; }
        }
    }

    public class UploadDocumentCompletedEventArgs : AsyncCompletedEventArgs
    {
        internal UploadDocumentCompletedEventArgs(UploadDocumentResponse result,
            UploadDocumentRequest request,
            Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            this.result = result;
            this.request = request;
        }

        UploadDocumentResponse result;

        public UploadDocumentResponse Result
        {
            get { return result; }
        }

        UploadDocumentRequest request;

        public UploadDocumentRequest Request
        {
            get { return request; }
        }
    }

    public class DownloadDocumentsCompletedEventArgs : AsyncCompletedEventArgs
    {
        internal DownloadDocumentsCompletedEventArgs(DownloadDocumentsResponse result,
            DownloadDocumentsRequest request,
            Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            this.result = result;
            this.request = request;
        }

        DownloadDocumentsResponse result;

        public DownloadDocumentsResponse Result
        {
            get { return result; }
        }

        DownloadDocumentsRequest request;

        public DownloadDocumentsRequest Request
        {
            get { return request; }
        }
    }

    public class UploadDocumentsCompletedEventArgs : AsyncCompletedEventArgs
    {
        internal UploadDocumentsCompletedEventArgs(UploadDocumentsResponse result,
            UploadDocumentsRequest request,
            Exception error, bool cancelled, object userState)
            : base(error, cancelled, userState)
        {
            this.result = result;
            this.request = request;
        }

        UploadDocumentsResponse result;

        public UploadDocumentsResponse Result
        {
            get { return result; }
        }

        UploadDocumentsRequest request;

        public UploadDocumentsRequest Request
        {
            get { return request; }
        }
    }

    #endregion
}
