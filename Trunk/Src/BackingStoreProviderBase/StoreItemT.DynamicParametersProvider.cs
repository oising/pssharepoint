using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Nivot.PowerShell
{
    public partial class StoreItem<T> : IDynamicParametersProvider
    {
        #region IDynamicParametersProvider Members

        public virtual RuntimeDefinedParameterDictionary GetItemDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary SetItemDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary GetChildItemsDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary GetChildNamesDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary ClearItemDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary NewItemDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary MoveItemDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary CopyItemDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary InvokeItemDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary ItemExistsDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary GetContentReaderDynamicParameters
        {
            get { return null; }
        }

        public virtual RuntimeDefinedParameterDictionary GetContentWriterDynamicParameters
        {
            get { return null; }
        }

        #endregion
    }
}
