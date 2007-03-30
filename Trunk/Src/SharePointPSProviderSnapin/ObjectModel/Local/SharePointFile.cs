using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Microsoft.SharePoint;
using System.Management.Automation.Provider;
using Nivot.PowerShell.SharePoint.ObjectModel.Helper;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	class SharePointFile : StoreItem<SPFile>, IContentReader
	{       
	    private IContentReader m_reader = null;

		public SharePointFile(SPFile file) : base(file)
		{
            StoreProviderMethods methods = StoreProviderMethods.GetItem | StoreProviderMethods.GetChildItems;
            RegisterSwitchParameter(methods, SharePointParams.ListItem);
		}

		public override string ChildName
		{
			get { return NativeObject.Name; }
		}

		public override bool IsContainer
		{
			get { return false; }
		}

		public override StoreItemOptions ItemOptions
		{
			get { return StoreItemOptions.ShouldTabComplete | StoreItemOptions.ShouldPipeItem; }
		}

        #region IContentReader Members

        public void Close()
        {
            if (m_reader != null)
            {
                m_reader.Close();
            }
        }

        public IList Read(long readCount)
        {
            EnsureReader();
            return m_reader.Read(readCount);
        }

        public void Seek(long offset, System.IO.SeekOrigin origin)
        {
            EnsureReader();
            m_reader.Seek(offset, origin);
        }

        #endregion

        private void EnsureReader()
        {
            if (m_reader == null)
            {
                m_reader = new SPFileReader(this.NativeObject);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (m_reader != null)
            {
                m_reader.Dispose();
                m_reader = null;
            }
            base.Dispose(disposing);
        }
    }
}
