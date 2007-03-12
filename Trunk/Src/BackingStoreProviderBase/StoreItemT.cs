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
using System.Management.Automation;

namespace Nivot.PowerShell
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class StoreItem<T> : IStoreItem, IStoreItemDynamicProperties,
		IDisposable where T : class
	{
		private T m_storeObject;
		private bool m_isDisposed = false;

		// lookup tables (via type) for delegates that can add or remove items to/from this type
		protected Dictionary<Type, Action<IStoreItem>> AddActions;
		protected Dictionary<Type, Action<IStoreItem>> RemoveActions;

		protected StoreItem(T storeObject)
		{
			m_storeObject = storeObject;
			AddActions = new Dictionary<Type, Action<IStoreItem>>();
			RemoveActions = new Dictionary<Type, Action<IStoreItem>>();
		}

		public virtual T NativeObject
		{
			get { return m_storeObject; }
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
			AddActions.Add(typeof(I), addAction);
		}

		protected void RegisterRemover<I>(Action<IStoreItem> removeAction)
		{
			RemoveActions.Add(typeof(I), removeAction);
		}

		protected Action<IStoreItem> GetAddAction(IStoreItem item)
		{
			Action<IStoreItem> addAction;

			if (AddActions.TryGetValue(item.NativeObject.GetType(), out addAction))
			{
				return addAction;
			}

			return null;
		}

		protected Action<IStoreItem> GetRemoveAction(IStoreItem item)
		{
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
			get { return m_storeObject; }
		}

		public abstract string ChildName { get; }

		public abstract bool IsContainer { get; }

		public abstract StoreItemFlags ItemFlags { get; }

		#endregion

		#region IStoreItemDynamicProperties Members

		public virtual RuntimeDefinedParameterDictionary GetItemDynamicProperties
		{
			get { return null; }
		}

		public virtual RuntimeDefinedParameterDictionary SetItemDynamicProperties
		{
			get { return null; }
		}

		public virtual RuntimeDefinedParameterDictionary GetChildItemsDynamicProperties
		{
			get { return null; }
		}

		public virtual RuntimeDefinedParameterDictionary GetChildNamesDynamicProperties
		{
			get { return null; }
		}

		public virtual RuntimeDefinedParameterDictionary ClearItemDynamicProperties
		{
			get { return null; }
		}

		public virtual RuntimeDefinedParameterDictionary NewItemDynamicProperties
		{
			get { return null; }
		}

		public virtual RuntimeDefinedParameterDictionary MoveItemDynamicProperties
		{
			get { return null; }
		}

		public virtual RuntimeDefinedParameterDictionary CopyItemDynamicProperties
		{
			get { return null; }
		}

		public virtual RuntimeDefinedParameterDictionary InvokeItemDynamicProperties
		{
			get { return null; }
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
			return ((IEnumerable<IStoreItem>)this).GetEnumerator();
		}

		#endregion

		#region IDisposable Members

		public virtual void Dispose(bool disposing)
		{
			if (!m_isDisposed)
			{
				// not already called?
				if (disposing)
				{
					// explicit dispose called: safe to assume NativeObject has not
					// been disposed through finalization
					if (NativeObject is IDisposable)
					{
						((IDisposable)NativeObject).Dispose();
					}
				}
				m_isDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		~StoreItem()
		{
			Dispose(false);
		}
	}

	[Flags]
	public enum StoreItemFlags
	{
		None = 0,
		/// <summary>
		/// Should appear in Tab Completion list
		/// </summary>
		TabComplete = 1,
		/// <summary>
		/// Should be sent to object pipeline
		/// </summary>
		PipeItem = 2
	}
}