using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
    class ListUtils
    {
        static bool Copy(SPList source, SPList destination)
        {
            return false;
        }

        static bool Copy(SPList source, SPWeb destination)
        {
            return false;
        }

        static bool Copy(SPListItem source, SPList destination)
        {
            return false;
        }

        static bool Copy(SPField source, SPList destination)
        {
            return false;
        }

        static string GetSchema(SPList source)
        {
            return source.SchemaXml;
        }

        static string GetSchema(SPListItem source)
        {
            return source.Xml;
        }
    }
}
