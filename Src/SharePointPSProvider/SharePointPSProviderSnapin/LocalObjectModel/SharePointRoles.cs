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
    
        class SharePointRoles : StoreItem<SPRoleCollection> {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="roles"></param>
            public SharePointRoles(SPRoleCollection roles)
                : base(roles) {

				// add SPRole
				RegisterAdder<SPRole>(new Action<IStoreItem>(
					delegate(IStoreItem item) {
						SPRole role = (SPRole)item.NativeObject;
						NativeObject.Add(role.Name, role.Description, role.PermissionMask);
					}
				));

				// remove SPRole
				RegisterRemover<SPRole>(new Action<IStoreItem>(
					delegate(IStoreItem item) {
						SPRole role = (SPRole)item.NativeObject;
						NativeObject.Remove(role.ID); // or role.Name
					}
				));
            }

			public override IEnumerator<IStoreItem> GetEnumerator() {

				// default child item for SPRoleCollection is SPRole
				foreach (SPRole role in NativeObject) {
					yield return new SharePointRole(role);
				}
			}

			public override bool IsContainer {
				get {
					return true;
				}
			}

			public override string ChildName {
				get {
					return "!Roles";
				}
			}

			public override StoreItemFlags ItemFlags {
				get {
					return StoreItemFlags.TabComplete;
				}
			}
        }
    
}
