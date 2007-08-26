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
using System.Text;

namespace Nivot.PowerShell
{
	public class StoreProviderInfo : ProviderInfo
	{
		public const string Slash = "/";
		public const string Backslash = "\\";

		public StoreProviderInfo(ProviderInfo providerInfo) : base(providerInfo)
		{			
		}

		public virtual StringComparison PathComparison
		{
			get
			{
			    return StringComparison.OrdinalIgnoreCase;
			}
		}

	    public virtual bool EnableItemCaching
	    {
	        get
	        {
                return false;
	        }
	    }

        [Obsolete("Path separator should always be '\\'")]
		public virtual char PathSeparator
		{
			get
			{
				return Char.Parse(Backslash);
			}
		}
	}
}
