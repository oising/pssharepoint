using System;
using System.Collections.Generic;
using System.Text;

namespace Nivot.PowerShell.SharePoint
{
    public static class SharePointConstants
    {
        public const string VersionString = "0.7.0.0";
        public static Version Version = new Version(VersionString);        
        public static string Sts2ObjectModelAssemblyName = String.Format("Nivot.PowerShell.SharePoint.Office11, Version={0}, Culture=neutral, PublicKeyToken=21257ce7cdf88373", VersionString);
        public static string Sts3ObjectModelAssemblyName = String.Format("Nivot.PowerShell.SharePoint.Office12, Version={0}, Culture=neutral, PublicKeyToken=21257ce7cdf88373", VersionString);
    }
}
