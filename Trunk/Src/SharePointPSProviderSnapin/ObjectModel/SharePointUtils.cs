using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	internal static class SharePointUtils
	{
		internal static SPGlobalAdmin s_admin;
		
		static SharePointUtils()
		{
			s_admin = new SPGlobalAdmin();
		}

		internal static IEnumerable<SPVirtualServer> GetSPVirtualServers(SPVirtualServerState state)
		{
			foreach (SPVirtualServer virtualServer in s_admin.VirtualServers)
			{
				if (virtualServer.State == state)
				{
					yield return virtualServer;
				}
			}
		}

		internal static Version GetSharePointVersion()
		{
			return s_admin.Version;
		}
	}
}
