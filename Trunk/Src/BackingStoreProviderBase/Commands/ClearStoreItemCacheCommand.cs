using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Clear, "StoreItemCache")]
    public class ClearStoreItemCacheCommand : Cmdlet
    {
        protected override void BeginProcessing()
        {
            StoreItemCache.Instance.Clear();
        }
    }
}
