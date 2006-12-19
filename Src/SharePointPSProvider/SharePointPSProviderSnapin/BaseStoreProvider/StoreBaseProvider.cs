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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using System.Text.RegularExpressions;

namespace Nivot.PowerShell {

	/// <summary>
	/// 
	/// </summary>
	public abstract class StoreBaseProvider : NavigationCmdletProvider {

		public const char PathSeparator = '\\'; // DOS is dead; long live DOS.

		/// <summary>
		/// Provides a handle to the runtime object model of the backing store
		/// </summary>
		public abstract IStoreObjectModel StoreObjectModel {
			get;
		}

		protected StoreBaseProvider() {
		}

		#region ItemCmdletProvider Overrides
		protected override bool IsValidPath(string path) {
			return StoreObjectModel.IsValidPath(NormalizePath(path));
		}

		protected override void GetItem(string path) {

			path = NormalizePath(path); // TODO: remove

			try {

				IStoreItem item = StoreObjectModel.GetItem(path);
				Debug.Assert(item != null); // FIXME: redundant? itemexists called first?
				WriteItemObject(item.NativeObject, path, item.IsContainer);

			} catch (Exception ex) {

				ThrowTerminatingError(
					new ErrorRecord(ex, String.Format("GetItem('{0}')", path),
					ErrorCategory.NotSpecified, null));
			}
		}

		protected override bool ItemExists(string path) {

			try {

				path = NormalizePath(path); // TODO: remove
				return StoreObjectModel.ItemExists(path);

			} catch (Exception ex) {

				ThrowTerminatingError(
					new ErrorRecord(ex, String.Format("ItemExists('{0}')", path),
					ErrorCategory.NotSpecified, null));
			}
			return false;
		}
		#endregion

		#region ContainerCmdletProvider Overrides
		protected override void GetChildItems(string path, bool recurse) {

			try {
				path = NormalizePath(path); // TODO: remove
				foreach (IStoreItem item in StoreObjectModel.GetChildItems(path)) {

					// be nice to sigbreak
					if (base.Stopping) {
						return;
					}

					// should we send this item to pipeline?
					if ((item.ItemFlags & StoreItemFlags.PipeItem) == StoreItemFlags.PipeItem) {

						string itemPath = MakePath(path, item.ChildName);
						WriteItemObject(item.NativeObject, itemPath, item.IsContainer);

						if (recurse) {
							if (item.IsContainer) {
								GetChildItems(itemPath, recurse);
							}
						}
					}
				}
			} catch (Exception ex) {

				ThrowTerminatingError(
					new ErrorRecord(ex, String.Format("GetChildItems('{0}')", path),
					ErrorCategory.NotSpecified, null));
			}
		}

		
		// FIXME: ignoring returnAllContainers
		protected override void GetChildNames(string path, ReturnContainers returnContainers) {
	
			try {		
				// enumerate children for current path
				path = NormalizePath(path); // TODO: remove
				foreach (IStoreItem item in StoreObjectModel.GetChildItems(path)) {

					// be nice to sigbreak
					if (base.Stopping) {
						return;
					}

					// should we tab complete this item?
					if ((item.ItemFlags & StoreItemFlags.TabComplete) == StoreItemFlags.TabComplete) {
						WriteItemObject(item.ChildName, MakePath(path, item.ChildName), item.IsContainer);
					}
				}
			} catch (Exception ex) {

				ThrowTerminatingError(
					new ErrorRecord(ex, String.Format("GetChildNames('{0}')", path),
					ErrorCategory.NotSpecified, null));
			}
		}
		
		protected override bool HasChildItems(string path) {
			path = NormalizePath(path);  // TODO: remove
			return StoreObjectModel.HasChildItems(path);
		}

		protected override void NewItem(string path, string type, object newItem) {
			ThrowTerminatingError(
			  new ErrorRecord(
				new NotImplementedException(path),
				"StoreBaseProvider.NewItem", ErrorCategory.NotImplemented, null)
			  );
		}

		protected override void MoveItem(string path, string destination) {
			// FIXME: need to verify copy before remove -- right now we rely on "throwterminatingerror" on copy failure
			if (ShouldProcess(destination, "Move")) {
				CopyItem(path, destination, false);
				RemoveItem(path, false);
			}
		}

