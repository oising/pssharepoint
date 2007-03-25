using System;
using System.Collections.Generic;
using System.Text;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
    public interface ISharePointObject
    {
        bool IsRemote { get; }
    }
}
