using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	internal class SharePointDocumentLibrary : StoreItem<SPDocumentLibrary>
	{
		public SharePointDocumentLibrary(SPDocumentLibrary docLib) : base(docLib)
		{			
		}

		public override IEnumerator<IStoreItem> GetEnumerator()
		{
			foreach (SPListItem listItem in NativeObject.Items)
			{
				// TODO: use dynamic get-item parameter to determine whether to return SPFile or SPListItem
				yield return new SharePointFile(listItem.File);
			}
		}

		public override bool IsContainer
		{
			get { return true; }
		}

		public override string ChildName
		{
			get { return NativeObject.Title; }
		}

		public override StoreItemFlags ItemFlags
		{
			get { return StoreItemFlags.TabComplete | StoreItemFlags.PipeItem; }
		}
	}
}
