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
