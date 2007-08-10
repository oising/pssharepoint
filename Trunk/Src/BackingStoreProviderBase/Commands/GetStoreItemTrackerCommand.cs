using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "StoreItemTracker")]
    public class GetStoreItemTrackerCommand : Cmdlet
    {
        private bool m_shouldReset; // = false;

        [Parameter(Mandatory = false)]
        public SwitchParameter Reset
        {
            get
            {
                return m_shouldReset;
            }
            set
            {
                m_shouldReset = value;
            }
        }

        protected override void EndProcessing()
        {
            if (m_shouldReset)
            {
                StoreItemTracker.Reset();
            }
            else
            {
                Dictionary<string, long[]> data = StoreItemTracker.GetTrackerData();
                                
                foreach (string itemName in data.Keys)
                {
                    PSObject output = new PSObject();

                    long constructed = data[itemName][StoreItemTracker.OP_CTOR];
                    long disposed = data[itemName][StoreItemTracker.OP_DISPOSE];
                    long finalized = data[itemName][StoreItemTracker.OP_FINALIZE];

                    output.Properties.Add(new PSNoteProperty("StoreItem", itemName));
                    output.Properties.Add(new PSNoteProperty("Constructed", constructed));
                    output.Properties.Add(new PSNoteProperty("Disposed", disposed )); // explicit disposal
                    output.Properties.Add(new PSNoteProperty("Finalized", finalized)); // finalizer disposal
                    output.Properties.Add(new PSNoteProperty("Total", constructed - (disposed + finalized)));
                    
                    WriteObject(output);
                }
            }
        }
    }
}
