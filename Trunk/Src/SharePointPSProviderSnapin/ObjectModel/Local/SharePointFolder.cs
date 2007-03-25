using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	class SharePointFolder : StoreItem<SPFolder> 
	{
		public SharePointFolder(SPFolder folder) : base(folder)
		{
			
		}

		public override IEnumerator<IStoreItem> GetEnumerator()
		{
			// return subfolders
			foreach (SPFolder folder in NativeObject.SubFolders)
			{
				yield return new SharePointFolder(folder);
			}

			// then files
			foreach (SPFile file in NativeObject.Files)
			{
				yield return new SharePointFile(file);
			}
		}

		public override string ChildName
		{
			get { return NativeObject.Name; }
		}

		public override bool IsContainer
		{
			get { return true; }
		}

		public override StoreItemOptions ItemOptions
		{
			get { return StoreItemOptions.ShouldTabComplete | StoreItemOptions.ShouldPipeItem; }
		}
	}
}
