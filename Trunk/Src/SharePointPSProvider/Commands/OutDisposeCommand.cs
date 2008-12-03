using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell.SharePoint.Commands
{
    [Cmdlet(VerbsData.Out, "Dispose")]
    public class OutDisposeCommand : Cmdlet
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
                try
                {
                    IDisposable disposable = psObj.BaseObject as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex, "OutDisposeCommand");
                    WriteError(new ErrorRecord(ex, "DisposeError", ErrorCategory.InvalidOperation, psObj));
                }
            }
        }
    }
}
