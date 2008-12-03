using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;

using Nivot.PowerShell;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	class SharePointFiles : StoreItem<SPFileCollection>
	{
		public SharePointFiles(SPFileCollection files) : base(files)
		{
		    //RegisterAdder<SPFile>(AddFile);
            //RegisterRemover<SPFile>(RemoveFile);
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

		public override StoreItemOptions ItemOptions
		{
			get { return StoreItemOptions.ShouldTabComplete; }
		}
	}
}
