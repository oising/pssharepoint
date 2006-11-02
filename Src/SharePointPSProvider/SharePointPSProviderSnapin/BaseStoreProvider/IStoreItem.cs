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
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell {

	/// <summary>
	/// Interface for backing-store items; IEnumerable for child items
	/// </summary>
	public interface IStoreItem : IEnumerable<IStoreItem> {

		/// <summary>
		/// Try to add a store item to this item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		bool AddItem(IStoreItem item);

		/// <summary>
		/// Try to remove a store item from this item
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		bool RemoveItem(IStoreItem item);

		/// <summary>
		/// The underlying native backing-store object
		/// </summary>
		object NativeObject {
			get;
		}

		/// <summary>
		/// Indexer to find a child by name
		/// </summary>
		/// <param name="childName"></param>
		/// <returns></returns>
		IStoreItem this[string childName] {
			get;
		}

		/// <summary>
		/// Final path chunk identifying this item, e.g. "web" in "/site/web"
		/// <remarks>Assumes ChildName is unique in its namespace, so implementers beware!</remarks>
		/// </summary>
		string ChildName {
			get;
		}

		/// <summary>
		/// Can we set-location to this item?
		/// </summary>
		bool IsContainer {
			get;
		}

		/// <summary>
		/// Flags for how the provider should treat this item, e.g. tab-complete only, don't tab-complete, pipe only etc.
		/// </summary>
		StoreItemFlags ItemFlags {
			get;
		}
	}
}
