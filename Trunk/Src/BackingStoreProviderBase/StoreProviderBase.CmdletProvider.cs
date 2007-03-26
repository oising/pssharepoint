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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using System.Text.RegularExpressions;

namespace Nivot.PowerShell
{
	/// <summary>
	/// 
	/// </summary>
	public abstract partial class StoreProviderBase
	{
        #region CmdletProvider Overrides

        /// <summary>
        /// Gives the provider the opportunity to initialize itself.
        /// </summary>
        /// 
        /// <param name="providerInfo">
        /// The information about the provider that is being started.
        /// </param>
        /// 
        /// <returns>
        /// Either the providerInfo that was passed or a derived class
        /// of ProviderInfo that was initialized with the provider information
        /// that was passed.
        /// </returns>
        /// 
        /// <remarks>
        /// The default implementation returns the ProviderInfo instance that
        /// was passed.To have session state maintain persisted data on behalf
        /// of the provider, the provider should derive from 
        /// <see cref="System.Management.Automation.ProviderInfo"/>
        /// and add any properties or methods for the data it wishes to persist.
        /// When Start gets called the provider should construct an instance of 
        /// its derived ProviderInfo using the providerInfo that is passed in 
        /// and return that new instance.
        /// </remarks>
        protected override ProviderInfo Start(ProviderInfo providerInfo)
        {
            WriteVerbose("Start()");
            return new StoreProviderInfo(providerInfo);
        }

        /// <summary>
        /// Gets an object that defines the additional parameters for the
        /// Start implementation for a provider.
        /// </summary>
        /// 
        /// <returns>
        /// Overrides of this method should return an object that has properties
        /// and fields decorated with parsing attributes similar to a cmdlet 
        /// class or a 
        /// <see cref="System.Management.Automation"/>.
        /// The default implemenation returns null.
        /// </returns>
        protected override object StartDynamicParameters()
        {
            return null;
        }

        /// <summary>
        /// Uninitialize the provider. Called by Session State when the provider
        /// is being removed.
        /// </summary>
        /// 
        /// <remarks>
        /// This is the time to free up any resources that the provider
        /// was using. The default implementation in CmdletProvider does 
        /// nothing.
        /// </remarks>
        protected override void Stop()
        {
            WriteVerbose("Stop()");
        }

        #endregion CmdletProvider Overrides
	}
}