		protected override void CopyItem(string path, string copyPath, bool recurse) {

			path = NormalizePath(path);  // TODO: remove
			copyPath = NormalizePath(copyPath);  // TODO: remove
			 
			IStoreItem source = StoreObjectModel.GetItem(path);
			IStoreItem destination = StoreObjectModel.GetItem(copyPath);

			// FIXME: is this redundant?
			Debug.Assert((source != null) && (destination != null), "source and/or destination invalid!");

			string sourceType = source.GetType().Name;
			string destinationType = destination.GetType().Name;
			WriteVerbose(String.Format("Copying from {0} to {1}", sourceType, destinationType));

			// TODO: implement recursive copying
			if (recurse) {
				WriteWarning("parameter -recurse is currently not implemented for copy operation.");
			}

			if (ShouldProcess(copyPath, "Copy")) {
				try {
					// try to copy
					bool success = destination.AddItem(source);

					if (!success) {

						// non-terminating error, continue with next record
						WriteError(new ErrorRecord(new NotImplementedException(
							String.Format("Copy operation from type {0} to type {1} is undefined.",
							sourceType, destinationType)), "StoreBaseProvider.CopyItem",
							ErrorCategory.NotImplemented, null));

					} else {

						// success
						WriteVerbose("Copy complete.");
					}

				} catch (ApplicationFailedException ex) {
					// native application failure
					WriteVerbose("Exception: " + ex.ToString());
					ThrowTerminatingError(new ErrorRecord(ex, "StoreError", ErrorCategory.DeviceError, null));
				}
			}
		}

		// FIXME: base implementation only handles drive-qualified path it appears
		protected override string GetParentPath(string path, string root) {
			return base.GetParentPath(path, root);
		}

		// FIXME: recurse is ignored, not sure how it applies?
		protected override void RemoveItem(string path, bool recurse) {
			
			string parentPath = GetParentPath(path, null); // FIXME: assumes PSDriveInfo != null

			IStoreItem parentItem = StoreObjectModel.GetItem(NormalizePath(parentPath));  // TODO: remove
			IStoreItem childItem = StoreObjectModel.GetItem(NormalizePath(path));  // TODO: remove
			Debug.Assert((parentItem != null) && (childItem != null)); // FIXME: redundant/itemexists?
			string parentType = parentItem.GetType().Name;
			string childType = childItem.GetType().Name;

			if (ShouldProcess(path, "Remove")) {
				bool success = parentItem.RemoveItem(childItem);

				if (!success) {

					// FIXME: should be WriteVerbose maybe?
					WriteWarning(String.Format("Failed: {0} does not have a Remover for type {1}",
						parentItem.GetType(), childItem.GetType()));

					// non-terminating error, continue with next record
					WriteError(new ErrorRecord(new NotImplementedException(path),
						"StoreBaseProvider.RemoveItem", ErrorCategory.NotImplemented, null)
					  );
				} else {
					// success
					WriteVerbose("Remove complete.");
				}

			}
		}
		#endregion

		#region NavigationCmdletProvider Overrides

		protected override bool IsItemContainer(string path) {

			IStoreItem item = StoreObjectModel.GetItem(NormalizePath(path));  // TODO: remove
			Debug.Assert(item != null); // FIXME: redundant?

			return item.IsContainer;
		}
		#endregion

		#region Helper Methods

		[Conditional("DEBUG")]
		public static void Dump(string format, params object[] parameters) {
			Debug.WriteLine(string.Format(format, parameters), "StoreBaseProvider");
		}

		/// <summary>
		/// Fix up whatever sort of path string Msh has thrown us
		/// <remarks>FIXME: assumes we're drive-qualified</remarks>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string NormalizePath(string path) {
			Dump("NormalizePath '{0}'", path);

			if (!String.IsNullOrEmpty(path)) {
				// flip slashes; remove a trailing slash, if any.
				string driveRoot = this.PSDriveInfo.Root.Replace('/', '\\').TrimEnd('\\');

				// is drive qualified?
				if (path.StartsWith(driveRoot)) {
					path = path.Replace(driveRoot, ""); // strip it
				}
			}

			// ensure drive is rooted
			if (path == String.Empty) {
				path = PathSeparator.ToString();
			}

			Dump("Normalized to '{0}'", path);

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

