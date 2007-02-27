using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	class SharePointFile : StoreItem<SPFile>
	{
		public SharePointFile(SPFile file) : base(file)
		{
			
		}

		public override string ChildName
		{
			get { return NativeObject.Name; }
		}

		public override bool IsContainer
		{
			get { return false; }
		}

		public override StoreItemFlags ItemFlags
		{
			get { return StoreItemFlags.TabComplete | StoreItemFlags.PipeItem; }
		}
	}
}
