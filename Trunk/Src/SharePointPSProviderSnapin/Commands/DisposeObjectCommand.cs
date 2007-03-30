using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell.SharePoint.Commands
{
    [Cmdlet("Dispose", "Object")]
    public class DisposeObjectCommand : Cmdlet
    {
        private PSObject[] m_inputObjects;

        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        public PSObject[] InputObject
        {
            get
            {
                return m_inputObjects;
            }
            set
            {
                m_inputObjects = value;
            }
        }

        protected override void ProcessRecord()
        {
            foreach (PSObject psObj in m_inputObjects)
            {
                IDisposable disposable = psObj.BaseObject as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
