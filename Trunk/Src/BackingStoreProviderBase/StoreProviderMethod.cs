using System;
using System.Collections.Generic;
using System.Text;

namespace Nivot.PowerShell
{
	public enum StoreProviderMethod
	{
		None,
		GetItem,
		SetItem,
		ClearItem,
		ItemExists,
		InvokeDefaultAction,
		GetChildItems,
		GetChildNames,
		NewItem,
		MoveItem,
		CopyItem,
		GetContentReader,
		GetContentWriter,
		SetProperty,
		GetProperty
	}
}