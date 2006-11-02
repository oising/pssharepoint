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
using System.Text;

using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint {

	/// <summary>
	/// 
	/// </summary>
	class SharePointWeb : StoreItem<SPWeb> {

		public SharePointWeb(SPWeb web)
			: base(web) {

			// add SPUser
			RegisterAdder<SPUser>(new Action<IStoreItem>(
				delegate(IStoreItem item) {
					SPUser user = (SPUser)item.NativeObject;
					NativeObject.Roles["Reader"].AddUser(user);
				}
			));

			// add SPGroup
			RegisterAdder<SPGroup>(new Action<IStoreItem>(
				delegate(IStoreItem item) {
					SPGroup group = (SPGroup)item.NativeObject;
					NativeObject.Groups.Add(group.Name, group.Owner, group.Users[0], group.Description);
				}
			));

			// add SPRole
			RegisterAdder<SPRole>(new Action<IStoreItem>(
				delegate(IStoreItem item) {
					SPRole role = (SPRole)item.NativeObject;
					NativeObject.Roles.Add(role.Name, role.Description, role.PermissionMask);
				}
			));

			// remove SPUser
			RegisterRemover<SPUser>(new Action<IStoreItem>(
				delegate(IStoreItem item) {
					SPUser user = (SPUser)item.NativeObject;
					NativeObject.Users.Remove(user.LoginName);
				}
			));

			// remove SPGroup
			RegisterRemover<SPGroup>(new Action<IStoreItem>(
				delegate(IStoreItem item) {
					SPGroup group = (SPGroup)item.NativeObject;
					NativeObject.Groups.RemoveByID(group.ID);
				}
			));

			// remove SPRole
			RegisterRemover<SPRole>(new Action<IStoreItem>(
				delegate(IStoreItem item) {
					SPRole role = (SPRole)item.NativeObject;
					NativeObject.Roles.RemoveByID(role.ID);
				}
			));

			// remove SPWeb
			RegisterRemover<SPWeb>(new Action<IStoreItem>(
				delegate(IStoreItem item) {
					SPWeb childWeb = (SPWeb)item.NativeObject;
					childWeb.Delete();
				}
			));
		}

		public override IEnumerator<IStoreItem> GetEnumerator() {

			// pseudo containers first
			yield return new SharePointAlerts(NativeObject.Alerts);
			yield return new SharePointGroups(NativeObject.Groups);
			yield return new SharePointLists(NativeObject.Lists);
			yield return new SharePointRoles(NativeObject.Roles);
			yield return new SharePointUsers(NativeObject.Users);

			// default child item for SPWebCollection is SPWeb
			foreach (SPWeb web in NativeObject.Webs) {
				yield return new SharePointWeb(web);
			}
		}

		public override bool IsContainer {
			get {
				return true;
			}
		}

		public override string ChildName {
			get {
				string url = NativeObject.ServerRelativeUrl;
				return url.Substring(url.LastIndexOf('/') + 1); // e.g. subsite in /site/subsite
			}
		}

		public override StoreItemFlags ItemFlags {
			get {
				return StoreItemFlags.TabComplete | StoreItemFlags.PipeItem;
			}
		}
	}
}
