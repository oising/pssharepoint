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
	public abstract partial class StoreProviderBase : NavigationCmdletProvider		
	{
        /// <summary>
        /// 
        /// </summary>
		public new StoreProviderInfo ProviderInfo
		{
			get
			{				
				return (StoreProviderInfo) base.ProviderInfo;
			}
		}

        /// <summary>
        /// 
        /// </summary>
	    public RuntimeDefinedParameterDictionary RuntimeDynamicParameters
	    {
	        get
	        {
	            object parameters = this.DynamicParameters;
	            WriteDebug("DynamicParameters null: " + (parameters == null));

                return parameters as RuntimeDefinedParameterDictionary;
	        }
	    }

		/// <summary>
		/// Provides a handle to the runtime object model of the backing store
		/// </summary>
		public abstract IStoreObjectModel StoreObjectModel { get; }

        /// <summary>
        /// Sets the <see cref="ThreadStaticAttribute"/> decorated static member StoreProviderContext&lt;TProvider&gt;.Current to this instance for use as a temporary reference for other classes to acccess this provider's instance methods. 
        /// </summary>
        /// <returns>Returns an <see cref="IDisposable"/> "Cookie" that when disposed, frees the static GCRoot holding this provider instance.</returns>
		protected StoreProviderContext.Cookie EnterContext()
		{
			return StoreProviderContext.Enter(this);
		}

		private RuntimeDefinedParameterDictionary GetDynamicParametersForMethod(StoreProviderMethods method, string path, params object[] context)
		{
			WriteDebug(String.Format("GetDynamicParametersForMethod {0} for path {1}", method, path));

            RuntimeDefinedParameterDictionary parameters = null;

            using (EnterContext())
            {
                // new-item is special-cased
				if (method == StoreProviderMethods.NewItem)
				{                    
					// dynamic properties are defined on the parent of the new item
					// since properties are normally provided by the context object
					// of the target path (which doesn't exist yet).
					path = GetParentPath(path, null);
				}

				// read dynamic property information from the context object
                using (IStoreItem item = StoreObjectModel.GetItem(path))
                {
                    IDynamicParametersProvider parameterProvider = item as IDynamicParametersProvider;
                    if (parameterProvider != null)
                    {
                        int count = 0;

                        parameters = parameterProvider.GetDynamicParameters(method);
                        if (parameters != null)
                        {
                            count = parameters.Count;
                        }
                        WriteDebug(String.Format("{0} assigned {1} dynamic parameter(s).", method, count));
                    }
					else
                    {
                        WriteDebug(
                            String.Format("{0} does not implement IDynamicParametersProvider.", item.GetType().Name));
                    }
                }
            }

            return parameters;
		}

		#region Helper Methods

		/// <summary>
		/// Fix up whatever sort of path string Msh has thrown us
		/// <remarks>FIXME: assumes we're drive-qualified</remarks>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private string NormalizePath(string path)
		{			
			if (!String.IsNullOrEmpty(path))
			{
                // are we working via a drive?
                if (this.PSDriveInfo != null)
                {
                    path = NormalizePathOnDrive(path);
                }
                else
                {
                    path = NormalizeDrivelessPath(path);
                }
			}

			// ensure drive is rooted
			if (path == String.Empty)
			{
				path = ProviderInfo.PathSeparator.ToString();
			}

			return path;
		}

        protected virtual string NormalizePathOnDrive(string path)
        {
            // flip slashes; remove a trailing slash, if any.
            string driveRoot = this.PSDriveInfo.Root.Replace('/', '\\').TrimEnd('\\');

            // is drive qualified?
            if (path.StartsWith(driveRoot))
            {
                path = path.Replace(driveRoot, String.Empty); // strip it
            }

            return path;
        }

        protected virtual string NormalizeDrivelessPath(string path)
        {
            ThrowTerminatingError(
                new ErrorRecord(new NotImplementedException("Driveless path support not implemented."), "DriveLessPath",
                                ErrorCategory.NotImplemented, path));
            return null;
        }

	    /*
				private bool IsDrive(string path) {
					bool isDrive = (path == String.Format(this.PSDriveInfo.Root + ":" + PathSeparator));
					Dump("IsDrive {0} : {1}", path, isDrive);

					return isDrive;
				}

				private string EnsureDriveIsRooted(string path) {
					Dump("EnsureDriveIsRooted {0}", path);
					if (!path.StartsWith(PathSeparator)) {
						return PathSeparator + path;
					}
					Dump("EnsureDriveIsRooted returning {0}", path);
				
					return path;
				}
		*/

		#endregion
	}
}