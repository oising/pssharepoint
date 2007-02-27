using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	class SharePointFiles : StoreItem<SPFileCollection>
	{
		public SharePointFiles(SPFileCollection files) : base(files)
		{
			
		}

		public override IEnumerator<IStoreItem> GetEnumerator()
		{			
			foreach (SPFile file in NativeObject)
			{
				yield return new SharePointFile(file);
			}
		}

		public override string ChildName
		{
			get { return "!Files"; }
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
