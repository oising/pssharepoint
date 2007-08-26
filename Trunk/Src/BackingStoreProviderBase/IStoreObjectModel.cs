#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

using System.Collections.ObjectModel;

namespace Nivot.PowerShell
{
	/// <summary>
	/// Interface for backing-store provider.
	/// </summary>
	public interface IStoreObjectModel
	{        
		bool IsValidPath(string path);
		bool ItemExists(string path);
		bool HasChildItems(string path);
		IStoreItem GetItem(string path);
		Collection<IStoreItem> GetChildItems(string path);

        /// <summary>
        /// What root was this OM instantiated from?
        /// </summary>
        string Root { get; }
	}
}