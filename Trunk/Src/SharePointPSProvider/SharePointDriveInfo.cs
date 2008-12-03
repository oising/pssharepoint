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
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;

using Nivot.PowerShell.SharePoint.ObjectModel;

//using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint
{	
	public class SharePointDriveInfo : PSDriveInfo, IDisposable
	{
		private SharePointObjectModel m_sharePointObjectModel;
		private readonly string m_virtualServer;
		private readonly bool m_isRemote;
		
		protected bool IsDisposed;

        internal SharePointDriveInfo(string name, ProviderInfo provider, Uri siteCollectionUrl, string description, PSCredential credential, StsVersion version, bool remote)
            : base(name, provider, SharePointObjectModel.ToRootString(siteCollectionUrl), description, credential)
		{            
			m_sharePointObjectModel = SharePointObjectModel.GetSharePointObjectModel(siteCollectionUrl, version, remote);
			m_isRemote = remote;
            m_virtualServer = siteCollectionUrl.Host;
		}

		internal SharePointObjectModel ObjectModel
		{
			get
			{
				EnsureNotDisposed();
				return m_sharePointObjectModel;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string VirtualServer
		{
			get
			{
				EnsureNotDisposed();
				return m_virtualServer;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Version SharePointVersion
		{
			get
			{
				EnsureNotDisposed();
				return m_sharePointObjectModel.SharePointVersion;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsRemote
		{
			get
			{
				EnsureNotDisposed();
				return m_isRemote;
			}
		}

		#region IDisposable Members

		private void EnsureNotDisposed()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("PSDriveInfo " + this.Name);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
			    Debug.WriteLine("Dispose(" + disposing + ")", "SharePointDriveInfo");
				if (disposing)
				{
					if (m_sharePointObjectModel != null)
					{
						m_sharePointObjectModel.Dispose();
						m_sharePointObjectModel = null;
					}					
				}
                IsDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~SharePointDriveInfo()
		{
			Dispose(false);
		}

		#endregion
	}
}