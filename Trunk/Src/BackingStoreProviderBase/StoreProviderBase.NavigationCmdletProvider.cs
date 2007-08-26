#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using System.Text.RegularExpressions;

namespace Nivot.PowerShell
{

	/// <summary>
	/// 
	/// </summary>
	public abstract partial class StoreProviderBase
	{

        // move-item

		#region NavigationCmdletProvider Overrides

		protected override bool IsItemContainer(string path)
		{
            WriteDebug("IsItemContainer: " + path);
		    path = NormalizePath(path);

			using (EnterContext())
			{
				using (IStoreItem item = StoreObjectModel.GetItem(path))
				{
					return item.IsContainer;
				}
			}
		}

		protected override void MoveItem(string path, string destination)
		{
			WriteDebug(String.Format("MoveItem: {0} -> {1}", path, destination));

            if (ArePathsEquivalent(path, destination))
            {
                ThrowTerminatingError(StoreProviderErrorRecord.InvalidArgument("source and destination are the same."));
            }

			using (EnterContext())
			{
				if (ShouldProcess(destination, "Move"))
				{
					// TODO: implement FastMove overrides

					CopyItem(path, destination, false);
					if (ItemExists(destination))
					{
						RemoveItem(path, false);
					}
					else
					{
						WriteWarning("Aborting removal of source item: unable to verify item was copied to " + destination);
					}
				}
			}
		}

		#endregion
	}
}