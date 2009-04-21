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
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Reflection;
using System.Threading;

namespace HubKey.Net.FrontPageRPC
{

    public class FrontPageRPCWebRequest : WebRequest
    {
        public static readonly int DefaultTimeout = 1000 * 60;
        public static readonly int HeaderReadTimeoutSeconds = 10;
        private static readonly char[] HeaderWhitespaceChars = new char[] { ' ', '\t' };
        private static BindingFlags NonPublicInstance = BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic;
        private static BindingFlags SetProperty = NonPublicInstance | BindingFlags.SetProperty;
        private static BindingFlags GetProperty = NonPublicInstance | BindingFlags.GetProperty;
        private static BindingFlags SetField = NonPublicInstance | BindingFlags.SetField;
        private ICredentials _credentials;
        private WebHeaderCollection _headers;
        private WebProxy _proxy;
        private Uri _origianlUri;
        internal Uri _uri;
        private string _connGroup;
        private long _contentLength;
        private string _contentType;
        private string _method;
        private string _body;
        private bool _preAuthen;
        private int _timeout;
        internal TcpClient _tpcClient;
        private RemoteCertificateValidationCallback _validationCallback;
        private Stream _stream;
        internal Stream _writeStream;
        internal Stream _readStream;
        private int _receiveBufferSize;
        private int _sendBufferSize;
        private WebHeaderCollection _responseHeaders;
        private HttpWebRequest _httpWebRequest;
        private string _lastResponse;
        private int _attempts = 0;

        internal FrontPageRPCWebRequest(Uri uri)
        {
            _headers = new WebHeaderCollection();
            _origianlUri = uri;
            _uri = new Uri(uri.AbsolutePath);
            _method = "POST";
            _body = "";
            _contentLength = 0;
            _timeout = DefaultTimeout;
            if (IsSecure)
                _validationCallback = new RemoteCertificateValidationCallback(OnCertificateValidation);
        }

        TcpClient TpcClient
        {
            get 
            { 
                if (_tpcClient == null)
                    _tpcClient = new TcpClient(_uri.DnsSafeHost, _uri.Port);
                return _tpcClient; 
            }
        }

        HttpWebRequest HttpWebRequest
        {
            get
            {
                if (_httpWebRequest == null)
                {
                    _httpWebRequest = (HttpWebRequest)WebRequest.Create(_uri.AbsoluteUri);
                    _httpWebRequest.Credentials = Credentials;
                    
                    Type tWebReq = _httpWebRequest.GetType();

                    tWebReq.InvokeMember("Async", SetProperty, null, _httpWebRequest, new object[] { false });

                    object objServerAuthState = tWebReq.InvokeMember("ServerAuthenticationState", GetProperty, null, _httpWebRequest, new object[0]);

                    objServerAuthState.GetType().InvokeMember("ChallengedUri", SetField, null, objServerAuthState, new object[] { new Uri(_uri.AbsoluteUri) });
                }
                return _httpWebRequest;
            }
        }

        public bool IsSecure
        {
            get { return _uri.Port == 443; }
        }

