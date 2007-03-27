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

using System.Collections.Generic;
using System.Web.Caching;

using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
    /// <summary>
    /// 
    /// </summary>
    internal class SharePointWeb : StoreItem<SPWeb>
    {
        internal const string DefaultRole = "Reader";

        public SharePointWeb(SPWeb web)
            : base(web)
        {
            RegisterAdder<SPUser>(AddUser);
            RegisterRemover<SPUser>(RemoveUser);
            RegisterAdder<SPGroup>(AddGroup);
            RegisterRemover<SPGroup>(RemoveGroup);
            RegisterAdder<SPRole>(AddRole);
            RegisterRemover<SPRole>(RemoveRole);
            RegisterRemover<SPWeb>(RemoveWeb);
        }

        public override IEnumerator<IStoreItem> GetEnumerator()
        {
            // pseudo containers first
            yield return new SharePointAlerts(NativeObject.Alerts);
            yield return new SharePointGroups(NativeObject.Groups);
            yield return new SharePointLists(NativeObject.Lists);
            yield return new SharePointRoles(NativeObject.Roles);
            yield return new SharePointUsers(NativeObject.Users);

            // default child item for SPWebCollection is SPWeb
            foreach (SPWeb web in NativeObject.Webs)
            {
                yield return new SharePointWeb(web);
            }
        }

        public override bool IsContainer
        {
            get { return true; }
        }

        public override string ChildName
        {
            get
            {
                string url = NativeObject.ServerRelativeUrl;
                return url.Substring(url.LastIndexOf('/') + 1); // e.g. subsite in /site/subsite
            }
        }

        public override StoreItemOptions ItemOptions
        {
            get { return StoreItemOptions.ShouldTabComplete | StoreItemOptions.ShouldPipeItem | StoreItemOptions.ShouldCache; }
        }

        public override CacheItemPriority CachePriority
        {
            get
            {
                return CacheItemPriority.High;
            }
        }

        #region Adder/Remover Members

        private void AddUser(SPUser user)
        {
            NativeObject.Roles[DefaultRole].AddUser(user);
            NativeObject.Update();
        }

        private void RemoveUser(SPUser user)
        {
            NativeObject.Users.Remove(user.LoginName);
        }

        private void AddRole(SPRole role)
        {
            NativeObject.Roles.Add(role.Name, role.Description, role.PermissionMask);
            NativeObject.Update();
        }

        private void RemoveRole(SPRole role)
        {
            NativeObject.Roles.RemoveByID(role.ID);
        }

        private void AddGroup(SPGroup group)
        {
            NativeObject.Groups.Add(group.Name, group.Owner, group.Users[0], group.Description);
            NativeObject.Update();
        }

        private void RemoveGroup(SPGroup group)
        {
            NativeObject.Groups.RemoveByID(group.ID);
        }

        private void RemoveWeb(SPWeb childWeb)
        {
            NativeObject.Webs[childWeb.ID].Delete();
        }

        #endregion
    }
}