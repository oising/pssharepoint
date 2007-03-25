using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

namespace Nivot.PowerShell
{
    interface ICacheable
    {
        bool ShouldCache { get; }
        CacheItemPriority Priority { get; }
    }
}
