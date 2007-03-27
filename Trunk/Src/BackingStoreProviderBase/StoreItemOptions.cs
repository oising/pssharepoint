using System;
using System.Collections.Generic;
using System.Text;

namespace Nivot.PowerShell
{
	/// <summary>
	/// Behavioural options for backing-store items.
	/// </summary>
	[Flags]
	public enum StoreItemOptions
	{
		None = 0,
		/// <summary>
		/// Should appear in Tab Completion list
		/// </summary>
		ShouldTabComplete = 1,
		/// <summary>
		/// Should be sent to object pipeline
		/// </summary>
		ShouldPipeItem = 2,
        /// <summary>
        /// Should be cached
        /// </summary>
        ShouldCache = 4
	}
}
