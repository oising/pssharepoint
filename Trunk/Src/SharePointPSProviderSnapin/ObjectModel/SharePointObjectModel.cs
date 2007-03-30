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

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	/// <summary>
	/// Base factory class for getting access to the SharePoint Object Model, either local or remote.
	/// </summary>
	internal abstract class SharePointObjectModel : IStoreObjectModel, IDisposable
	{
		// FIXME: needs to understand provider-qualified paths
		// e.g. 
		//   sharepoint::\\server\site\web (virtual server qualified, search local [then remote])
		//   sharepoint::[\]site\web (default server, local)
		private static Regex s_pathRegex;

        protected bool IsDisposed = false;

		static SharePointObjectModel()
		{
			RegexOptions options = (RegexOptions.IgnoreCase | RegexOptions.Compiled);			
			s_pathRegex = new Regex(@"(\\(?:[^!\\]+\\?)*)(?:(!users|!groups|!roles|!alerts|!lists)\\?([^!\\]+)?)?", options);
		}

		protected static SharePointProvider Provider
		{
			get
			{
				return StoreProviderContext.Current as SharePointProvider;
			}
		}

		internal static SharePointObjectModel GetSharePointObjectModel(Uri siteCollectionUrl, bool remote)
		{
			SharePointObjectModel objectModel;
			
			if (remote)
			{
				objectModel = new RemoteSharePointObjectModel(siteCollectionUrl);
			}
			else
			{
				objectModel = new LocalSharePointObjectModel(siteCollectionUrl);
			}
			return objectModel;
		}

		internal abstract Version SharePointVersion
		{
			get;
		}

		#region IStoreObjectModel Members

		public virtual bool IsValidPath(string path)
		{
			return s_pathRegex.IsMatch(path);
		}

		public virtual bool HasChildItems(string path)
		{
			Provider.WriteVerbose("SharePointObjectModel::HasChildItems called for path " + path);

			using (IStoreItem item = GetItem(path))
			{
				// slight optimization
				if (item.IsContainer)
				{
					foreach (IStoreItem childItem in item)
					{
						using (childItem)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public virtual bool ItemExists(string path)
		{
			bool itemExists;

			using (IStoreItem item = GetItem(path))
			{
				itemExists = (item != null);				
			}

			return itemExists;
		}

		public virtual Collection<IStoreItem> GetChildItems(string path)
		{
			using (IStoreItem parentItem = GetItem(path))
			{				
				Collection<IStoreItem> childItems = new Collection<IStoreItem>();

				foreach (IStoreItem childItem in parentItem)
				{
					if (Provider.Stopping)
					{
						break;
					}
					childItems.Add(childItem);
				}
				return childItems;
			}			
		}

		public abstract IStoreItem GetItem(string path);

		#endregion

        #region IDisposable Members

        protected void EnsureNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                string name = GetType().Name;

                if (disposing)
                {
                    Debug.WriteLine("Dispose()", name);
                }
                else
                {
                    Debug.WriteLine("Finalize()", name);
                }
                IsDisposed = true;
            }            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SharePointObjectModel()
        {
            Dispose(false);
        }

        #endregion
    }
}