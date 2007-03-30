#region BSD License Header

/*
 * Copyright (c) 2006, Oisin Grehan @ Nivot Inc (www.nivot.org)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
 * Neither the name of Nivot Incorporated nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

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
            path = NormalizePath(path); 

			using (EnterContext())
			{
				try
				{
					foreach (IStoreItem item in StoreObjectModel.GetChildItems(path))
					{
						// be nice to sigbreak
						if (base.Stopping)
						{
							return;
						}

						// should we send this item to pipeline? -Force will always send item to pipeline
						if (((item.ItemOptions & StoreItemOptions.ShouldPipeItem) == StoreItemOptions.ShouldPipeItem) || this.Force)
						{
							string itemPath = MakePath(path, item.ChildName);						    
                            PSObject output = item.GetPSObject();

                            WriteItemObject(output, itemPath, item.IsContainer);

							if (recurse)
							{
								if (item.IsContainer)
								{
									GetChildItems(itemPath, recurse);
								}
							}
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
			
			return GetDynamicParametersForMethod(StoreProviderMethods.GetChildItems, path, recurse);
        }

		// FIXME: ignoring returnAllContainers
		protected override void GetChildNames(string path, ReturnContainers returnContainers)
		{
		    WriteDebug("GetChildNames: " + path);
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

						// should we tab complete / output name for this item?
						if ((item.ItemOptions & StoreItemOptions.ShouldTabComplete) == StoreItemOptions.ShouldTabComplete)
						{
							WriteItemObject(item.ChildName, MakePath(path, item.ChildName), item.IsContainer);
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

			return GetDynamicParametersForMethod(StoreProviderMethods.NewItem, path, itemTypeName, newItemValue);
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