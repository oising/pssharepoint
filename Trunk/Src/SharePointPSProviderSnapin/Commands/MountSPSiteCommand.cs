using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.Commands
{
	[Cmdlet(VerbsCommon.Get, "SPVirtualServer")]
	class MountSPSiteCommand : PSCmdlet
	{
		private const string ObjectParameterSet = "Object";
		private const string UrlParameterSet = "Url";

		private SPSite[] m_sites = null;
		private Uri[] m_urls = null;

		[Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = ObjectParameterSet)]
		public SPSite[] Site
		{
			get
			{
				return m_sites;
			}
			set
			{
				m_sites = value;	
			}
		}

		[Parameter(Mandatory = false, Position = 0, ParameterSetName = UrlParameterSet)]
		public Uri[] Url
		{
			get
			{
				return m_urls;
			}
			set
			{
				m_urls = value;
			}
		}

		protected override void ProcessRecord()
		{
			if (ParameterSetName == ObjectParameterSet)
			{
				foreach (SPSite site in m_sites)
				{
				}
			}
			else
			{
				foreach (Uri url in m_urls)
				{
				}
			}
		}

		protected override void EndProcessing()
		{
			if (m_sites != null)
			{
				foreach (SPSite site in m_sites)
				{
					site.Dispose();
				}
				m_sites = null;
			}
		}

		private void AddSharePointDrive(string name, Uri siteUrl)
		{
			
		}
	}
}
