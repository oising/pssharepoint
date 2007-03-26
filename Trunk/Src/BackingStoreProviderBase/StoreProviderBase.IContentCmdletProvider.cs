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
	public abstract partial class StoreProviderBase : IContentCmdletProvider
	{

		// get-content
		// set-content
		// clear-content

		#region IContentCmdletProvider Overrides

		/// <summary>
		/// Gets an IContentReader from the provider for the item at the
		/// specified path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the object to be opened for reading content.
		/// </param>
		/// 
		/// <returns>
		/// An IContentReader for the specified content.
		/// </returns>
		/// 
		/// <remarks>
		/// Overrides of this method should return an 
		/// <see cref="System.Management.Automation.Provider.IContentReader"/>
		/// for the item specified by the path.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not return a content 
		/// reader for objects that are generally hidden from the user unless 
		/// the Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/>
		/// method if
		/// the path represents an item that is hidden from the user and Force 
		/// is set to false.
		/// </remarks>
		public IContentReader GetContentReader(string path)
		{
			return null;
		} // GetContentReader

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// get-content cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with
		/// parsing attributes similar to a cmdlet class or a 
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object GetContentReaderDynamicParameters(string path)
		{
			return null;
		} // GetContentReaderDynamicParameters

		/// <summary>
		/// Gets an IContentWriter from the provider for the content at the
		/// specified path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the object to be opened for writing content.
		/// </param>
		/// 
		/// <returns>
		/// An IContentWriter for the item at the specified path.
		/// </returns>
		/// 
		/// <remarks>
		/// Overrides of this method should return an 
		/// <see cref="System.Management.Automation.Provider.IContentWriter"/>
		/// for the item specified by the path.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not return a content 
		/// writer for objects that are generally hidden from 
		/// the user unless the Force property is set to true. An error should 
		/// be sent to the WriteError method if the path represents an item 
		/// that is hidden from the user and Force is set to false.
		/// </remarks>
		public IContentWriter GetContentWriter(string path)
		{
			return null;
		} // GetContentWriter

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// set-content and add-content cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation will returns null.
		/// </remarks>
		public object GetContentWriterDynamicParameters(string path)
		{
			return null;
		} // GetContentWriterDynamicParameters

		/// <summary>
		/// Clears the content from the specified item.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item to clear the content from.
		/// </param>
		///
		/// <remarks>
		/// Overrides of this method should remove any content from the object 
		/// but not remove (delete) the object itself.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not clear or write 
		/// objects that are generally hidden from the user unless the Force 
		/// property is set to true. An error should be sent to the WriteError 
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// This method should call 
		/// <see cref="System.Management.Automation.Providers.CmdletProvider.ShouldProcess"/>
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void ClearContent(string path)
		{
			// Write code here to clear contents
			// after performing necessary validations

			// Example 
			// 
			// if (ShouldProcess(path, "clear"))
			// {
			//      // Clear the contents and then call WriteItemObject
			//      WriteItemObject("", path, false);
			// }
		} //ClearContent

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// clear-content cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with
		/// parsing attributes similar to a cmdlet class or a 
		/// <see cref="System.Management.Automation"/>.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object ClearContentDynamicParameters(string path)
		{
			return null;
		} // ClearContentDynamicParameters

		#endregion IContentCmdletProvider Overrides
	}
}