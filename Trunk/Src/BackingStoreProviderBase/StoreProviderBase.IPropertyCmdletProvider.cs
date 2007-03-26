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
	public abstract partial class StoreProviderBase : IPropertyCmdletProvider
	{
		// get-itemproperty
		// set-itemproperty
		// clear-itemproperty

		#region IPropertyCmdletProvider Overrides

		/// <summary>
		/// Gets the properties of the item specified by the path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item to retrieve properties from.
		/// </param>
		/// 
		/// <param name="providerSpecificPickList">
		/// A list of properties that should be retrieved. If this parameter is
		/// null or empty, all properties should be retrieved.
		/// </param>
		/// 
		/// <remarks>
		/// 
		/// Providers override this method to give the user the ability to  
		/// retrieve properties to provider objects using the get-itemproperty 
		/// cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not retrieve properties
		/// from objects that are generally hidden from the user unless the 
		/// Force property is set to true. An error should be sent to the 
		/// <see cref="CmdletProvider.WriteError"/>
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// An <see cref="System.Management.Automation.PSObject"/> can be used
		/// as a property bag for the properties that need to be returned if 
		/// the <paramref name="providerSpecificPickList"/> contains multiple 
		/// properties to get.
		///
		/// An instance of <see cref="System.Management.Automation.PSObject"/>
		/// representing the properties that were retrieved should be passed 
		/// to the 
		/// <see cref="CmdletProvider.WritePropertyObject"/> 
		/// method. 
		/// 
		/// It is recommended that you support wildcards in property names 
		/// using <see cref="System.Management.Automation.WildcardPattern"/>.
		/// </remarks>
		public void GetProperty(string path,
								Collection<string> providerSpecificPickList)
		{
			// PSObject psObject = new PSObject();
			// psObject.AddNote( propertyName, propertyValue );
			// WritePropertyObject( propertyValue, path );
		} // GetProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// get-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="providerSpecificPickList">
		/// A list of properties that were specified on the command line.
		/// This parameter may be empty or null if the properties are being
		/// piped into the command.
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
		public object GetPropertyDynamicParameters(string path,
												   Collection<string> providerSpecificPickList)
		{
			return null;
		} // GetPropertyDynamicParameters

		/// <summary>
		/// Sets the specified properties of the item at the specified path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item to set the properties on.
		/// </param>
		/// 
		/// <param name="property">
		/// An PSObject which contains a collection of the names and values
		/// of the properties to be set.
		/// </param>
		/// 
		/// <remarks>
		/// Providers override this method to give the user the ability to set 
		/// the value of provider object properties using the set-itemproperty 
		/// cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not retrieve properties
		/// from objects that are generally hidden from the user unless the 
		/// Force property is set to true. An error should be sent to the 
		/// WriteError 
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// 
		/// An instance of <see cref="System.Management.Automation.PSObject"/>
		/// representing the properties that were set should be passed 
		/// to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WritePropertyObject"/> 
		/// method. 
		/// 
		/// <paramref name="propertyValue"/> is a property bag containing the 
		/// properties that should be set. See 
		/// <see cref="System.Management.Automation.PSObject"/> for more 
		/// information.
		/// 
		/// This method should call 
		/// <see cref="System.Management.Automation.CmdletProvider.ShouldProcess"/>
		/// and check its return value before making any changes to the store 
		/// this provider is working upon.
		/// </remarks>
		public void SetProperty(string path, PSObject propertyValue)
		{
			// Write code here to set the specified property
			// after performing the necessary validations
			//
			// WritePropertyObject(propertyValue, path);

			// Example 
			// 
			//      if (ShouldProcess(path, "set property"))
			//      {
			//          // set the property here and then call WritePropertyObject
			//          WritePropertyObject(propertyValue, path);
			//      }
			// }
		} // SetProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the 
		/// set-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="propertyValue">
		/// An PSObject which contains a collection of the name, and value 
		/// of the properties to be set if they were specified on the command
		/// line.  The PSObject could be empty or null if the parameters are
		/// being piped into the command.
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
		public object SetPropertyDynamicParameters(string path,
												   PSObject propertyValue)
		{
			return null;
		} // SetPropertyDynamicParameters

		/// <summary>
		/// Clears a property of the item at the specified path.
		/// </summary>
		/// 
		/// <param name="path">
		/// The path to the item on which to clear the property.
		/// </param>
		/// 
		/// <param name="propertyToClear">
		/// The name of the property to clear.
		/// </param>
		///
		/// <remarks>
		/// Providers override this method to give the user the ability to clear
		/// the value of provider object properties using the clear-itemproperty 
		/// cmdlet.
		/// 
		/// Providers that declare 
		/// <see cref="System.Management.Automation.Provider.ProviderCapabilities"/>
		/// of ExpandWildcards, Filter, Include, or Exclude should ensure that 
		/// the path passed meets those requirements by accessing the 
		/// appropriate property from the base class.
		/// 
		/// By default, overrides of this method should not clear properties 
		/// from objects that are generally hidden from the user unless the 
		/// Force property is set to true. An error should be sent to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WriteError"/>
		/// method if the path represents an item that is hidden from the user 
		/// and Force is set to false.
		/// 
		/// An <see cref="System.Management.Automation.PSObject"/> can be used
		/// as a property bag for the properties that need to be returned if 
		/// the <paramref name="propertyToClear"/> contains multiple 
		/// properties to write.
		/// 
		/// An instance of <see cref="System.Management.Automation.PSObject"/>
		/// representing the properties that were cleared should be passed 
		/// to the 
		/// <see cref="System.Management.Automation.CmdletProvider.WritePropertyObject"/> 
		/// method. 
		/// 
		/// This method should call 
		/// <see cref="System.Management.Automation.CmdletProvider.ShouldProcess"/> 
		/// and check its return value before making any changes to the store this provider is
		/// working upon.
		/// </remarks>
		public void ClearProperty(string path,
								  Collection<string> propertyToClear)
		{
			// Write code here to clear properties
			// of the item speicified at the path
			// after performing necessary validations

			// Example
			// 
			// if (propertyToClear.Count == 1)
			// {
			//      if (ShouldProcess(path, "clear properties"))
			//      {
			//          // Clear properties and then call WriteItemObject
			//          WritePropertyObject(propertyToClear[0], path);            
			//      }
			// }
		} // ClearProperty

		/// <summary>
		/// Allows the provider to attach additional parameters to the
		/// clear-itemproperty cmdlet.
		/// </summary>
		/// 
		/// <param name="path">
		/// If the path was specified on the command line, this is the path
		/// to the item to get the dynamic parameters for.
		/// </param>
		/// 
		/// <param name="propertyToClear">
		/// The name of the property to clear. This parameter may be null or
		/// empty if the properties to clear are being piped into the command.
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
		public object ClearPropertyDynamicParameters(string path,
													 Collection<string> propertyToClear)
		{
			return null;
		} // ClearPropertyDynamicParameters

		#endregion IPropertyCmdletProvider
	}
}