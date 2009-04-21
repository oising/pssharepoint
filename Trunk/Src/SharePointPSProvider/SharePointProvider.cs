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
using System.Diagnostics;

using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Nivot.PowerShell.SharePoint
{
	[CmdletProvider("SharePoint", ProviderCapabilities.ShouldProcess | ProviderCapabilities.Credentials)]
	public class SharePointProvider : StoreProviderBase
	{
		/// <summary>
		/// This is our hook into all runtime checks of the SharePoint object model
		/// </summary>
		public override IStoreObjectModel StoreObjectModel
		{
			get
			{
				SharePointObjectModel objectModel = null;
				
				if (this.PSDriveInfo != null)
				{
					objectModel = ((SharePointDriveInfo)this.PSDriveInfo).ObjectModel;
				}
				else
				{                    
					ThrowTerminatingError(SharePointErrorRecord.NotImplementedError("Must use drive-qualifed paths."));
				}

				return objectModel;
			}
		}
        
		#region Drive methods

		protected override PSDriveInfo NewDrive(PSDriveInfo drive)
		{
			using (EnterContext())
			{
				if (drive is SharePointDriveInfo)
				{
					// whaa, where, how???
					return drive;
				}

				string root = drive.Root;
                
                if (String.IsNullOrEmpty(root))
				{
					WriteError(SharePointErrorRecord.ArgumentNullOrEmpty("Root"));
					return null;
				}

				SharePointDriveInfo driveInfo;

				try
				{
				    var siteCollectionUrl = new Uri(root);

                    driveInfo =
                        new SharePointDriveInfo(drive.Name, ProviderInfo, siteCollectionUrl, "SharePoint Drive", Credential, ParamStsVersion, ParamRemoteIsSet);

					WriteVerbose("PSDriveInfo.Root = " + driveInfo.Root);
				}
                catch (UriFormatException)
                {
                    WriteError(
                        SharePointErrorRecord.ArgumentError(
                            "Invalid root syntax: please use a valid Url."));
                    
                    return null;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    WriteError(new ErrorRecord(ex, "InvalidStsVersion", ErrorCategory.InvalidArgument, null));
                    return null;
                }
				catch (Exception ex)
				{
					Trace.WriteLine(ex, "NewDrive");
				    
                    WriteVerbose(ex.ToString());

                    string message = String.Format("Unable to open site collection at {0} : {1}.", root, ex.Message);
					WriteError(new ErrorRecord(new ArgumentException(
					                           	message, ex), "NewDrive", ErrorCategory.OpenError, null));

					return null;
				}

				return driveInfo;
			}
		}

		protected override object NewDriveDynamicParameters()
		{
			var parameters = new DynamicParameterBuilder();
		    parameters.AddStringParam("Version", true);
			parameters.AddSwitchParam("Remote");
		    parameters.AddSwitchParam("SSL"); // TODO: implement this

			return parameters.ParameterDictionary;
		}

		protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
		{
			if (drive == null)
			{
				var argEx = new ArgumentNullException("drive");
				WriteError(new ErrorRecord(argEx, "NullDrive",
				                           ErrorCategory.InvalidArgument, drive));

				return null;
			}

			if (drive is IDisposable)
			{
				((IDisposable) drive).Dispose();
			}

			return drive;
		}

		#endregion

		private bool ParamRemoteIsSet 
		{
			get { return RuntimeDynamicParameters["Remote"].IsSet; }
		}

        // TODO: support enum dynamic parameters in provider base.
	    private StsVersion ParamStsVersion
	    {
	        get
	        {
	            object value = RuntimeDynamicParameters["Version"].Value;
                if (Enum.IsDefined(typeof(StsVersion), value))
                {
                    return (StsVersion) Enum.Parse(typeof (StsVersion), (string) value, true);
                }
	            throw new ArgumentOutOfRangeException("StsVe" + "rsion",
                    String.Format("Invalid version '{0}'; valid values are Sts2 or Sts3.", value));
	        }
	    }
	}
}