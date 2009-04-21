using System;
using System.Collections.Generic;
using System.Text;

namespace Nivot.PowerShell.SharePoint
{
    public static class SharePointConstants
    {
        public const string VersionString = "0.7.0.0";
        public static readonly Version Version = new Version(VersionString);
        public static readonly string Sts2ObjectModelAssemblyName = String.Format("Nivot.PowerShell.SharePoint.Office11, Version={0}, Culture=neutral, PublicKeyToken=21257ce7cdf88373", VersionString);
        public static readonly string Sts3ObjectModelAssemblyName = String.Format("Nivot.PowerShell.SharePoint.Office12, Version={0}, Culture=neutral, PublicKeyToken=21257ce7cdf88373", VersionString);
        public static readonly string Sts2AssemblyFile = "Nivot.PowerShell.SharePoint.Office11.dll";
        public static readonly string Sts3AssemblyFile = "Nivot.PowerShell.SharePoint.Office12.dll";
        public static readonly string FactoryTypeName = "Nivot.PowerShell.SharePoint.ObjectModelFactory";
    }
}
