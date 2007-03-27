using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Management.Automation;

using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	internal class SharePointDocumentLibrary : StoreItem<SPDocumentLibrary>
	{
	    private RuntimeDefinedParameterDictionary m_params;

		public SharePointDocumentLibrary(SPDocumentLibrary docLib) : base(docLib)
		{
		    CreateDynamicParameters();

		    RegisterRemover<SPFile>(RemoveFile);
		    RegisterRemover<SPListItem>(RemoveListItem);
		}

	    public override IEnumerator<IStoreItem> GetEnumerator()
		{
		    bool returnListItems = ParamListItemIsSet;
			
            foreach (SPListItem listItem in NativeObject.Items)
			{
                if (returnListItems)
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

        public override RuntimeDefinedParameterDictionary GetChildItemsDynamicParameters
        {
            get
            {
                Provider.WriteDebug("SharePointDocumentLibrary: supplying GetChildItemsDynamicParameters");

                return m_params;
            }
        }

        public override RuntimeDefinedParameterDictionary GetItemDynamicParameters
        {
            get
            {
                Provider.WriteDebug("SharePointDocumentLibrary: supplying GetItemDynamicParameters");

                return m_params;
            }
        }
     
	    private static bool ParamListItemIsSet
	    {
	        get
	        {
	            Provider.WriteDebug("SharePointDocumentLibrary: ParamListItemIsSet?");

	            return Provider.RuntimeDynamicParameters["ListItem"].IsSet;
	        }
	    }

        private void CreateDynamicParameters()
        {
            // allow override of default return object from SPFile to SPListItem
            DynamicParameterBuilder builder = new DynamicParameterBuilder();
            builder.AddSwitchParam("ListItem");
            m_params = builder.GetDictionary();
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
