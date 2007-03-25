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

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	internal class SharePointGroup : StoreItem<SPGroup>
	{
		public SharePointGroup(SPGroup group)
			: base(group)
		{
			// add SPUser
			RegisterAdder<SPUser>(new Action<IStoreItem>(
			                      	delegate(IStoreItem item) { NativeObject.AddUser((SPUser) item.NativeObject); }
			                      	));

			// remove SPUser
			RegisterRemover<SPUser>(new Action<IStoreItem>(
			                        	delegate(IStoreItem item) { NativeObject.RemoveUser((SPUser) item.NativeObject); }
			                        	));

			// add (merge) SPGroup
			RegisterAdder<SPGroup>(new Action<IStoreItem>(
			                       	delegate(IStoreItem item)
			                       		{
			                       			SPGroup sourceGroup = (SPGroup) item.NativeObject;
			                       			foreach (SPUser user in sourceGroup.Users)
			                       			{
			                       				AddItem(new SharePointUser(user));
			                       			}
			                       		}
			                       	));
		}

		public override IEnumerator<IStoreItem> GetEnumerator()
		{
			// pseudo containers
			yield return new SharePointRoles(NativeObject.Roles);
			yield return new SharePointUsers(NativeObject.Users);

			// default child item for SPGroup is SPUser
			foreach (SPUser user in NativeObject.Users)
			{
				yield return new SharePointUser(user);
			}
		}

		public override bool IsContainer
		{
			get { return true; }
		}

		public override string ChildName
		{
			get { return NativeObject.Name; }
		}

		public override StoreItemOptions ItemOptions
		{
			get { return StoreItemOptions.ShouldTabComplete | StoreItemOptions.ShouldPipeItem; }
		}
	}
}