using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using HubKey.Web.Services.SharePoint;

using System.Management.Automation.Provider;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	class SharePointFile : StoreItem<SPFile>, IContentReader
	{       
	    private IContentReader m_reader;

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
            try
            {
                if (m_reader != null)
                {
                    m_reader.Dispose();
                    m_reader = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        sealed class SPFileReader : IContentReader
        {
            private bool m_isDisposed;
            private readonly MemoryStream m_stream;

            internal SPFileReader(SPFile file)
            {
                m_stream = new MemoryStream(file.OpenBinary());
            }

            IList IContentReader.Read(long readCount)
            {
                EnsureNotDisposed();

                List<byte> blocks = new List<byte>();

                byte[] bytes = new byte[readCount];
                checked
                {
                    m_stream.Read(bytes, (int)m_stream.Position, (int)readCount);
                }
                blocks.AddRange(bytes);

                //for (int index = 0; index < readCount; index++)
                //{
                //    ReadByte(blocks);
                //}

                return blocks;
            }

            void IContentReader.Seek(long offset, SeekOrigin origin)
            {
                EnsureNotDisposed();

                m_stream.Seek(offset, origin);
            }

            void IContentReader.Close()
            {
                Dispose();
            }

            //private void ReadByte(List<byte> blocks)
            //{            
            //    blocks.Add((byte)m_stream.ReadByte());
            //}

            private void EnsureNotDisposed()
            {
                if (m_isDisposed)
                {
                    throw new ObjectDisposedException("SPFileReader");
                }
            }

            public void Dispose()
            {
                if (m_stream != null)
                {
                    m_isDisposed = true;
                    m_stream.Dispose();
                }
            }
        }
    }
}
