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
	public abstract partial class StoreProviderBase {

        // new-psdrive
        // remove-psdrive

        #region DriveCmdletProvider Overrides

        /// <summary>
        /// Gives the provider an opportunity to validate the drive that is
        /// being added.  It also allows the provider to modify parts of the
        /// PSDriveInfo object.  This may be done for performance or
        /// reliability reasons or to provide extra data to all calls using
        /// the Drive.
        /// </summary>
        /// 
        /// <param name="drive">
        /// The proposed new drive.
        /// </param>
        /// 
        /// <returns>
        /// The new drive that is to be added to the Windows PowerShell namespace.  This
        /// can either be the same <paramref name="drive"/> object that
        /// was passed in or a modified version of it. The default 
        /// implementation returns the drive that was passed.
        /// </returns>
        /// 
        /// <remarks>
        /// This method gives the provider an opportunity to associate 
        /// provider specific information with a drive. This is done by 
        /// deriving a new class from 
        /// <see cref="System.Management.Automation.PSDriveInfo"/>
        /// and adding any properties, methods, or fields that are necessary. 
        /// When this method gets called, the override should create an instance
        /// of the derived PSDriveInfo using the passed in PSDriveInfo. The derived
        /// PSDriveInfo should then be returned. Each subsequent call into the provider
        /// that uses this drive will have access to the derived PSDriveInfo via the
        /// PSDriveInfo property provided by the base class. Implementers of this 
        /// method should verify that the root exists and that a connection to 
        /// the data store (if there is one) can be made.  Any failures should 
        /// be sent to the
        /// <see cref="System.Management.Automation.Provider.CmdletProvider.WriteError(ErrorRecord)"/>
        /// method and null should be returned.
        /// </remarks>
        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            return drive;
        }

        /// <summary>
        /// Allows the provider to attach additional parameters to the 
        /// New-PSDrive cmdlet.
        /// </summary>
        /// 
        /// <returns>
        /// Implementors of this method should return an object that has 
        /// properties and fields decorated with parsing attributes similar 
        /// to a cmdlet class or a 
        /// <see cref="System.Management.Automation"/>.
        /// The default implemenation returns null.
        /// </returns>
        protected override object NewDriveDynamicParameters()
        {
            return null;
        }// NewDriveDynamicParameters

        /// <summary>
        /// Cleans up provider specific data for a drive before it is  
        /// removed. This method gets called before a drive gets removed. 
        /// </summary>
        /// 
        /// <param name="drive">
        /// The Drive object that represents the mounted drive.
        /// </param>
        /// 
        /// <returns>
        /// If the drive can be removed then the drive that was passed in is 
        /// returned. If the drive cannot be removed, null should be returned
        /// and an exception is written to the 
        /// <see cref="System.Management.Automation.Provider.CmdletProvider.WriteError(ErrorRecord)"/>
        /// method. The default implementation returns the drive that was 
        /// passed.
        /// </returns>
        /// 
        /// <remarks>
        /// An implementer has to ensure that this method is overridden to free 
        /// any resources that may be associated with the drive being removed.
        /// </remarks>
        /// 
        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            return drive;
        }

        /// <summary>
        /// Allows the provider to map drives after initialization.
        /// </summary>
        /// 
        /// <returns>
        /// A drive collection with the drives that the provider wants to be 
        /// added to the session upon initialization. The default 
        /// implementation returns an empty 
        /// <see cref="System.Management.Automation.PSDriveInfo"/> collection.
        /// </returns>
        /// 
        /// <remarks>
        /// After the Start method is called on a provider, the
        /// InitializeDefaultDrives method is called. This is an opportunity
        /// for the provider to mount drives that are important to it. For
        /// instance, the Active Directory provider might mount a drive for
        /// the defaultNamingContext if the machine is joined to a domain.
        /// 
        /// All providers should mount a root drive to help the user with
        /// discoverability. This root drive might contain a listing of a set
        /// of locations that would be interesting as roots for other mounted
        /// drives. For instance, the Active Directory provider may create a
        /// drive that lists the naming contexts found in the namingContext
        /// attributes on the RootDSE. This will help users discover
        /// interesting mount points for other drives.
        /// </remarks>
        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            Collection<PSDriveInfo> drives = new Collection<PSDriveInfo>();
            return drives;
        }

        #endregion DriveCmdletProvider Overrides
	}
}