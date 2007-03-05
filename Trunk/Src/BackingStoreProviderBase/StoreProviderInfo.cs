using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell
{
	public class StoreProviderInfo : ProviderInfo
	{
		public StoreProviderInfo(ProviderInfo providerInfo) : base(providerInfo)
		{
			
		}

		public virtual bool UseCaseSensitivePaths
		{
			get
			{
				return false;
			}
		}
	}
}
