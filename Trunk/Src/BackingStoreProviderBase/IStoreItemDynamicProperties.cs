using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell
{
	/// <summary>
	/// 
	/// </summary>
	public interface IStoreItemDynamicProperties
	{
		RuntimeDefinedParameterDictionary GetItemDynamicProperties { get; }
		RuntimeDefinedParameterDictionary SetItemDynamicProperties { get; }
		RuntimeDefinedParameterDictionary GetChildItemsDynamicProperties { get; }
		RuntimeDefinedParameterDictionary GetChildNamesDynamicProperties { get; }
		RuntimeDefinedParameterDictionary ClearItemDynamicProperties { get; }
		RuntimeDefinedParameterDictionary NewItemDynamicProperties { get; }
		RuntimeDefinedParameterDictionary MoveItemDynamicProperties { get; }
		RuntimeDefinedParameterDictionary CopyItemDynamicProperties { get; }
		RuntimeDefinedParameterDictionary InvokeItemDynamicProperties { get; }
	}
}
