using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nivot.PowerShell.ProviderSupport.FileSystem
{
    public class FileSystemFile : FileSystemTarget<FileInfo>
    {
        public FileSystemFile(FileInfo storeObject) : base(storeObject)
        {            
        }

        ///<summary>
        /// Final path chunk identifying this item, e.g. "web" in "/site/web"
        ///<remarks>
        ///Assumes ChildName is unique in its namespace, as is expected in filesystem-like providers.
        ///</remarks>
        ///</summary>        
        public override string ChildName
        {
            get { return NativeObject.Name; }
        }

        ///<summary>
        /// Can we set-location to this item?
        ///</summary>
        public override bool IsContainer
        {
            get { return false; }
        }
    }
}
