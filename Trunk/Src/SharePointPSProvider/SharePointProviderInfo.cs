using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell.SharePoint
{
	internal class SharePointProviderInfo : StoreProviderInfo
	{
		internal SharePointProviderInfo(ProviderInfo providerInfo) : base(providerInfo)
		{			
		}

        public override bool EnableItemCaching
        {
            get
            {
                return true;
            }
        }
	}
}
