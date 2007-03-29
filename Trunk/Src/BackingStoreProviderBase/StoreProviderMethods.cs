using System;
using System.Collections.Generic;
using System.Text;

namespace Nivot.PowerShell
{
    [Flags]
	public enum StoreProviderMethods
	{
		None                    = 0,
		GetItem                 = 1,
		SetItem                 = 2,
		ClearItem               = 4,
		ItemExists              = 8,
		InvokeDefaultAction     = 16,
		GetChildItems           = 32,
		GetChildNames           = 64,
		NewItem                 = 128,
		MoveItem                = 256,
		CopyItem                = 512,
		GetContentReader        = 1024,
		GetContentWriter        = 2048,
		SetProperty             = 4096,
		GetProperty             = 8192,
        NewDrive                = 16384,
        RemoveDrive             = 32768
	}
}