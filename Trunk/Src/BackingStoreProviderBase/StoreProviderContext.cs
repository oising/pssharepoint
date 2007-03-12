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
