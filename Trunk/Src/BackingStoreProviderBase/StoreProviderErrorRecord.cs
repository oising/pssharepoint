using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell
{
    internal static class StoreProviderErrorRecord
    {
        internal static ErrorRecord OperationFailed(string message, Exception inner)
        {
            ProviderInvocationException ex = new ProviderInvocationException(message, inner);
            ErrorRecord record = new ErrorRecord(ex, "OperationFailed", ErrorCategory.NotSpecified, null);
            
            return record;
        }

		internal static ErrorRecord NotImplemented(string message)
		{
			NotImplementedException exception = new NotImplementedException(message);			
			ErrorRecord record = new ErrorRecord(exception, "NotImplemented", ErrorCategory.NotImplemented, null);

			return record;
		}
    }
}
