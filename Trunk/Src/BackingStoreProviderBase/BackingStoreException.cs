using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;

namespace Nivot.PowerShell
{
    [Serializable, ComVisible(true)]
    public class BackingStoreException : RuntimeException
    {
        public BackingStoreException(string message) : base(message)
        {            
        }

        public BackingStoreException(string message, Exception innerException) : base(message, innerException)
        {            
        }
    }
}