        public override string ConnectionGroupName
        {
            get { return _connGroup; }
            set { _connGroup = value; }
        }
        public override long ContentLength
        {
            get { return _contentLength; }
            set { _contentLength = value; }
        }
        public override string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }
        public override ICredentials Credentials
        {
            get
            {
                if (_credentials == null)
                {
                    _credentials = (NetworkCredential)CredentialCache.DefaultCredentials; 
                }
                return _credentials; 
            }
            set
            {
                _credentials = value;
            }
        }
        public override WebHeaderCollection Headers
        {
            get { return _headers; }
            set { _headers = value; }
        }
        public override string Method
        {
            get { return _method; }
            set { _method = value; }
        }
        public override bool PreAuthenticate
        {
            get { return _preAuthen; }
            set { _preAuthen = value; }
        }
        public override IWebProxy Proxy
        {
            get { return _proxy; }
            set { _proxy = (WebProxy)value; }
        }
        public override Uri RequestUri
        {
            get { return _origianlUri; }
        }
        public override int Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
        public override void Abort()
        {
        }
        public override IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }
        public override IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }
        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
        public override Stream GetRequestStream()
        {
            string authMessage = null;
            return GetRequestStream(authMessage);
        }
        
        public Stream GetRequestStream(string authMessage)
        {
            OpenConnection();

            WriteHeaders(true, true);
            ParseResponseHeaders(ReadResponseString());
            string challenge = _responseHeaders["WWW-Authenticate"];

            if (string.IsNullOrEmpty(challenge))
            {
                WriteHeaders(false, false);
                return _writeStream;
            }

            if (authMessage == null)
                authMessage = GetAuthorizationMessage(challenge);
            Headers["Authorization"] = authMessage;

            if (!challenge.StartsWith("ntlm", StringComparison.OrdinalIgnoreCase))
            {
                WriteHeaders(false, false);
                return _writeStream;
            }

            WriteHeaders(true, true);
            ParseResponseHeaders(ReadResponseString());

            challenge = _responseHeaders["WWW-Authenticate"];

            if (challenge == null && ++_attempts < 2)
            {
                return GetRequestStream(authMessage);
            }

            Headers["Authorization"] = GetAuthorizationMessage(challenge);

            WriteHeaders(false, false);
            return _writeStream;
        }

        void OpenConnection()
        {
            if (_tpcClient != null)
            {
                _tpcClient.Close();
                _tpcClient = null;
            }
            if (IsSecure)
            {
                SslStream sslStream = new SslStream(TpcClient.GetStream(), false, _validationCallback);
                sslStream.AuthenticateAsClient(_uri.DnsSafeHost);
                _stream = (Stream)sslStream;
            }
            else
            {
                NetworkStream stream = TpcClient.GetStream();
                _stream = (Stream)stream;
            }

            _headers["Host"] = _uri.DnsSafeHost;
            _tpcClient.SendTimeout = _tpcClient.ReceiveTimeout = _timeout;
            _receiveBufferSize = _tpcClient.ReceiveBufferSize;
            _sendBufferSize = _tpcClient.SendBufferSize;
            _writeStream = new BufferedStream(_stream, _sendBufferSize);
            _readStream = new BufferedStream(_stream, _receiveBufferSize);
        }

        public override WebResponse GetResponse()
        {
            if (_writeStream == null || !_writeStream.CanWrite)
                throw new Exception("Request stream needs to stay open.");
            _writeStream.Flush();
            ParseResponseHeaders(ReadHeader());
            FrontPageRPCWebResponse response = new FrontPageRPCWebResponse(this);
            return response;
        }



        void WriteHeaders(bool flush, bool zeroContentLength)
        {
            StringBuilder sb = new StringBuilder();
            Headers["Content-Length"] = zeroContentLength? "0" : _contentLength.ToString();
            Headers["Content-Type"] = _contentType;
            sb.Append(_method);
            sb.Append(" ");
            sb.Append(_uri.PathAndQuery);
            sb.Append(" HTTP/1.1\r\n");
            sb.Append(_headers);
            sb.Append(_body);
            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            _writeStream.Write(buffer, 0, buffer.Length);
            if (flush) _writeStream.Flush();
        }

        string ReadResponseString()
        {
            StringBuilder sb = new StringBuilder();
            byte[] buffer = new byte[_receiveBufferSize];
            int nread = 0;
            while ((nread = _readStream.Read(buffer, 0, _receiveBufferSize)) != 0)
            {
                sb.Append(Encoding.UTF8.GetString(buffer, 0, nread));
                if (_tpcClient.Available == 0) break;
            }
            _lastResponse = sb.ToString();
            return _lastResponse;
        }

        string ReadHeader()
        {
            StringBuilder sb = new StringBuilder();
            byte[] buffer = new byte[4];
            int nread = 0;
            DateTime start = DateTime.Now;
            while (_tpcClient.Available == 0)
            {
                Thread.Sleep(50);
                if (DateTime.Now.Subtract(start).TotalSeconds >= HeaderReadTimeoutSeconds)
                    throw new WebException("The server timed out while waiting for response headers.", WebExceptionStatus.Timeout);
            }
            while ((nread = _readStream.ReadByte()) != -1)
            {
                buffer[0] = buffer[1];
                buffer[1] = buffer[2];
                buffer[2] = buffer[3];
                buffer[3] = (byte)nread;
                sb.Append((char)nread);
                if (buffer[0] == (byte)'\r' && buffer[1] == (byte)'\n' && buffer[2] == buffer[0] && buffer[3] == buffer[1])
                    break;
            }
            _lastResponse = sb.ToString();
            return _lastResponse;
        }

        internal WebHeaderCollection ResponseHeaders
        {
            get { return _responseHeaders; }
        }

        void ParseResponseHeaders(string response)
        {
            _responseHeaders = new WebHeaderCollection();
            StringReader sr = new StringReader(response);
            string line = string.Empty;
            string lastHeader = string.Empty;
            while (!string.IsNullOrEmpty((line = sr.ReadLine())))
            {
                if (line.StartsWith(" ") || line.StartsWith("\t"))
                {
                    _responseHeaders[lastHeader] = string.Concat(_responseHeaders[lastHeader], line);
                    continue;
                }
                int separatorIndex = line.IndexOf(':');
                if (separatorIndex < 0)
                    continue;

                string headerName = line.Substring(0, separatorIndex);
                string headerValue = line.Substring(separatorIndex + 1).Trim(HeaderWhitespaceChars);

                _responseHeaders[headerName] = headerValue;
                lastHeader = headerName;
            }
        }

        string GetAuthorizationMessage(string challenge)
        {
            Authorization auth = AuthenticationManager.Authenticate(challenge, HttpWebRequest, Credentials);
            return auth.Message;
        }

        private static bool OnCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors != SslPolicyErrors.None && errors != SslPolicyErrors.RemoteCertificateNameMismatch)
                throw new PolicyException(errors.ToString());
            return true;
        }



    }

    public class FrontPageRPCWebRequestCreate : IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            FrontPageRPCWebRequest request = new FrontPageRPCWebRequest(uri);
            return request;
        }
    }

    public class FrontPageRPCWebResponse : WebResponse
    {

        private WebHeaderCollection _headers;
        private Uri _uri;
        private long _contentLength;
        private string _contentType;
        private FrontPageRPCWebRequest _request;
        internal FrontPageRPCWebResponse()
        {
        }

        internal FrontPageRPCWebResponse(FrontPageRPCWebRequest request)
        {
            _request = request;
            _headers = _request.ResponseHeaders;
            _uri = _request._uri;
        }

        public override long ContentLength
        {
            get 
            {
                _contentLength = 0;
                string contentLength = Headers["Content-Length"];
                if (!string.IsNullOrEmpty(contentLength))
                    long.TryParse(contentLength, out _contentLength);
                if (_contentLength == 0)
                    _contentLength = (long)_request._tpcClient.Available;
                if (_contentLength == 0)
                    _contentLength = (long)_request._tpcClient.ReceiveBufferSize;
                return _contentLength;
            }
        }

        public override string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }
        public override WebHeaderCollection Headers
        {
            get { return _headers; }
        }
        public override Uri ResponseUri
        {
            get { return _uri; }
        }

        public override void Close()
        {
            base.Close();
        }
        public override Stream GetResponseStream()
        {
            return _request._readStream;
        }
    }


}
