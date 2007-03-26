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
        
        // get-item
        // item-exists

        #region ItemCmdletProvider Overrides

        protected override bool IsValidPath(string path)
        {
            WriteDebug("IsValidPath: " + path);

            return StoreObjectModel.IsValidPath(NormalizePath(path));
        }

        protected override void GetItem(string path)
        {
            WriteDebug("GetItem: " + path);

            using (EnterContext())
            {
                // FIXME: need better path normalization
                path = NormalizePath(path);

                try
                {
                    IStoreItem item = StoreObjectModel.GetItem(path);

                    // FIXME: redundant? itemexists called first?
                    Debug.Assert(item != null);

                    PSObject output = item.GetPSObject();

                    WriteItemObject(output, path, item.IsContainer);
                }
                catch (Exception ex)
                {
                    ThrowTerminatingError(
                        new ErrorRecord(ex, String.Format("GetItem('{0}')", path),
                                        ErrorCategory.NotSpecified, null));
                }
            }
        }

        protected override object GetItemDynamicParameters(string path)
        {
            WriteDebug("GetItemDynamicParameters: " + path);

            using (EnterContext())
            {
                IDynamicParametersProvider item = StoreObjectModel.GetItem(path) as IDynamicParametersProvider;
                if (item != null)
                {
                    WriteDebug("GetItem assigned dynamic parameters.");
                    return item.GetItemDynamicParameters;
                }
                return null;
            }
        }

        protected override bool ItemExists(string path)
        {
            WriteDebug("ItemExists: " + path);

            using (EnterContext())
            {
                try
                {
                    // FIXME: need better path normalization
                    path = NormalizePath(path);

                    return StoreObjectModel.ItemExists(path);
                }
                catch (Exception ex)
                {
                    ThrowTerminatingError(
                        new ErrorRecord(ex, String.Format("ItemExists('{0}')", path),
                                        ErrorCategory.NotSpecified, null));
                }
                return false;
            }
        }

        protected override object ItemExistsDynamicParameters(string path)
        {
            WriteDebug("ItemExistsDynamicParameters: " + path);

            using (EnterContext())
            {
                IDynamicParametersProvider item = StoreObjectModel.GetItem(path) as IDynamicParametersProvider;
                if (item != null)
                {
                    WriteDebug("ItemExists assigned dynamic parameters.");
                    return item.ItemExistsDynamicParameters;
                }
                return null;
            }
        }

        #endregion
	}
}