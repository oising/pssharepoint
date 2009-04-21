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
using System.Xml.XPath;

namespace HubKey.Net.FrontPageRPC
{
    public class FrontPageRPCException : Exception
    {
        FrontPageRPCError _rpcError = null;

        internal FrontPageRPCException(string message)
            : this(message, null, null)
        {
        }

        internal FrontPageRPCException(string message, Exception innerException)
            : this(message, innerException, null)
        {
        }

        internal FrontPageRPCException(string message, Exception innerException, FrontPageRPCError rpcError)
            : base(message, innerException)
        {
            if (rpcError == null)
                rpcError = new FrontPageRPCError(-1, message);
            _rpcError = rpcError;
        }

        public FrontPageRPCError FrontPageRPCError
        {
            get { return _rpcError; }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this._rpcError.Msg))
                return this._rpcError.Msg;
            else if (this.InnerException == null)
                return base.ToString();
            else
                return base.ToString() + "\r\n\r\n" + InnerException.ToString();
        }
    }

    public class FrontPageRPCError
    {
        public int Status = 0;
        public int OSStatus = 0;
        public string Msg = "";
        public string OSMessage = "";
        public Exception Exception = null;

        internal FrontPageRPCError(int status, string msg)
        {
            Status = status;
            Msg = msg;
        }

        internal FrontPageRPCError(Exception ex)
            : this(-1, ex.Message)
        {
            Exception = ex;
        }

        internal FrontPageRPCError(XPathNavigator nav)
        {
            if (nav == null)
                return;
            XPathNavigator error = nav.SelectSingleNode("//ul[@name='status']");
            if (error != null)
            {
                Status = Convert.ToInt32(error.SelectSingleNode("li[@name='status']").Value);
                OSStatus = Convert.ToInt32(error.SelectSingleNode("li[@name='osstatus']").Value);
                Msg = error.SelectSingleNode("li[@name='msg']").Value;
                OSMessage = error.SelectSingleNode("li[@name='osmsg']").Value;
            }
        }
    }
}
