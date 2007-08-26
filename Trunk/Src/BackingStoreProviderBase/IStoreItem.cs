#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;
using System.Web.Caching;

namespace Nivot.PowerShell
{
	/// <summary>
	/// Interface for backing-store items; IEnumerable for child items
	/// </summary>
	public interface IStoreItem : IEnumerable<IStoreItem>, IDisposable
	{
		/// <summary>
		/// Try to add a store item to this item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		bool AddChildItem(IStoreItem item);

		/// <summary>
		/// Try to remove a store item from this item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		bool RemoveChildItem(IStoreItem item);

		/// <summary>
		/// Try to invoke this item
		/// </summary>
		void InvokeItem();

		/// <summary>
		/// The underlying native backing-store object
		/// </summary>
		object NativeObject { get; }

        /// <summary>
        /// Get the object for output after applying a decorator, if needed.
        /// </summary>
        /// <returns>The object destined for pipeline output.</returns>
	    object GetOutputObject();

		/// <summary>
		/// Indexer to find a child by name
		/// </summary>
		/// <param name="childName"></param>
		/// <returns></returns>
		IStoreItem this[string childName] { get; }

		/// <summary>
		/// Final path chunk identifying this item, e.g. "web" in "/site/web"
		/// <remarks>Assumes ChildName is unique in its namespace, as is expected in filesystem-like providers.</remarks>
		/// </summary>
		string ChildName { get; }

		/// <summary>
		/// Can we set-location to this item?
		/// </summary>
		bool IsContainer { get; }

		/// <summary>
		/// Flags for how the provider should treat this item, e.g. tab-complete only, don't tab-complete, pipe only etc.
		/// </summary>
		StoreItemOptions ItemOptions { get; }

        /// <summary>
        /// If this item is cacheable, this denotes its cache priority.
        /// </summary>
        CacheItemPriority CachePriority { get; }
	}
}