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
using System.Diagnostics;
using System.Web.Caching;

using System.Management.Automation;

namespace Nivot.PowerShell
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract partial class StoreItem<T> : IStoreItem, ICacheable, IDisposable where T : class
	{
		private T m_storeObject;

        protected bool IsDisposed = false;

		protected Dictionary<Type, Action<IStoreItem>> AddActions;
		protected Dictionary<Type, Action<IStoreItem>> RemoveActions;

		protected StoreItem(T storeObject)
		{
			m_storeObject = storeObject;

			AddActions = new Dictionary<Type, Action<IStoreItem>>();
			RemoveActions = new Dictionary<Type, Action<IStoreItem>>();

		    Provider.WriteDebug("Constructing " + GetType().Name);
		}

		/// <summary>
		/// 
		/// </summary>
		protected virtual T NativeObject
		{
			get
			{
				EnsureNotDisposed();
				return m_storeObject;
			}

            private set
            {
                m_storeObject = value;
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual PSObject GetPSObject()
        {
            return new PSObject(NativeObject);
        }

        /// <summary>
        /// 
        /// </summary>
        protected static StoreProviderBase Provider
        {
            get
            {
                StoreProviderBase provider = StoreProviderContext.Current;
                Debug.Assert(provider != null, "StoreProviderContext.Current != null");
                
                return provider;
            }
        }

		#region Indexer (ChildName)

		/// <summary>
		/// This indexer is used to find a child item by name
		/// </summary>
		/// <param name="childName"></param>
		/// <returns></returns>
		public virtual IStoreItem this[string childName]
		{
			get
			{
				EnsureNotDisposed();

				// cycle through children
				foreach (IStoreItem storeItem in this)
				{
					// FIXME: using case-insensitive comparison; this may not always be true.
					if (String.Compare(storeItem.ChildName, childName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return storeItem;
					}
				}
				return null;
			}
		}

		#endregion

		#region Protected Members

		protected void RegisterAdder<I>(Action<IStoreItem> addAction)
		{
			EnsureNotDisposed();
			AddActions.Add(typeof(I), addAction);
		}

		protected void RegisterRemover<I>(Action<IStoreItem> removeAction)
		{
			EnsureNotDisposed();
			RemoveActions.Add(typeof(I), removeAction);
		}

		protected Action<IStoreItem> GetAddAction(IStoreItem item)
		{
			EnsureNotDisposed();
			Action<IStoreItem> addAction;

			if (AddActions.TryGetValue(item.NativeObject.GetType(), out addAction))
			{
				return addAction;
			}

			return null;
		}

		protected Action<IStoreItem> GetRemoveAction(IStoreItem item)
		{
			EnsureNotDisposed();
			Action<IStoreItem> removeAction;

			if (RemoveActions.TryGetValue(item.NativeObject.GetType(), out removeAction))
			{
				return removeAction;
			}

			return null;
		}

		#endregion

		#region IStoreItem Members

		public virtual bool AddItem(IStoreItem item)
		{
			EnsureNotDisposed();
			Action<IStoreItem> addAction = GetAddAction(item);

			if (addAction != null)
			{
				try
				{
					// try to add item
					addAction(item);
					return true;
				}
				catch (Exception exception)
				{
					throw new ApplicationFailedException("Native Store Error", exception);
				}
			}
			// no adder found
			return false;
		}

		public virtual bool RemoveItem(IStoreItem item)
		{
			EnsureNotDisposed();
			Action<IStoreItem> removeAction = GetRemoveAction(item);

			if (removeAction != null)
			{
				try
				{
					// try to remove item
					removeAction(item);
					return true;
				}
				catch (Exception exception)
				{
					throw new ApplicationFailedException("Native Store Error", exception);
				}
			}
			// no remover found
			return false;
		}

		public virtual void InvokeItem()
		{			
			throw new Exception("The method or operation is not implemented.");
		}

		object IStoreItem.NativeObject
		{
			get
			{
				EnsureNotDisposed();
				return m_storeObject;
			}
		}

		public abstract string ChildName { get; }

		public abstract bool IsContainer { get; }

		public abstract StoreItemOptions ItemOptions { get; }

		#endregion

		#region IEnumerable<IStoreItem> Members

		public virtual IEnumerator<IStoreItem> GetEnumerator()
		{
			yield break;
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			EnsureNotDisposed();
			return ((IEnumerable<IStoreItem>)this).GetEnumerator();
		}

		#endregion

        #region ICacheable Members

        public virtual bool ShouldCache
        {
            get
            {
                return true;
            }
        }

        public virtual CacheItemPriority Priority
        {
            get
            {
                return CacheItemPriority.Default;
            }
        }

        #endregion

		#region IDisposable Members

		public virtual void Dispose(bool disposing)
		{
            Debug.WriteLine(String.Format("Dispose({0})", disposing), GetType().Name);

			if (!IsDisposed)
			{
				// not already called?
				if (disposing)
				{
                    // explicit dispose called: safe to assume NativeObject has not
					// been disposed through finalization
					if (NativeObject is IDisposable)
					{
						((IDisposable)NativeObject).Dispose();                        
                        NativeObject = null; // hence T : class restraint
					}
				}
				IsDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		protected void EnsureNotDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(GetType().Name, "Object has been already disposed!");
			}
		}

		~StoreItem()
		{            
			Dispose(false);
		}
	}
}