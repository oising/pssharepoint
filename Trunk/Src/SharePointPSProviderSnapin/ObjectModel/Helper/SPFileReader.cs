using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation.Provider;
using System.Text;
using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel.Helper
{
    sealed class SPFileReader : IContentReader, IDisposable
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
                m_stream.Read(bytes, (int)m_stream.Position, (int) readCount);
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
