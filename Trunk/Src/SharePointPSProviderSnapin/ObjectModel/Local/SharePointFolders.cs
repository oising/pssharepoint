using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;

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

		public override StoreItemFlags ItemFlags
		{
			get { return StoreItemFlags.TabComplete; }
		}
	}
}
