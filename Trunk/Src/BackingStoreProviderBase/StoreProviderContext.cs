#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

// original author: Jachym Kouba (powershellcx project)
// changes: removed generic provider type parameter (redundant here)

using System;
using System.Diagnostics;
using System.Management.Automation.Provider;

namespace Nivot.PowerShell
{
    public static class StoreProviderContext
    {
        [ThreadStatic]
        private static StoreProviderBase _current;

        public static StoreProviderBase Current
        {
            get {
                Debug.Assert(_current != null,
                             "Current provider context is null! did you forget to call EnterContext() in the calling method?");

                return _current;
            }
        }

        public static Cookie Enter(StoreProviderBase provider)
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
            internal readonly StoreProviderBase _previous;

            internal Cookie(StoreProviderBase previous)
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
