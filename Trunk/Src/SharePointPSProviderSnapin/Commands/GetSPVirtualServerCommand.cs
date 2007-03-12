using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using Microsoft.SharePoint.Administration;
using Nivot.PowerShell.SharePoint.ObjectModel;

namespace Nivot.PowerShell.SharePoint.Commands
{
	[Cmdlet(VerbsCommon.Get, "SPVirtualServer")]
	public class GetSPVirtualServerCommand : Cmdlet
	{
		private SPVirtualServerState m_state = SPVirtualServerState.Ready;

		[Parameter(Position = 0, Mandatory = false)]
		public SPVirtualServerState State
		{
			get
			{
				return m_state;
			}
			set
			{
				m_state = value;
			}
		}

		protected override void BeginProcessing()
		{
			foreach (SPVirtualServer virtualServer in SharePointUtils.GetSPVirtualServers(m_state))
			{
				WriteObject(virtualServer);
			}
		}
	}
}
