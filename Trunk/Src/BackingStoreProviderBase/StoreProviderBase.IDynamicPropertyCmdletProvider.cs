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
    public abstract partial class StoreProviderBase : IDynamicPropertyCmdletProvider
    {

		// rename-itemproperty
		// remove-itemproperty
		// new-itemproperty
		// copy-itemproperty
		// move-itemproperty

		#region IDynamicPropertyCmdletProvider Overrides

		/// <summary>
		/// Copies a property of the item at the specified path to a new
		/// property on the destination item.
		/// </summary>
		/// 
		/// <param name="sourcePath">
		/// The path to the item from which to copy the property.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The name of the property to copy.
		/// </param>
		/// 
		/// <param name="destinationPath">
		/// The path to the item on which to copy the property to.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The destination property to copy to.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to copy
		/// properties of provider objects using the copy-itemproperty cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not copy properties 
		/// from or to objects that are generally hidden from the user unless 
		/// the Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/>
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false. 
		/// 
		/// This method should call 
		/// <see cref="CmdletProvider.ShouldProcess"/>
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void CopyProperty(string sourcePath, string sourceProperty,
						string destinationPath, string destinationProperty)
		{
			// Write code here to modify property of 
			// an item after performing necessary
			// validations
			//
			// WritePropertyObject(destinationProperty, destinationPath );

			// Example 
			// 
			// if (ShouldProcess(destinationPath, "copy property"))
			// {
			//      // Copy property and then call WritePropertyObject
			//      WritePropertyObject(destinationProperty, destinationPath);
			// }

		} // CopyProperty

		/// <summary>
		/// Allows the provider a attach additional parameters to the
		/// copy-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="sourcePath">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The name of the property to copy.
		/// </param>
		/// 
		/// <param name="destinationPath">
		/// The path to the item on which to copy the property to.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The destination property to copy to.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no
		/// dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object CopyPropertyDynamicParameters(string sourcePath,
							string sourceProperty, string destinationPath,
								string destinationProperty)
		{
			return null;
		} // CopyPropertyDynamicParameters

		/// <summary>
		/// Moves a property on an item specified by the path.
		/// </summary>
		/// 
		/// <param name="sourcePath">
		/// The path to the item from which to move the property.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The name of the property to move.
		/// </param>
		/// 
		/// <param name="destinationPath">
		/// The path to the item on which to move the property to.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The destination property to move to.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to move
		/// properties from one provider object to another using the move-itemproperty cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not move properties on 
		/// or to objects that are generally hidden from the user unless the 
		/// Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/> 
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// This method should call 
		/// <see cref="CmdletProvider.ShouldProcess"/> 
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void MoveProperty(string sourcePath, string sourceProperty,
						string destinationPath, string destinationProperty)
		{
			// Write code to move property of an item
			// here after performing necessary validations
			//
			// WritePropertyObject( destinationProperty, destinationPath );

			// Example
			// 
			// if (ShouldProcess(destinationPath, "move property"))
			// {
			//      // Move the properties and then call WritePropertyObject
			//      WritePropertyObject(destinationProperty, destinationPath);
			// }

		} // MoveProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// move-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="sourcePath">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The name of the property to move.
		/// </param>
		/// 
		/// <param name="destinationPath">
		/// The path to the item on which to move the property to.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The destination property to move to.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a
		/// <see cref="System.Management.Automation"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object MovePropertyDynamicParameters(string sourcePath,
							string sourceProperty, string destinationPath,
								string destinationProperty)
		{
			return null;
		} // MovePropertyDynamicParameters

		/// <summary>
		/// Creates a new property on the specified item
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item on which the new property should be created.
		/// </param>
		/// 
		/// <param name="propertyName">
		/// The name of the property that should be created.
		/// </param>
		/// 
		/// <param name="type">
		/// The type of the property that should be created.
		/// </param>
		/// 
		/// <param name="value">
		/// The new value of the property that should be created.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to add
		/// properties to provider objects using the new-itemproperty cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not create new 
		/// properties on objects that are generally hidden from the user unless
		/// the Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/> 
		/// method if the path represents an item that is hidden from the user
		/// and Force is set to false.
		/// 
		/// This method should call 
		/// <see cref="CmdletProvider.ShouldProcess"/> 
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void NewProperty(string path, string propertyName, string type,
						object value)
		{
			// Write code here to create a new property
			// after performing necessary validations
			//
			// WritePropertyObject( propertyValue, path );

			// setting 
			// 
			// if (ShouldProcess(path, "new property"))
			// {
			//      // Set the new property and then call WritePropertyObject
			//      WritePropertyObject(value, path);
			// }
		} // NewProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// new-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="propertyName">
		/// The name of the property that should be created.
		/// </param>
		/// 
		/// <param name="type">
		/// The type of the property that should be created.
		/// </param>
		/// 
		/// <param name="value">
		/// The new value of the property that should be created.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object NewPropertyDynamicParameters(string path,
							string propertyName, string type, object value)
		{
			return null;
		} // NewPropertyDynamicParameters

		/// <summary>
		/// Removes a property on the item specified by the path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item from which the property should be removed.
		/// </param>
		/// 
		/// <param name="propertyName">
		/// The name of the property to be removed.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to
		/// remove properties from provider objects using the remove-itemproperty
		/// cmdlet.
		/// 
		/// Providers that declare
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not remove properties 
		/// on objects that are generally hidden from the user unless the Force 
		/// property is set to true. An error should be sent to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WriteError"/>
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// This method should call 
		/// <see cref="System.Management.Automatin.CmdletProvider.ShouldProcess"/>
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void RemoveProperty(string path, string propertyName)
		{
			// Write code here to remove property of 
			// of an item after performing necessary
			// operations

			// Example 
			// if (ShouldProcess(propertyName, "delete"))
			//{
			//    // delete the property
			//}
		} // RemoveProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// remove-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="propertyName">
		/// The name of the property that should be removed.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a 
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object RemovePropertyDynamicParameters(string path,
			string propertyName)
		{
			return null;
		} // RemovePropertyDynamicParameters

		/// <summary>
		/// Renames a property of the item at the specified path
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item on which to rename the property.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The property to rename.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The new name of the property.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to 
		/// rename properties of provider objects using the rename-itemproperty
		/// cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not rename properties 
		/// on objects that are generally hidden from the user unless the Force
		/// property is set to true. An error should be sent to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WriteError"/> 
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// This method should call
		/// <see cref="System.Management.Automation.CmdletProvider.ShouldProcess"/> 
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void RenameProperty(string path, string sourceProperty,
			string destinationProperty)
		{
			// Write code here to rename a property after
			// performing necessary validaitions
			//
			// WritePropertyObject(destinationProperty, path);

			// Example 
			// if (ShouldProcess(destinationProperty, "delete"))
			//{
			//    // Delete property here
			//}
		} // RenameProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// rename-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="sourceProperty">
		/// The property to rename.
		/// </param>
		/// 
		/// <param name="destinationProperty">
		/// The new name of the property.
		/// </param>
		/// 
		/// <returns>
		/// An object that has properties and fields decorated with parsing
		/// attributes similar to a cmdlet class or a
		/// <see cref="System.Management.Automation.PseudoParameterDictionary"/>.
		/// Null can be returned if no dynamic parameters are to be added.
		/// </returns>
		/// 
		/// <remarks>
		/// The default implementation returns null.
		/// </remarks>
		public object RenamePropertyDynamicParameters(string path,
							string sourceProperty, string destinationProperty)
		{
			return null;
		} // RenamePropertyDynamicParameters

		#endregion IDynamicPropertyCmdletProvider Overrides
	}
}