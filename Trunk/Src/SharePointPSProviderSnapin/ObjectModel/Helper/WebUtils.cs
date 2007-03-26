using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint
{
    static class WebUtils
    {
		static SPWeb RemoveWeb(SPWeb target)
		{
			SPWeb parentWeb = target.ParentWeb;

			// trying to delete root web?
			if (parentWeb.ID == target.ID)
			{
				return target;
			}

			// TODO: delete
			target.Delete();

			return parentWeb;
		}
    }
}
