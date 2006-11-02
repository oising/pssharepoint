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
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

namespace Nivot.PowerShell.SharePoint {

	/// <summary>
	/// Base factory class for getting access to the SharePoint Object Model, either local or remote.
	/// <remarks>TODO: constructor overloads for provider-qualified path, e.g. no PSDriveInfo available</remarks>
	/// </summary>
	public class SharePointObjectModel : IStoreObjectModel {

		private static Regex s_pathRegex = new Regex(@"(\\(?:[^!\\]+\\?)*)(?:(!users|!groups|!roles|!alerts|!lists)\\?([^!\\]+)?)?", RegexOptions.IgnoreCase);
		private Uri m_virtualServer;

		protected SharePointObjectModel(Uri virtualServer) {
			m_virtualServer = virtualServer;
		}

		static SharePointObjectModel() {
			// wire-up missing assembly handler
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
		}

		public static IStoreObjectModel GetSharePointObjectModel(Uri virtualServer) {

			// TODO: detect to use webservice (remote) or local and
			//	instantiate via Activator class
			return new LocalSharePointObjectModel(virtualServer);
		}

		#region IStoreObjectModel Members

		public virtual bool IsValidPath(string path) {
			return s_pathRegex.IsMatch(path);
		}

		public virtual bool HasChildItems(string path) {
			StoreBaseProvider.Dump("SharePointObjectModel::HasChildItems called for path {0}", path);

			IStoreItem item = GetItem(path);
			Debug.Assert(item != null);

			// FIXME: redundant maybe?
			if (item.IsContainer) {

				// FIXME: I don't like the look of this, but nor do I like the alternatives
				foreach (IStoreItem childItem in item) {
					return true; // HACK: if we get here, we've got child items
				}
			}
			return false;
		}

		public virtual bool ItemExists(string path) {
			StoreBaseProvider.Dump("SharePointObjectModel::ItemExists called for path {0}", path);

			bool itemExists = (GetItem(path) != null);
			StoreBaseProvider.Dump("SharePointObjectModel::ItemExists returning {0}", itemExists);

			return itemExists;
		}

		public virtual Collection<IStoreItem> GetChildItems(string path) {

			IStoreItem parentItem = GetItem(path);
			Debug.Assert(parentItem != null);

			Collection<IStoreItem> childItems = new Collection<IStoreItem>();

			foreach (IStoreItem childItem in parentItem) {
				childItems.Add(childItem);
			}

			return childItems;
		}

		public virtual IStoreItem GetItem(string path) {
			throw new Exception("The method or operation is not implemented.");
		}
		#endregion

		/*
		 * NOTE: This is not used yet!
		 * The idea will is to embed the LocalSharePointObjectModel Assembly as a resource
		 * which allows us to defer loading until needed. This serves two purposes:
		 *
		 * A) The referenced assembly doesn't need to be in the GAC, and can still be found immediately.
		 * B) We can load this provider on a non-sharepoint box since it will not try to load MS.SharePoint.dll
		 * 
		 */ 
		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {
			
			string[] resources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
			
			foreach (string resource in resources) {
				
				string baseName = resource.Substring(0, resource.LastIndexOf('.'));
				ResourceManager resourceManager = new ResourceManager(baseName, Assembly.GetExecutingAssembly());
				ResourceSet resourceSet = resourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);
				IDictionaryEnumerator enumerator = resourceSet.GetEnumerator();

				while (enumerator.MoveNext()) {
					object obj = enumerator.Value;
					if (obj is byte[]) {
						try {
							Assembly assembly = Assembly.Load((byte[])obj);
							if (args.Name == assembly.GetName().FullName) {
								return assembly;
							}
						} catch {

						}
					}
				}
			}
			return null;
		}
	}

	/// <summary>
	/// Not used... just an idea floating around...
	/// </summary>
	[Flags()]
	public enum SPItemType {
		Unknown = 0, // default until path is parsed
		Alert = 1,
		Group = 2,
		List = 4,
		Role = 8,
		User = 16,
		Web = 32,

		Alerts = Alert | Container,
		Groups = Group | Container,
		Lists = List | Container,
		Roles = Role | Container,
		Users = User | Container,
		Webs = Web | Container,

		Container = 32768
	}
}
