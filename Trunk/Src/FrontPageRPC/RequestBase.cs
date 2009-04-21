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
using System.Net;
using System.IO;

namespace HubKey.Net.FrontPageRPC
{
    public abstract class RequestBase : IRequest
    {
        protected string _webUrl;
        internal ParameterCollection _parameters;
        protected long _contentLength = 0;
        protected byte[] _requestBytes;
        protected bool _useFrontPageRPCWebRequest = false;
        protected WebClient _wc;
        protected bool _isMultiPart = false;

        public RequestBase()
        {
        }

        /// <summary>
        /// Gets or sets the website URL.
        /// </summary>
        public string WebUrl
        {
            get 
            {
                if (string.IsNullOrEmpty(_webUrl))
                    return _wc.BaseAddress;
                return _webUrl; 
            }
            set { _webUrl = value; }
        }

        public abstract string MethodName { get; }

        /// <summary>
        /// Gets the FPSE Version.
        /// </summary>
        public virtual string Version
        {
            get { return _wc.ServerInfo.Version; }
        }

        /// <summary>
        /// Gets the FPSE dll path.
        /// </summary>
        public virtual string ServiceURL
        {
            get { return _wc.ServerInfo.AuthorScriptUrl; }
        }

        /// <summary>
        /// Gets the http method.
        /// </summary>
        public virtual string HttpMethod
        {
            get { return "POST"; }
        }

        /// <summary>
        /// Gets the URI of the web request.
        /// </summary>
        public virtual Uri Address
        {
            get
            {
                return new Uri((_useFrontPageRPCWebRequest ? "rpc:" : "") + Utils.CombineUrl(WebUrl, ServiceURL));
            }
        }

        /// <summary>
        /// Gets or sets whether to use the FrontPageRPCWebRequest object instead of HTTPWebRequest. Default for Uploading documents.
        /// </summary>
        /// <remarks>FrontPageRPCWebRequest allows streaming from a filestream rather than loading into memory first. Setting HTTPWebRequest.AllowWriteStreamBuffering = false has an issue
        /// described at http://support.microsoft.com/kb/908573 which seems to be difficult to avoid with FPSE FrontPageRPC calls.
        /// </remarks>
        public virtual bool UseFrontPageRPCWebRequest
        {
            get { return _useFrontPageRPCWebRequest; }
            set { _useFrontPageRPCWebRequest = value; }
        }

        /// <summary>
        /// Gets the request content length.
        /// </summary>
        public virtual long ContentLength
        {
            get
            {
                return RequestBytes.Length;
            }
        }

        internal WebClient WebClient
        {
            get { return _wc; }
            set { _wc = value; }
        }

        internal bool IsMultiPart
        {
            get { return _isMultiPart; }
        }



        protected virtual byte[] RequestBytes
        {
            get
            {
                if (_requestBytes == null)
                    _requestBytes = Parameters.ToByteArray();
                return _requestBytes;
            }
            set { _requestBytes = value; }
        }

        internal abstract ParameterCollection Parameters { get; }

        internal virtual void WriteRequest(Stream requestStream)
        {
            try
            {
                int nread;
                byte[] buffer = new byte[4096];

                MemoryStream pStream = new MemoryStream(RequestBytes);

                while ((nread = pStream.Read(buffer, 0, 4096)) != 0)
                    requestStream.Write(buffer, 0, nread);
            }
            finally
            {
                RequestBytes = null;
            }
        }
    }

}
