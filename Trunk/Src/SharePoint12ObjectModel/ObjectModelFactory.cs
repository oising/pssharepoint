using System;
using System.Collections.Generic;
using System.Text;

namespace Nivot.PowerShell.SharePoint
{
    public class ObjectModelFactory : ISharePointObjectModelFactory
    {
        #region ISharePointObjectModelFactory Members

        public SharePointObjectModel GetInstance(Uri siteCollectionUrl, bool remote)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
