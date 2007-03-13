using System;
using System.Collections.Generic;
using System.Text;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	internal class SharePointConnectionInfo
	{
		private Uri m_siteCollectionUrl;
		private Version m_sharePointVersion;
		private bool m_isRemote;

		public Uri SiteCollectionUrl
		{
			get
			{
				return m_siteCollectionUrl;
			}
		}

		public Version SharePointVersion
		{
			get
			{
				return m_sharePointVersion;
			}
		}

		public bool IsRemote
		{
			get
			{
				return m_isRemote;
			}
		}

		internal SharePointConnectionInfo(Uri virtualServer, string root)
		{
			m_siteCollectionUrl = new Uri(virtualServer, root);
			m_sharePointVersion = new Version("2.0.0.0"); // FIXME: this means nothing			
			m_isRemote = false;
		}
	}
}