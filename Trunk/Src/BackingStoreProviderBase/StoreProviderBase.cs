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
	public abstract class StoreProviderBase : NavigationCmdletProvider,
		IPropertyCmdletProvider, IContentCmdletProvider, IDynamicPropertyCmdletProvider
	{
		public new StoreProviderInfo ProviderInfo
		{
			get
			{				
				return (StoreProviderInfo) base.ProviderInfo;
			}
		}

		protected new RuntimeDefinedParameterDictionary DynamicParameters
		{
		    get
		    {
				return base.DynamicParameters as RuntimeDefinedParameterDictionary;
		    }
		}

		//protected new TDriveInfo PSDriveInfo
		//{
		//    get
		//    {
		//        return (TDriveInfo)base.PSDriveInfo;
		//    }
		//}

		/// <summary>
		/// Provides a handle to the runtime object model of the backing store
		/// </summary>
		public abstract IStoreObjectModel StoreObjectModel { get; }

		protected StoreProviderContext<StoreProviderBase>.Cookie EnterContext()
		{
			return StoreProviderContext<StoreProviderBase>.Enter(this);
		}

        // ...

		#region CmdletProvider Overrides

		/// <summary>
		/// Gives the provider the opportunity to initialize itself.
		/// </summary>
		/// 
		/// <param name="providerInfo">
		/// The information about the provider that is being started.
		/// </param>
		/// 
		/// <returns>
		/// Either the providerInfo that was passed or a derived class
		/// of ProviderInfo that was initialized with the provider information
		/// that was passed.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns the ProviderInfo instance that
		/// was passed.To have session state maintain persisted data on behalf
		/// of the provider, the provider should derive from 
		/// <see cref="System.Management.Automation.ProviderInfo"/>
		/// and add any properties or methods for the data it wishes to persist.
		/// When Start gets called the provider should construct an instance of 
		/// its derived ProviderInfo using the providerInfo that is passed in 
		/// and return that new instance.
		/// </remarks>
		protected override ProviderInfo Start(ProviderInfo providerInfo)
		{
			WriteVerbose("Start()");
			return new StoreProviderInfo(providerInfo);
		}

		/// <summary>
		/// Gets an object that defines the additional parameters for the
		/// Start implementation for a provider.
		/// </summary>
		/// 
		/// <returns>
		/// Overrides of this method should return an object that has properties
		/// and fields decorated with parsing attributes similar to a cmdlet 
		/// class or a 
		/// <see cref="System.Management.Automation"/>.
		/// The default implemenation returns null.
		/// </returns>
		protected override object StartDynamicParameters()
		{
			return null;
		}

		/// <summary>
		/// Uninitialize the provider. Called by Session State when the provider
		/// is being removed.
		/// </summary>
		/// 
		/// <remarks>
		/// This is the time to free up any resources that the provider
		/// was using. The default implementation in CmdletProvider does 
		/// nothing.
		/// </remarks>
		protected override void Stop()
		{
			WriteVerbose("Stop()");
		}

		#endregion CmdletProvider Overrides

        // new-psdrive
        // remove-psdrive

		#region DriveCmdletProvider Overrides

		/// <summary>
		/// Gives the provider an opportunity to validate the drive that is
		/// being added.  It also allows the provider to modify parts of the
		/// PSDriveInfo object.  This may be done for performance or
		/// reliability reasons or to provide extra data to all calls using
		/// the Drive.
		/// </summary>
		/// 
		/// <param name="drive">
		/// The proposed new drive.
		/// </param>
		/// 
		/// <returns>
		/// The new drive that is to be added to the Windows PowerShell namespace.  This
		/// can either be the same <paramref name="drive"/> object that
		/// was passed in or a modified version of it. The default 
		/// implementation returns the drive that was passed.
		/// </returns>
		/// 
		/// <remarks>
		/// This method gives the provider an opportunity to associate 
		/// provider specific information with a drive. This is done by 
		/// deriving a new class from 
		/// <see cref="System.Management.Automation.PSDriveInfo"/>
		/// and adding any properties, methods, or fields that are necessary. 
		/// When this method gets called, the override should create an instance
		/// of the derived PSDriveInfo using the passed in PSDriveInfo. The derived
		/// PSDriveInfo should then be returned. Each subsequent call into the provider
		/// that uses this drive will have access to the derived PSDriveInfo via the
		/// PSDriveInfo property provided by the base class. Implementers of this 
		/// method should verify that the root exists and that a connection to 
		/// the data store (if there is one) can be made.  Any failures should 
		/// be sent to the
		/// <see cref="System.Management.Automation.Provider.CmdletProvider.WriteError(ErrorRecord)"/>
		/// method and null should be returned.
		/// </remarks>
		protected override PSDriveInfo NewDrive(PSDriveInfo drive)
		{
			return drive;
		}

		/// <summary>
		/// Allows the provider to attach additional parameters to the 
		/// New-PSDrive cmdlet.
		/// </summary>
		/// 
		/// <returns>
		/// Implementors of this method should return an object that has 
		/// properties and fields decorated with parsing attributes similar 
		/// to a cmdlet class or a 
		/// <see cref="System.Management.Automation"/>.
		/// The default implemenation returns null.
		/// </returns>
		protected override object NewDriveDynamicParameters()
		{
			return null;
		}// NewDriveDynamicParameters

		/// <summary>
		/// Cleans up provider specific data for a drive before it is  
		/// removed. This method gets called before a drive gets removed. 
		/// </summary>
		/// 
		/// <param name="drive">
		/// The Drive object that represents the mounted drive.
		/// </param>
		/// 
		/// <returns>
		/// If the drive can be removed then the drive that was passed in is 
		/// returned. If the drive cannot be removed, null should be returned
		/// and an exception is written to the 
		/// <see cref="System.Management.Automation.Provider.CmdletProvider.WriteError(ErrorRecord)"/>
		/// method. The default implementation returns the drive that was 
		/// passed.
		/// </returns>
		/// 
		/// <remarks>
		/// An implementer has to ensure that this method is overridden to free 
		/// any resources that may be associated with the drive being removed.
		/// </remarks>
		/// 
		protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
		{
			return drive;
		}

		/// <summary>
		/// Allows the provider to map drives after initialization.
		/// </summary>
		/// 
		/// <returns>
		/// A drive collection with the drives that the provider wants to be 
		/// added to the session upon initialization. The default 
		/// implementation returns an empty 
		/// <see cref="System.Management.Automation.PSDriveInfo"/> collection.
		/// </returns>
		/// 
		/// <remarks>
		/// After the Start method is called on a provider, the
		/// InitializeDefaultDrives method is called. This is an opportunity
		/// for the provider to mount drives that are important to it. For
		/// instance, the Active Directory provider might mount a drive for
		/// the defaultNamingContext if the machine is joined to a domain.
		/// 
		/// All providers should mount a root drive to help the user with
		/// discoverability. This root drive might contain a listing of a set
		/// of locations that would be interesting as roots for other mounted
		/// drives. For instance, the Active Directory provider may create a
		/// drive that lists the naming contexts found in the namingContext
		/// attributes on the RootDSE. This will help users discover
		/// interesting mount points for other drives.
		/// </remarks>
		protected override Collection<PSDriveInfo> InitializeDefaultDrives()
		{
			Collection<PSDriveInfo> drives = new Collection<PSDriveInfo>();
			return drives;
		}

		#endregion DriveCmdletProvider Overrides

        // ...

		#region ItemCmdletProvider Overrides

		protected override bool IsValidPath(string path)
		{
			return StoreObjectModel.IsValidPath(NormalizePath(path));
		}

		protected override void GetItem(string path)
		{
			using (EnterContext())
			{
				path = NormalizePath(path); // TODO: remove

				try
				{
					IStoreItem item = StoreObjectModel.GetItem(path);
					Debug.Assert(item != null); // FIXME: redundant? itemexists called first?
					WriteItemObject(item.NativeObject, path, item.IsContainer);
				}
				catch (Exception ex)
				{
					ThrowTerminatingError(
						new ErrorRecord(ex, String.Format("GetItem('{0}')", path),
						                ErrorCategory.NotSpecified, null));
				}
			}
		}

		protected override object GetItemDynamicParameters(string path)
		{
			return base.GetItemDynamicParameters(path);
		}

		protected override bool ItemExists(string path)
		{
			using (EnterContext())
			{
				try
				{
					path = NormalizePath(path); // TODO: remove
					return StoreObjectModel.ItemExists(path);
				}
				catch (Exception ex)
				{
					ThrowTerminatingError(
						new ErrorRecord(ex, String.Format("ItemExists('{0}')", path),
						                ErrorCategory.NotSpecified, null));
				}
				return false;
			}
		}

		#endregion

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
			using (EnterContext())
			{
				try
				{
					path = NormalizePath(path); // TODO: remove
					foreach (IStoreItem item in StoreObjectModel.GetChildItems(path))
					{
						// be nice to sigbreak
						if (base.Stopping)
						{
							return;
						}

						// should we send this item to pipeline?
						if ((item.ItemFlags & StoreItemFlags.PipeItem) == StoreItemFlags.PipeItem)
						{
							string itemPath = MakePath(path, item.ChildName);
							WriteItemObject(item.NativeObject, itemPath, item.IsContainer);

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


		// FIXME: ignoring returnAllContainers
		protected override void GetChildNames(string path, ReturnContainers returnContainers)
		{
			using (EnterContext())
			{
				try
				{
					// enumerate children for current path
					path = NormalizePath(path); // TODO: remove
					foreach (IStoreItem item in StoreObjectModel.GetChildItems(path))
					{
						// be nice to sigbreak
						if (base.Stopping)
						{
							return;
						}

						// should we tab complete this item?
						if ((item.ItemFlags & StoreItemFlags.TabComplete) == StoreItemFlags.TabComplete)
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

		protected override bool HasChildItems(string path)
		{
			using (EnterContext())
			{
				// FIXME: normalize path is incomplete
				path = NormalizePath(path);
				return StoreObjectModel.HasChildItems(path);
			}
		}

		protected override void NewItem(string path, string type, object newItem)
		{
			ThrowTerminatingError(
				new ErrorRecord(
					new NotImplementedException(path),
					"StoreBaseProvider.NewItem", ErrorCategory.NotImplemented, null)
				);
		}

		protected override void MoveItem(string path, string destination)
		{
			using (EnterContext())
			{				
				if (ShouldProcess(destination, "Move"))
				{
					CopyItem(path, destination, false);
					if (ItemExists(destination))
					{
						RemoveItem(path, false);
					}
				}
			}
		}

		protected override void CopyItem(string path, string copyPath, bool recurse)
		{
			using (EnterContext())
			{
				path = NormalizePath(path); // TODO: remove
				copyPath = NormalizePath(copyPath); // TODO: remove

				IStoreItem source = StoreObjectModel.GetItem(path);
				IStoreItem destination = StoreObjectModel.GetItem(copyPath);

				// FIXME: is this redundant?
				Debug.Assert((source != null) && (destination != null), "source and/or destination invalid!");

				string sourceType = source.GetType().Name;
				string destinationType = destination.GetType().Name;
				WriteVerbose(String.Format("Copying from {0} to {1}", sourceType, destinationType));

				// TODO: implement recursive copying
				if (recurse)
				{
					WriteWarning("parameter -recurse is currently not implemented for copy operation.");
					return;
				}

				if (ShouldProcess(copyPath, "Copy"))
				{
					try
					{
						// try to copy
						bool success = destination.AddItem(source);

						if (!success)
						{
							// non-terminating error, continue with next record
							WriteError(new ErrorRecord(new NotImplementedException(
							                           	String.Format("Copy operation from type {0} to type {1} is undefined.",
							                           	              sourceType, destinationType)), "StoreBaseProvider.CopyItem",
							                           ErrorCategory.NotImplemented, null));
						}
						else
						{
							// success
							WriteVerbose("Copy complete.");
						}
					}
					catch (ApplicationFailedException ex)
					{
						// native application failure
						WriteVerbose("Exception: " + ex.ToString());
						ThrowTerminatingError(new ErrorRecord(ex, "StoreError", ErrorCategory.DeviceError, null));
					}
				}
			}
		}

		// FIXME: base implementation only handles drive-qualified path it appears
		protected override string GetParentPath(string path, string root)
		{
			return base.GetParentPath(path, root);
		}

		// FIXME: recurse is ignored, not sure how it applies?
		protected override void RemoveItem(string path, bool recurse)
		{
			using (EnterContext())
			{
				string parentPath = GetParentPath(path, null); // FIXME: assumes PSDriveInfo != null

				IStoreItem parentItem = StoreObjectModel.GetItem(NormalizePath(parentPath)); // TODO: remove
				IStoreItem childItem = StoreObjectModel.GetItem(NormalizePath(path)); // TODO: remove
				Debug.Assert((parentItem != null) && (childItem != null)); // FIXME: redundant/itemexists?
				string parentType = parentItem.GetType().Name;
				string childType = childItem.GetType().Name;

				if (ShouldProcess(path, "Remove"))
				{
					bool success = parentItem.RemoveItem(childItem);

					if (!success)
					{
						// FIXME: should be WriteVerbose maybe?
						WriteWarning(String.Format("Failed: {0} does not have a Remover for type {1}",
						                           parentItem.GetType(), childItem.GetType()));

						// non-terminating error, continue with next record
						WriteError(new ErrorRecord(new NotImplementedException("Remove-Item"),
						                           "StoreBaseProvider.RemoveItem", ErrorCategory.NotImplemented, null));
					}
					else
					{
						// success
						WriteVerbose("Remove complete.");
					}
				}
			}
		}

		#endregion

        // ...

		#region NavigationCmdletProvider Overrides

		protected override bool IsItemContainer(string path)
		{
			using (EnterContext())
			{
				IStoreItem item = StoreObjectModel.GetItem(NormalizePath(path)); // TODO: remove
				Debug.Assert(item != null); // FIXME: redundant?

				return item.IsContainer;
			}
		}

		#endregion

		// get-content
		// set-content
		// clear-content

		#region IContentCmdletProvider Overrides

		/// <summary>
		/// Gets an IContentReader from the provider for the item at the
		/// specified path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the object to be opened for reading content.
		/// </param>
		/// 
		/// <returns>
		/// An IContentReader for the specified content.
		/// </returns>
		/// 
		/// <remarks>
		/// Overrides of this method should return an 
		/// <see cref="System.Management.Automation.Provider.IContentReader"/>
		/// for the item specified by the path.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not return a content 
		/// reader for objects that are generally hidden from the user unless 
		/// the Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/>
		/// method if
		/// the path represents an item that is hidden from the user and Force 
		/// is set to false.
		/// </remarks>
		public IContentReader GetContentReader(string path)
		{
			return null;
		} // GetContentReader

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// get-content cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with
		/// parsing attributes similar to a cmdlet class or a 
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object GetContentReaderDynamicParameters(string path)
		{
			return null;
		} // GetContentReaderDynamicParameters

		/// <summary>
		/// Gets an IContentWriter from the provider for the content at the
		/// specified path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the object to be opened for writing content.
		/// </param>
		/// 
		/// <returns>
		/// An IContentWriter for the item at the specified path.
		/// </returns>
		/// 
		/// <remarks>
		/// Overrides of this method should return an 
		/// <see cref="System.Management.Automation.Provider.IContentWriter"/>
		/// for the item specified by the path.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not return a content 
		/// writer for objects that are generally hidden from 
		/// the user unless the Force property is set to true. An error should 
		/// be sent to the WriteError method if the path represents an item 
		/// that is hidden from the user and Force is set to false.
		/// </remarks>
		public IContentWriter GetContentWriter(string path)
		{
			return null;
		} // GetContentWriter

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// set-content and add-content cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation will returns null.
		/// </remarks>
		public object GetContentWriterDynamicParameters(string path)
		{
			return null;
		} // GetContentWriterDynamicParameters

		/// <summary>
		/// Clears the content from the specified item.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item to clear the content from.
		/// </param>
		///
		/// <remarks>
		/// Overrides of this method should remove any content from the object 
		/// but not remove (delete) the object itself.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not clear or write 
		/// objects that are generally hidden from the user unless the Force 
		/// property is set to true. An error should be sent to the WriteError 
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// This method should call 
		/// <see cref="System.Management.Automation.Providers.CmdletProvider.ShouldProcess"/>
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void ClearContent(string path)
		{
			// Write code here to clear contents
			// after performing necessary validations

			// Example 
			// 
			// if (ShouldProcess(path, "clear"))
			// {
			//      // Clear the contents and then call WriteItemObject
			//      WriteItemObject("", path, false);
			// }
		} //ClearContent

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// clear-content cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with
		/// parsing attributes similar to a cmdlet class or a 
		/// <see cref="System.Management.Automation"/>.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object ClearContentDynamicParameters(string path)
		{
			return null;
		} // ClearContentDynamicParameters

		#endregion IContentCmdletProvider Overrides

		// get-itemproperty
		// set-itemproperty
		// clear-itemproperty

		#region IPropertyCmdletProvider Overrides

		/// <summary>
		/// Gets the properties of the item specified by the path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item to retrieve properties from.
		/// </param>
		/// 
		/// <param name="providerSpecificPickList">
		/// A list of properties that should be retrieved. If this parameter is
		/// null or empty, all properties should be retrieved.
		/// </param>
		/// 
		/// <remarks>
		/// 
		/// Providers override this method to give the user the ability to  
		/// retrieve properties to provider objects using the get-itemproperty 
		/// cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not retrieve properties
		/// from objects that are generally hidden from the user unless the 
		/// Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/>
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// An <see cref="System.Management.Automation.PSObject"/> can be used
		/// as a property bag for the properties that need to be returned if 
		/// the <paramref name="providerSpecificPickList"/> contains multiple 
		/// properties to get.
		///
		/// An instance of <see cref="System.Management.Automation.PSObject"/>
		/// representing the properties that were retrieved should be passed 
		/// to the 
		/// <see cref="CmdletProvider.WritePropertyObject"/> 
		/// method. 
		/// 
		/// It is recommended that you support wildcards in property names 
		/// using <see cref="System.Management.Automation.WildcardPattern"/>.
		/// </remarks>
		public void GetProperty(string path,
								Collection<string> providerSpecificPickList)
		{
			// PSObject psObject = new PSObject();
			// psObject.AddNote( propertyName, propertyValue );
			// WritePropertyObject( propertyValue, path );
		} // GetProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// get-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="providerSpecificPickList">
		/// A list of properties that were specified on the command line.
		/// This parameter may be empty or null if the properties are being
		/// piped into the command.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a 
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object GetPropertyDynamicParameters(string path,
												   Collection<string> providerSpecificPickList)
		{
			return null;
		} // GetPropertyDynamicParameters

		/// <summary>
		/// Sets the specified properties of the item at the specified path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item to set the properties on.
		/// </param>
		/// 
		/// <param name="property">
		/// An PSObject which contains a collection of the names and values
		/// of the properties to be set.
		/// </param>
		/// 
		/// <remarks>
		/// Providers override this method to give the user the ability to set 
		/// the value of provider object properties using the set-itemproperty 
		/// cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not retrieve properties
		/// from objects that are generally hidden from the user unless the 
		/// Force property is set to true. An error should be sent to the 
		/// WriteError 
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// 
		/// An instance of <see cref="System.Management.Automation.PSObject"/>
		/// representing the properties that were set should be passed 
		/// to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WritePropertyObject"/> 
		/// method. 
		/// 
		/// <paramref name="propertyValue"/> is a property bag containing the 
		/// properties that should be set. See 
		/// <see cref="System.Management.Automation.PSObject"/> for more 
		/// information.
		/// 
		/// This method should call 
		/// <see cref="System.Management.Automation.CmdletProvider.ShouldProcess"/>
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void SetProperty(string path, PSObject propertyValue)
		{
			// Write code here to set the specified property
			// after performing the necessary validations
			//
			// WritePropertyObject(propertyValue, path);

			// Example 
			// 
			//      if (ShouldProcess(path, "set property"))
			//      {
			//          // set the property here and then call WritePropertyObject
			//          WritePropertyObject(propertyValue, path);
			//      }
			// }
		} // SetProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the 
		/// set-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="propertyValue">
		/// An PSObject which contains a collection of the name, and value 
		/// of the properties to be set if they were specified on the command
		/// line.  The PSObject could be empty or null if the parameters are
		/// being piped into the command.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a 
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object SetPropertyDynamicParameters(string path,
												   PSObject propertyValue)
		{
			return null;
		} // SetPropertyDynamicParameters

		/// <summary>
		/// Clears a property of the item at the specified path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item on which to clear the property.
		/// </param>
		/// 
		/// <param name="propertyToClear">
		/// The name of the property to clear.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to clear
		/// the value of provider object properties using the clear-itemproperty 
		/// cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not clear properties 
		/// from objects that are generally hidden from the user unless the 
		/// Force property is set to true. An error should be sent to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WriteError"/>
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// An <see cref="System.Management.Automation.PSObject"/> can be used
		/// as a property bag for the properties that need to be returned if 
		/// the <paramref name="propertyToClear"/> contains multiple 
		/// properties to write.
		/// 
		/// An instance of <see cref="System.Management.Automation.PSObject"/>
		/// representing the properties that were cleared should be passed 
		/// to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WritePropertyObject"/> 
		/// method. 
		/// 
		/// This method should call 
		/// <see cref="System.Management.Automation.CmdletProvider.ShouldProcess"/> 
		/// and check its return value before making any changes to the store this provider is
		/// working upon.
		/// </remarks>
		public void ClearProperty(string path,
								  Collection<string> propertyToClear)
		{
			// Write code here to clear properties
			// of the item speicified at the path
			// after performing necessary validations

			// Example
			// 
			// if (propertyToClear.Count == 1)
			// {
			//      if (ShouldProcess(path, "clear properties"))
			//      {
			//          // Clear properties and then call WriteItemObject
			//          WritePropertyObject(propertyToClear[0], path);            
			//      }
			// }
		} // ClearProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// clear-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="propertyToClear">
		/// The name of the property to clear. This parameter may be null or
		/// empty if the properties to clear are being piped into the command.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a 
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object ClearPropertyDynamicParameters(string path,
													 Collection<string> propertyToClear)
		{
			return null;
		} // ClearPropertyDynamicParameters

		#endregion IPropertyCmdletProvider

		// rename-itemproperty
		// remove-itemproperty
		// new-itemproperty
		// copy-itemproperty
		// move-itemproperty

		#region IDynamicPropertyCmdletProvider Overrides

		/// <summary>
		/// Copies a property of the item at the specified path to a new
		/// property on the destination item.
		/// </summary>
		/// 
		/// <param name="sourcePath">
		/// The path to the item from which to copy the property.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The name of the property to copy.
		/// </param>
		/// 
		/// <param name="destinationPath">
		/// The path to the item on which to copy the property to.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The destination property to copy to.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to copy
		/// properties of provider objects using the copy-itemproperty cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not copy properties 
		/// from or to objects that are generally hidden from the user unless 
		/// the Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/>
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false. 
		/// 
		/// This method should call 
		/// <see cref="CmdletProvider.ShouldProcess"/>
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void CopyProperty(string sourcePath, string sourceProperty,
						string destinationPath, string destinationProperty)
		{
			// Write code here to modify property of 
			// an item after performing necessary
			// validations
			//
			// WritePropertyObject(destinationProperty, destinationPath );

			// Example 
			// 
			// if (ShouldProcess(destinationPath, "copy property"))
			// {
			//      // Copy property and then call WritePropertyObject
			//      WritePropertyObject(destinationProperty, destinationPath);
			// }

		} // CopyProperty

		/// <summary>
		/// Allows the provider a attach additional parameters to the
		/// copy-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="sourcePath">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The name of the property to copy.
		/// </param>
		/// 
		/// <param name="destinationPath">
		/// The path to the item on which to copy the property to.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The destination property to copy to.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no
		/// dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object CopyPropertyDynamicParameters(string sourcePath,
							string sourceProperty, string destinationPath,
								string destinationProperty)
		{
			return null;
		} // CopyPropertyDynamicParameters

		/// <summary>
		/// Moves a property on an item specified by the path.
		/// </summary>
		/// 
		/// <param name="sourcePath">
		/// The path to the item from which to move the property.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The name of the property to move.
		/// </param>
		/// 
		/// <param name="destinationPath">
		/// The path to the item on which to move the property to.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The destination property to move to.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to move
		/// properties from one provider object to another using the move-itemproperty cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not move properties on 
		/// or to objects that are generally hidden from the user unless the 
		/// Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/> 
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// This method should call 
		/// <see cref="CmdletProvider.ShouldProcess"/> 
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void MoveProperty(string sourcePath, string sourceProperty,
						string destinationPath, string destinationProperty)
		{
			// Write code to move property of an item
			// here after performing necessary validations
			//
			// WritePropertyObject( destinationProperty, destinationPath );

			// Example
			// 
			// if (ShouldProcess(destinationPath, "move property"))
			// {
			//      // Move the properties and then call WritePropertyObject
			//      WritePropertyObject(destinationProperty, destinationPath);
			// }

		} // MoveProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// move-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="sourcePath">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The name of the property to move.
		/// </param>
		/// 
		/// <param name="destinationPath">
		/// The path to the item on which to move the property to.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The destination property to move to.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a
		/// <see cref="System.Management.Automation"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object MovePropertyDynamicParameters(string sourcePath,
							string sourceProperty, string destinationPath,
								string destinationProperty)
		{
			return null;
		} // MovePropertyDynamicParameters

		/// <summary>
		/// Creates a new property on the specified item
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item on which the new property should be created.
		/// </param>
		/// 
		/// <param name="propertyName">
		/// The name of the property that should be created.
		/// </param>
		/// 
		/// <param name="type">
		/// The type of the property that should be created.
		/// </param>
		/// 
		/// <param name="value">
		/// The new value of the property that should be created.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to add
		/// properties to provider objects using the new-itemproperty cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not create new 
		/// properties on objects that are generally hidden from the user unless
		/// the Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/> 
		/// method if the path represents an item that is hidden from the user
		/// and Force is set to false.
		/// 
		/// This method should call 
		/// <see cref="CmdletProvider.ShouldProcess"/> 
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void NewProperty(string path, string propertyName, string type,
						object value)
		{
			// Write code here to create a new property
			// after performing necessary validations
			//
			// WritePropertyObject( propertyValue, path );

			// setting 
			// 
			// if (ShouldProcess(path, "new property"))
			// {
			//      // Set the new property and then call WritePropertyObject
			//      WritePropertyObject(value, path);
			// }
		} // NewProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// new-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="propertyName">
		/// The name of the property that should be created.
		/// </param>
		/// 
		/// <param name="type">
		/// The type of the property that should be created.
		/// </param>
		/// 
		/// <param name="value">
		/// The new value of the property that should be created.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object NewPropertyDynamicParameters(string path,
							string propertyName, string type, object value)
		{
			return null;
		} // NewPropertyDynamicParameters

		/// <summary>
		/// Removes a property on the item specified by the path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item from which the property should be removed.
		/// </param>
		/// 
		/// <param name="propertyName">
		/// The name of the property to be removed.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to
		/// remove properties from provider objects using the remove-itemproperty
		/// cmdlet.
		/// 
		/// Providers that declare
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not remove properties 
		/// on objects that are generally hidden from the user unless the Force 
		/// property is set to true. An error should be sent to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WriteError"/>
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// This method should call 
		/// <see cref="System.Management.Automatin.CmdletProvider.ShouldProcess"/>
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void RemoveProperty(string path, string propertyName)
		{
			// Write code here to remove property of 
			// of an item after performing necessary
			// operations

			// Example 
			// if (ShouldProcess(propertyName, "delete"))
			//{
			//    // delete the property
			//}
		} // RemoveProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// remove-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="propertyName">
		/// The name of the property that should be removed.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a 
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object RemovePropertyDynamicParameters(string path,
			string propertyName)
		{
			return null;
		} // RemovePropertyDynamicParameters

		/// <summary>
		/// Renames a property of the item at the specified path
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item on which to rename the property.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The property to rename.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The new name of the property.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to 
		/// rename properties of provider objects using the rename-itemproperty
		/// cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not rename properties 
		/// on objects that are generally hidden from the user unless the Force
		/// property is set to true. An error should be sent to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WriteError"/> 
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// This method should call
		/// <see cref="System.Management.Automation.CmdletProvider.ShouldProcess"/> 
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void RenameProperty(string path, string sourceProperty,
			string destinationProperty)
		{
			// Write code here to rename a property after
			// performing necessary validaitions
			//
			// WritePropertyObject(destinationProperty, path);

			// Example 
			// if (ShouldProcess(destinationProperty, "delete"))
			//{
			//    // Delete property here
			//}
		} // RenameProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// rename-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The property to rename.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The new name of the property.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object RenamePropertyDynamicParameters(string path,
							string sourceProperty, string destinationProperty)
		{
			return null;
		} // RenamePropertyDynamicParameters

		#endregion IDynamicPropertyCmdletProvider Overrides

		#region Helper Methods

		/// <summary>
		/// Fix up whatever sort of path string Msh has thrown us
		/// <remarks>FIXME: assumes we're drive-qualified</remarks>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string NormalizePath(string path)
		{
			WriteVerbose("NormalizePath " + path);

			if (!String.IsNullOrEmpty(path))
			{
				// flip slashes; remove a trailing slash, if any.
				string driveRoot = this.PSDriveInfo.Root.Replace('/', '\\').TrimEnd('\\');

				// is drive qualified?
				if (path.StartsWith(driveRoot))
				{
					path = path.Replace(driveRoot, ""); // strip it
				}
			}

			// ensure drive is rooted
			if (path == String.Empty)
			{
				path = ProviderInfo.PathSeparator.ToString();
			}

			WriteVerbose("Normalized to " + path);

			return path;
		}

		/*
				private bool IsDrive(string path) {
					bool isDrive = (path == String.Format(this.PSDriveInfo.Root + ":" + PathSeparator));
					Dump("IsDrive {0} : {1}", path, isDrive);

					return isDrive;
				}

				private string EnsureDriveIsRooted(string path) {
					Dump("EnsureDriveIsRooted {0}", path);
					if (!path.StartsWith(PathSeparator)) {
						return PathSeparator + path;
					}
					Dump("EnsureDriveIsRooted returning {0}", path);
				
					return path;
				}
		*/

		#endregion
	}
}