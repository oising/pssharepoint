// original author: Jachym Kouba (pscx project)

using System;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace Nivot.PowerShell
{
    public class DynamicParameterBuilder
    {
        private readonly RuntimeDefinedParameterDictionary m_dictionary;

        public DynamicParameterBuilder()
        {
            m_dictionary = new RuntimeDefinedParameterDictionary();
        }

        public void AddSwitchParam(string name)
        {
            AddParam<SwitchParameter>(name, false, null);
        }

        public void AddStringParam(string name)
        {
            AddStringParam(name, false);
        }

        public void AddStringParam(string name, bool mandatory)
        {
            AddStringParam(name, mandatory, null);
        }

        public void AddStringParam(string name, string parameterSet)
        {
            AddParam<String>(name, false, parameterSet);
        }

        public void AddStringParam(string name, bool mandatory, string parameterSet)
        {
            AddParam<String>(name, mandatory, parameterSet);
        }

        public void AddParam<T>(string name, bool mandatory, string parameterSet)
        {
            ParameterAttribute pa = new ParameterAttribute();
            pa.ParameterSetName = parameterSet;
            pa.Mandatory = mandatory;

            RuntimeDefinedParameter rdp = new RuntimeDefinedParameter();
            rdp.Name = name;
            rdp.ParameterType = typeof(T);
            rdp.Attributes.Add(pa);

            m_dictionary.Add(name, rdp);
        }

        public RuntimeDefinedParameterDictionary GetDictionary()
        {
            return m_dictionary;
        }
    }
}
