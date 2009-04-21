#define __ROOTWEB

#region BSD License Header

/*
 * Copyright (c) 2006, Oisin Grehan @ Nivot Inc (www.nivot.org)
 * All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
 * Neither the name of Nivot Incorporated nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#endregion

using System;
using Microsoft.SharePoint;
using Nivot.PowerShell.ObjectModel;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
    internal class Sts2LocalSharePointObjectModel : SharePointObjectModel
    {
        private SPSite m_site;        

        internal Sts2LocalSharePointObjectModel(Uri siteCollectionUrl) : base(siteCollectionUrl, StsVersion.Sts2)
        {
            m_site = new SPSite(siteCollectionUrl.ToString());

            try
            {
                // Attempt to open a web: this will trigger an error if the url is not a valid sharepoint site.
                using (SPWeb web = m_site.OpenWeb())
                {
                    Provider.WriteVerbose("OpenWeb() succeeded: got " + web.Name);
                }
            }
            // FIXME: catch only OpenWeb thrown exceptions
            catch
            {
                Provider.ThrowTerminatingError(
                    SharePointErrorRecord.InvalidOperationError("InvalidSite", "Invalid site collection root: cannot open SPWeb."));
            }
        }

        internal Sts2LocalSharePointObjectModel(SPSite site) : base(new Uri(site.Url), StsVersion.Sts2)
        {
            m_site = site;
        }

        internal SPSite SiteCollection
        {
            get
            {
                EnsureNotDisposed();
                return m_site;
            }
        }

        public override Version SharePointVersion
        {
            get
            {
                return SharePointUtils.GetSharePointVersion();
            }
        }

        protected override IStoreItem GetRootStoreItem()
        {
#if __ROOTWEB
            IStoreItem storeItem = new SharePointWeb(m_site.RootWeb);
#else
            // start at SPSite
            IStoreItem storeItem = new SharePointSite(m_site);
#endif
            return storeItem;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!IsDisposed)
                {
                    if (disposing)
                    {
                        if (m_site != null)
                        {
                            m_site.Dispose();
                            m_site = null;
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
    }
}