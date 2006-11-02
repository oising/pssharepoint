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
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text;

using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint {
	public class LocalSharePointObjectModel : SharePointObjectModel, IDisposable {
		
		SPSite m_site;

		public LocalSharePointObjectModel(Uri url)
			: base(url) {
			m_site = new SPSite(url.ToString()); // initialize object model
		}

		public override IStoreItem GetItem(string path) {
			
			// always a minimum of '\'
			string[] chunks = path.Split(SharePointPSProvider.PathSeparator[0]);
			
			// start at root SPWeb
			IStoreItem storeItem = new SharePointWeb(m_site.RootWeb);
			if (path == SharePointPSProvider.PathSeparator) {
				return storeItem; // at root
			}

			foreach (string chunk in chunks) {
				if (chunk == String.Empty) {
					continue; // skip first chunk
				}
				storeItem = storeItem[chunk]; // use indexer to find this chunk
				if (storeItem == null) {
					return null;
				}
			}
			return storeItem;
		}

		#region IDisposable Members

		public void Dispose() {
			if (m_site != null) {
				m_site.Dispose();
			}
		}

		#endregion
	}
}
