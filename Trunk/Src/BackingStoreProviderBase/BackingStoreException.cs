#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Nivot.PowerShell
{
    [Serializable, ComVisible(false)]
    public class BackingStoreException : RuntimeException
    {
        public BackingStoreException() { }

        public BackingStoreException(string message) : base(message) { }

        public BackingStoreException(string message, Exception innerException) : base(message, innerException) { }

        protected BackingStoreException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}