#region BSD License Header

/*
 * Copyright (c) 2006, Oisin Grehan @ Nivot Inc (www.nivot.org)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
 * Neither the name of Nivot Incorporated nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#endregion

// original author: Jachym Kouba
// (a colleague from the PowerShellCX project)

using System;
using System.Management.Automation.Provider;

namespace Nivot.PowerShell
{
    public static class StoreProviderContext<TProvider> where TProvider : CmdletProvider
    {
        [ThreadStatic]
        private static TProvider _current;

        public static TProvider Current
        {
            get { return _current; }
        }

        public static Cookie Enter(TProvider provider)
        {
            if (provider == null)
            {
            	throw new ArgumentNullException("provider");
            }

            Cookie cookie = new Cookie(_current);
            _current = provider;

            return cookie;
        }

        public struct Cookie : IDisposable
        {
            internal readonly TProvider _previous;

            internal Cookie (TProvider previous)
	        {
                _previous = previous;
	        }

            public void Dispose()
            {
                _current = _previous;
            }
        }
    }
}
