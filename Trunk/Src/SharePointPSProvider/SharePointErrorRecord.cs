using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell.SharePoint
{
	public static class SharePointErrorRecord
	{
		public static ErrorRecord ArgumentNullOrEmpty(string argument)
		{
			Exception ex = new ArgumentNullException(argument);
			
			return new ErrorRecord(ex, "ArgumentNullOrEmpty", ErrorCategory.InvalidArgument, null);
		}

		public static ErrorRecord InvalidOperationError(string errorId, string message)
		{
			Exception ex = new InvalidOperationException(message);
			
			return new ErrorRecord(ex, errorId, ErrorCategory.InvalidOperation, null);
		}

		public static ErrorRecord NotImplementedError(string message)
		{
			Exception ex = new NotImplementedException(message);
			
			return new ErrorRecord(ex, "NotImplemented", ErrorCategory.NotImplemented, null);
		}

		public static ErrorRecord ArgumentError(string format, params object[] parameters)
		{
			string message = String.Format(format, parameters);
			Exception ex = new ArgumentException(message);			

			return new ErrorRecord(ex, "InvalidArgument", ErrorCategory.InvalidArgument, null);
		}
	}
}
