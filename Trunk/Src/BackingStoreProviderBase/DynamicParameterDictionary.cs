#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

// based on code by Jachym Kouba (pscx project "DynamicParameterBuilder")
// modifications: indirected parameter dictionary by StoreProviderMethods enum

using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Nivot.PowerShell
{
    internal class DynamicParameterDictionary
    {
    	private readonly Dictionary<StoreProviderMethods, RuntimeDefinedParameterDictionary> m_parameterDictionary;

		internal DynamicParameterDictionary()
        {
			m_parameterDictionary = new Dictionary<StoreProviderMethods, RuntimeDefinedParameterDictionary>();
        }

        internal void AddSwitchParam(StoreProviderMethods methods, string name)
        {		    
            AddParam<SwitchParameter>(methods, name, false, null);
        }

        internal void AddStringParam(StoreProviderMethods methods, string name)
        {         
			AddStringParam(methods, name, false);
        }

        internal void AddStringParam(StoreProviderMethods methods, string name, bool mandatory)
        {         
			AddStringParam(methods, name, mandatory, null);
        }

        internal void AddStringParam(StoreProviderMethods methods, string name, string parameterSet)
        {         
			AddParam<String>(methods, name, false, parameterSet);
        }

        internal void AddStringParam(StoreProviderMethods methods, string name, bool mandatory, string parameterSet)
        {            
			AddParam<String>(methods, name, mandatory, parameterSet);
        }

        internal void AddParam<T>(StoreProviderMethods methods, string name, bool mandatory, string parameterSet)
        {
		    PerformActionForEachMethod(methods, delegate(StoreProviderMethods method) { EnsureKey(method); });

		    PerformActionForEachMethod(methods,
                delegate(StoreProviderMethods method)
                {
                    ParameterAttribute pa = new ParameterAttribute();
                    pa.ParameterSetName = parameterSet;
                    pa.Mandatory = mandatory;

                    RuntimeDefinedParameter rdp = new RuntimeDefinedParameter();
                    rdp.Name = name;
                    rdp.ParameterType = typeof (T);
                    rdp.Attributes.Add(pa);

                    m_parameterDictionary[method].Add(name, rdp);
                });
        }

        internal RuntimeDefinedParameterDictionary GetDictionary(StoreProviderMethods method)
        {
            if (m_parameterDictionary.ContainsKey(method))
            {
                return m_parameterDictionary[method];
            }
            return null;
        }

        private static void PerformActionForEachMethod(StoreProviderMethods methods, Action<StoreProviderMethods> methodAction)
        {
            Array allMethods = Enum.GetValues(typeof(StoreProviderMethods));
            Array.ForEach((int[])allMethods,
                delegate(int method)
                {
                    if (((int)methods & method) == method)
                    {
                        methodAction((StoreProviderMethods)method);
                    }
                });
        }

        private void EnsureKey(StoreProviderMethods method)
        {
            if (m_parameterDictionary.ContainsKey(method))
            {
                return;
            }
            m_parameterDictionary[method] = new RuntimeDefinedParameterDictionary();
        }
    }
}
