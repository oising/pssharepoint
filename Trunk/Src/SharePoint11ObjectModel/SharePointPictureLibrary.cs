using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Management.Automation;

using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	internal class SharePointPictureLibrary : StoreItem<SPPictureLibrary>
	{
        public SharePointPictureLibrary(SPPictureLibrary picLib)
            : base(picLib)
		{
		    RegisterRemover<SPFile>(RemoveFile);
		    RegisterRemover<SPListItem>(RemoveListItem);
            RegisterSwitchParameter(StoreProviderMethods.GetChildItems, SharePointParams.ListItem);
		}

	    public override IEnumerator<IStoreItem> GetEnumerator()
		{
            bool? returnListItems = IsSwitchParameterSet(SharePointParams.ListItem);
			
            foreach (SPListItem listItem in NativeObject.Items)
			{
                if (returnListItems == true)
                {
                    yield return new SharePointListItem(listItem);
                }
                else
                {
                    Debug.Assert(listItem.File != null, "listItem.File != null");
                    yield return new SharePointFile(listItem.File);
                }
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

		public override StoreItemOptions ItemOptions
		{
			get { return StoreItemOptions.ShouldTabComplete | StoreItemOptions.ShouldPipeItem; }
		}
     
        private void RemoveFile(SPFile file)
        {
            SPListItem listItem = file.Item;
            RemoveListItem(listItem);
        }

        private void RemoveListItem(SPListItem listItem)
        {
            NativeObject.Items.DeleteItemById(listItem.ID);
        }
	}
}
