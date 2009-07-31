using System;
using System.Collections.Generic;
using System.Text;
using HubKey.Web.Services.SharePoint;
using Nivot.PowerShell.SharePoint.ObjectModel;

namespace Nivot.PowerShell.SharePoint
{
    public class ObjectModelFactory : ISharePointObjectModelFactory
    {
        #region ISharePointObjectModelFactory Members

        public SharePointObjectModel GetInstance(Uri siteCollectionUrl, bool remote)
        {
            if (remote)
            {
                return new Sts3RemoteSharePointObjectModel(siteCollectionUrl);
            }
            else
            {
                throw new NotImplementedException("Local Object Model access for Sts3 not implemented. Use -remote switch to use web service interface.");
            }
        }

        #endregion
    }
}
