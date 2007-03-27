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
using System.Reflection;
using System.Web.Caching;

using System.Management.Automation;

namespace Nivot.PowerShell
{
	/// <summary>
	/// A wrapper class for facilitating manipulation and enumeration of a native backing-store object and its children.
	/// </summary>
    /// <typeparam name="TNative">The type of the backing-store object this wrapper exposes.</typeparam>
    public abstract partial class StoreItem<TNative> : IStoreItem, IDisposable where TNative : class
	{
        private TNative m_storeObject;

        /// <summary>
        /// Has this wrapper (and the underlying backing-store object) been disposed?
        /// </summary>
        protected bool IsDisposed = false;

        /// <summary>
        /// 
        /// </summary>
		protected Dictionary<Type, Delegate> AddActions;

        /// <summary>
        /// 
        /// </summary>
		protected Dictionary<Type, Delegate> RemoveActions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeObject"></param>
        protected StoreItem(TNative storeObject)
        {
            m_storeObject = storeObject;

            AddActions = new Dictionary<Type, Delegate>();
            RemoveActions = new Dictionary<Type, Delegate>();

            Provider.WriteDebug("Constructing " + GetType().Name);            
        }

		/// <summary>
		/// Gets the native backing-store object this wrapper exposes.
		/// </summary>
        protected virtual TNative NativeObject
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
        /// Gets the PSObject wrapped native backing-store object this wrapper exposes. This method is called to obtain the object to write to the pipeline.
        /// <remarks>This method should be overridden in derived classes to facilitate decoration of the output object. </remarks>
        /// </summary>
        /// <returns>A PSObject wrapping the native backing-store object.</returns>
        public virtual PSObject GetPSObject()
        {
            return new PSObject(NativeObject);
        }

        /// <summary>
        /// Gets the current StoreProviderBase derived provider that is utilising this object.
        /// <remarks>Use this member to invoke WriteVerbose, WriteDebug and other instance methods on the active provider instance in a thread-safe manner.</remarks>
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

        protected void RegisterAdder<TSubject>(Action<TSubject> action) where TSubject : class
		{
			EnsureNotDisposed();
            AddActions.Add(typeof(TSubject), action);
		}

        protected void RegisterRemover<TSubject>(Action<TSubject> action) where TSubject : class
		{
			EnsureNotDisposed();
            RemoveActions.Add(typeof(TSubject), action);
		}

		protected Delegate GetAddAction(IStoreItem item)
		{
			EnsureNotDisposed();
			Delegate addAction;

			if (AddActions.TryGetValue(item.NativeObject.GetType(), out addAction))
			{
				return addAction;
			}

			return null;
		}

		protected Delegate GetRemoveAction(IStoreItem item)
		{
			EnsureNotDisposed();
			Delegate removeAction;

			if (RemoveActions.TryGetValue(item.NativeObject.GetType(), out removeAction))
			{
				return removeAction;
			}

			return null;
		}

		#endregion

		#region IStoreItem Members

        /// <summary>
        /// Try to add a store item to this item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
		public virtual bool AddChildItem(IStoreItem item)
		{
			EnsureNotDisposed();
			Delegate addAction = GetAddAction(item);

			if (addAction != null)
			{
				try
				{
					// try to add item
					addAction.DynamicInvoke(item.NativeObject);
					return true;
				}
				catch (TargetInvocationException ex)
				{
				    throw new BackingStoreException("AddChildItem Failed!", ex.InnerException);
				}
			}
			// no adder found
			return false;
		}

        /// <summary>
        /// Try to remove a store item from this item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
		public virtual bool RemoveChildItem(IStoreItem item)
		{
			EnsureNotDisposed();
			Delegate removeAction = GetRemoveAction(item);

			if (removeAction != null)
			{
				try
				{
					// try to remove item
					removeAction.DynamicInvoke(item.NativeObject);
					return true;
				}
				catch (TargetInvocationException ex)
				{
                    throw new BackingStoreException("RemoveChildItem Failed!", ex.InnerException);
				}
			}
			// no remover found
			return false;
		}

		public virtual void InvokeItem()
		{			
			throw new NotImplementedException("The method or operation is not implemented.");
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

	    public virtual CacheItemPriority CachePriority
	    {
	        get
	        {
	            return CacheItemPriority.Default;
	        }
	    }

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
                        NativeObject = null; // hence reference-type restraint
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

        /// <summary>
        /// TODO: allow user-defined external scriptblock actions ;-) 
        /// </summary>
        /// <typeparam name="TSubject"></typeparam>
        /// <param name="target"></param>
        /// <param name="subject"></param>
        public delegate void StoreItemAction<TSubject>(TNative target, TSubject subject) where TSubject : class;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeItem"></param>
        /// <returns></returns>
        public static implicit operator TNative(StoreItem<TNative> storeItem)
        {
            return storeItem.NativeObject;
        }
	}
}