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

namespace Nivot.PowerShell
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IDynamicParametersProvider
	{
		RuntimeDefinedParameterDictionary GetDynamicParameters(StoreProviderMethods method);
		void SetDynamicParameters(StoreProviderMethods method, RuntimeDefinedParameterDictionary parameters);
		void ClearDynamicParameters(StoreProviderMethods method);
	}
}
