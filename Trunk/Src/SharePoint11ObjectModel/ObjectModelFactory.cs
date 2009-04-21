using System;
using System.Collections.Generic;
using System.Text;
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
                throw new NotImplementedException();
            }
            else
            {
                return new Sts2LocalSharePointObjectModel(siteCollectionUrl);
            }
        }

        #endregion
    }
}
