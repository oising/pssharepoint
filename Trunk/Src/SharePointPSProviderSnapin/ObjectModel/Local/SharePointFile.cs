using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Microsoft.SharePoint;
using System.Management.Automation.Provider;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	class SharePointFile : StoreItem<SPFile>, IContentReader
	{
		public SharePointFile(SPFile file) : base(file)
		{			
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
            throw new Exception("The method or operation is not implemented. TEST.");
        }

        public IList Read(long readCount)
        {
            throw new Exception("The method or operation is not implemented. TEST.");
        }

        public void Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new Exception("The method or operation is not implemented. TEST.");
        }

        #endregion
    }
}
