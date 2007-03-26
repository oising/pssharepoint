using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell
{
	/// <summary>
	/// 
	/// </summary>
	public interface IDynamicParametersProvider
	{
		RuntimeDefinedParameterDictionary GetItemDynamicParameters { get; }
		RuntimeDefinedParameterDictionary SetItemDynamicParameters { get; }
		RuntimeDefinedParameterDictionary ClearItemDynamicParameters { get; }
		RuntimeDefinedParameterDictionary ItemExistsDynamicParameters { get; }
		RuntimeDefinedParameterDictionary InvokeItemDynamicParameters { get; }
		RuntimeDefinedParameterDictionary GetChildItemsDynamicParameters { get; }
		RuntimeDefinedParameterDictionary GetChildNamesDynamicParameters { get; }	
		RuntimeDefinedParameterDictionary NewItemDynamicParameters { get; }
		RuntimeDefinedParameterDictionary MoveItemDynamicParameters { get; }
		RuntimeDefinedParameterDictionary CopyItemDynamicParameters { get; }
        RuntimeDefinedParameterDictionary GetContentReaderDynamicParameters { get; }
        RuntimeDefinedParameterDictionary GetContentWriterDynamicParameters { get; }
	}
}
