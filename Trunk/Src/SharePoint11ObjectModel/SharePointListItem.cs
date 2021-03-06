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
using System.Text;
using System.Management.Automation;

using Microsoft.SharePoint;

namespace Nivot.PowerShell.SharePoint.ObjectModel
{
	internal class SharePointListItem : StoreItem<SPListItem>
	{        
		public SharePointListItem(SPListItem listItem)
			: base(listItem)
		{
		    StoreProviderMethods methods = StoreProviderMethods.GetItem | StoreProviderMethods.GetChildItems;
            RegisterSwitchParameter(methods, SharePointParams.ListItem);
		}

		public override string ChildName
		{
			get { return NativeObject.ID.ToString(); }
		}

		public override bool IsContainer
		{
			get { return false; }
		}

        public override object GetOutputObject()
        {
            PSObject output;

            bool? returnListItems = IsSwitchParameterSet(SharePointParams.ListItem);

            if (returnListItems == true)
			{
				// Native SPListItem
				output = new PSObject(NativeObject);
			}
			else
			{
				// PSCustomObject
				output = new PSObject();

				foreach (SPField field in NativeObject.Fields)
				{
                    if (field.Hidden == false)
                    {
                        string name = field.Title;

                        if (output.Properties.Match(name).Count != 0)
                        {
                            // already added (e.g. Title is used a few times)
                            name = field.InternalName;
                        }

                        PSNoteProperty property = new PSNoteProperty(name, NativeObject[field.InternalName]);
                        output.Properties.Add(property);
                    }
				}

			    //PSNoteProperty nativeItem = new PSNoteProperty("_ListItem", NativeObject);
			    //output.Properties.Add(nativeItem);

                // no longer need native object
                Dispose();
			}

        	return output;
        }

		public override StoreItemOptions ItemOptions
		{
			get { return StoreItemOptions.ShouldTabComplete | StoreItemOptions.ShouldPipeItem; }
		}
	}
}