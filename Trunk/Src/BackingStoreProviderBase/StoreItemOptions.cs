#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

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
