#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using System.Text.RegularExpressions;

namespace Nivot.PowerShell
{

	/// <summary>
	/// 
	/// </summary>
	public abstract partial class StoreProviderBase
	{
		// get-childitem
		// rename-item
		// new-item
		// remove-item
		// set-location
		// push-location
		// pop-location
		// get-location -stack

		#region ContainerCmdletProvider Overrides

		protected override void GetChildItems(string path, bool recurse)
		{
		    WriteDebug("GetChildItems: " + path);
            
            string fullPath = path; // FIXED: needed for PSPath            
            path = NormalizePath(path); 

			using (EnterContext())
			{
				try
				{
				    Collection<IStoreItem> childItems = StoreObjectModel.GetChildItems(path);
					foreach (IStoreItem item in childItems)
					{
						// should we send this item to pipeline? -Force will always send item to pipeline
						if (((item.ItemOptions & StoreItemOptions.ShouldPipeItem) == StoreItemOptions.ShouldPipeItem) || this.Force)
						{
							string itemPath = MakePath(fullPath, item.ChildName); // FIXED: was 'path'
                            object output = item.GetOutputObject();

                            WriteItemObject(output, itemPath, item.IsContainer);

							if (recurse)
							{
								if (item.IsContainer)
								{
									GetChildItems(itemPath, recurse);
								}
							}
						}
                        else
						{
						    item.Dispose();
						}
                        
                        // be nice to sigbreak
                        if (base.Stopping)
                        {
                            return;
                        }
					}
				}
				catch (Exception ex)
				{
					ThrowTerminatingError(
						new ErrorRecord(ex, String.Format("GetChildItems('{0}')", path),
						                ErrorCategory.NotSpecified, null));
				}
			}
		}

        protected override object GetChildItemsDynamicParameters(string path, bool recurse)
        {
            WriteDebug("GetChildItemsDynamicParameters: " + path);
            path = NormalizePath(path);
			
			return GetDynamicParametersForMethod(StoreProviderMethods.GetChildItems, path);
        }

		// FIXME: ignoring returnAllContainers
		protected override void GetChildNames(string path, ReturnContainers returnContainers)
		{
		    WriteDebug("GetChildNames: " + path);
            string fullPath = path; // FIXED: needed for PSPath
            path = NormalizePath(path); 

			using (EnterContext())
			{
				try
				{
                    // enumerate children for current path
					foreach (IStoreItem item in StoreObjectModel.GetChildItems(path))
					{
						// be nice to sigbreak
						if (base.Stopping)
						{
							return;
						}

                        using (item)
                        {
                            // should we tab complete / output name for this item?
                            if ((item.ItemOptions & StoreItemOptions.ShouldTabComplete) ==
                                StoreItemOptions.ShouldTabComplete)
                            {
                                string itemPath = MakePath(fullPath, item.ChildName); // FIXED: was 'path'
                                WriteItemObject(item.ChildName, itemPath, item.IsContainer);
                            }
                        }
					}
				}
				catch (Exception ex)
				{
					ThrowTerminatingError(
						new ErrorRecord(ex, String.Format("GetChildNames('{0}')", path),
						                ErrorCategory.NotSpecified, null));
				}
			}
		}

		protected override object GetChildNamesDynamicParameters(string path)
		{
			WriteDebug("GetChildNamesDynamicParameters: " + path);
			path = NormalizePath(path);

			return GetDynamicParametersForMethod(StoreProviderMethods.GetChildNames, path);
		}

		protected override bool HasChildItems(string path)
		{
		    WriteDebug("HasChildItems: " + path);
            path = NormalizePath(path);

			using (EnterContext())
			{
				return StoreObjectModel.HasChildItems(path);
			}
		}

		protected override void NewItem(string path, string type, object newItem)
		{
			ThrowTerminatingError(
				StoreProviderErrorRecord.NotImplemented("New-Item"));			
		}

		protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
		{
			WriteDebug(String.Format("NewItemDynamicParameters: new {0} in {1} ; value ({2})", path, itemTypeName, newItemValue));
			path = NormalizePath(path);

			return GetDynamicParametersForMethod(StoreProviderMethods.NewItem, path);
		}

		protected override void CopyItem(string path, string copyPath, bool recurse)
		{
			WriteDebug(String.Format("CopyItem: {0} -> {1} (recurse: {2})", path, copyPath, recurse));

			using (EnterContext())
			{
				if (recurse)
				{
					WriteWarning("parameter -recurse is not implemented for copy operations.");
					return;
				}

				path = NormalizePath(path);
				copyPath = NormalizePath(copyPath);

				using (IStoreItem source = StoreObjectModel.GetItem(path))
				{
					using (IStoreItem destination = StoreObjectModel.GetItem(copyPath))
					{
						// FIXME: is this redundant?
						Debug.Assert((source != null) && (destination != null), "source and/or destination invalid!");

						string sourceTypeName = source.GetType().Name;
						string destinationTypeName = destination.GetType().Name;
						WriteVerbose(String.Format("Copying from {0} to {1}", sourceTypeName, destinationTypeName));

						if (ShouldProcess(copyPath, "Copy"))
						{							
							try
							{
								// try to copy
								bool success = destination.AddChildItem(source);

								if (!success)
								{
									// non-terminating error, continue with next record
									WriteError(
										StoreProviderErrorRecord.NotImplemented(
											String.Format("Copy operation from type {0} to type {1} is undefined.",
											              sourceTypeName, destinationTypeName)));
								}
								else
								{
									// success
									WriteVerbose("Copy successful.");
								}
							}
							catch (BackingStoreException ex)
							{
								// native application failure
								WriteVerbose("Exception: " + ex.ToString());
								ThrowTerminatingError(new ErrorRecord(ex, "StoreError", ErrorCategory.NotSpecified, null));
							}
						}
					}
				}
			}
		}

		// FIXME: recurse is not handled, not sure how it should work?
		protected override void RemoveItem(string path, bool recurse)
		{
		    WriteDebug(String.Format("Remove: {0} ; Recurse: {1}", path, recurse));

			using (EnterContext())
			{
                if (recurse)
                {
                    WriteWarning("parameter -recurse is not implemented for remove operations.");
                    return;
                }

				// FIXME: assumes PSDriveInfo != null  (???)
				string parentPath = GetParentPath(path, null);

				using (IStoreItem parentItem = StoreObjectModel.GetItem(NormalizePath(parentPath)))
				{
					using (IStoreItem childItem = StoreObjectModel.GetItem(NormalizePath(path)))
					{
						Debug.Assert((parentItem != null) && (childItem != null)); // FIXME: redundant/itemexists?

						string parentTypeName = parentItem.GetType().Name;
						string childTypeName = childItem.GetType().Name;

						if (ShouldProcess(path, "Remove"))
						{
							bool success = parentItem.RemoveChildItem(childItem);

							if (!success)
							{
								WriteVerbose(String.Format("Failed: {0} does not have a Remover for type {1}",
									parentTypeName, childTypeName));

								// non-terminating error, continue with next record
								WriteError(StoreProviderErrorRecord.NotImplemented("Remove-Item"));
							}
							else
							{
								// success
								WriteVerbose("Remove complete.");
							}
						}
					}
				}
			}			
		}

		#endregion
	}
}