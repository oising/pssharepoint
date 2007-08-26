#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
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
        /// The default implementation returns null.
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