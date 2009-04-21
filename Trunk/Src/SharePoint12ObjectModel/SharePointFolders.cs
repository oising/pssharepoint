using System;
using System.Collections.Generic;
using System.Text;

using HubKey.Web.Services.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	class SharePointFolders : StoreItem<SPFolderCollection>
	{
		public SharePointFolders(SPFolderCollection folders) : base(folders)
		{
			
		}

		public override string ChildName
		{
			get { return "!Folders"; }
		}

		public override bool IsContainer
		{
			get { return true; }
		}

		public override StoreItemOptions ItemOptions
		{
			get { return StoreItemOptions.ShouldTabComplete; }
		}
	}
}
