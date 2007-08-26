#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Management.Automation;

namespace Nivot.PowerShell.ObjectModel
{
	/// <summary>
	///
	/// </summary>
	public abstract class IndexerObjectModelBase<TProvider> : IStoreObjectModel, IDisposable
        where TProvider : StoreProviderBase
	{
	    private readonly string m_root = null;

        protected bool IsDisposed;

        protected IndexerObjectModelBase(string root)
        {
            m_root = root.Replace("/", @"\");
        }

		protected static TProvider Provider
		{
			get
			{
				return StoreProviderContext.Current as TProvider;
			}
		}

        protected static void EnsureNotNullOrEmpty(string item)
        {
            if (String.IsNullOrEmpty(item))
            {
                ArgumentException ex = new ArgumentException(item + " is null or empty.");
                Provider.ThrowTerminatingError(
                    new ErrorRecord(ex, "ArgNullOrEmpty", ErrorCategory.InvalidArgument, item));
            }
        }

	    protected abstract IStoreItem GetRootStoreItem();

		#region IStoreObjectModel Members

		public virtual bool IsValidPath(string path)
		{
			if (String.IsNullOrEmpty(path))
			{
			    return false;
			}
            return true;
		}

		public virtual bool HasChildItems(string path)
		{
			Provider.WriteVerbose("IndexerObjectModelBase::HasChildItems " + path);

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
					    childItem.Dispose(); // prevent leak
						break;
					}
					childItems.Add(childItem);
				}
				return childItems;
			}			
		}

		//public abstract IStoreItem GetItem(string path);

        public virtual IStoreItem GetItem(string path)
        {
            EnsureNotDisposed();
            EnsureNotNullOrEmpty(path);

            char separator = Provider.ProviderInfo.PathSeparator;

            // always a minimum of '\'
            string[] chunks = path.Split(separator);

            // get the root item to start the search from
            IStoreItem storeItem = GetRootStoreItem();

            if (path == separator.ToString())
            {
                return storeItem; // at root
            }            

            // index into object hierarchy
            foreach (string chunk in chunks)
            {
                if (chunk == String.Empty)
                {
                    // skip first chunk
                    continue;
                }

                // use indexer to find this chunk
                using (IStoreItem parentItem = storeItem) {                 
                    storeItem = parentItem[chunk];
                }

                if (storeItem == null)
                {
                    return null;
                }                
            }
            return storeItem;

    
        }

        /// <summary>
        /// 
        /// </summary>
	    public virtual string Root
	    {
            get { return m_root; }
	    }

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
            try
            {
                Dispose(true);
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        ~IndexerObjectModelBase()
        {
            Dispose(false);
        }

        #endregion
    }
}