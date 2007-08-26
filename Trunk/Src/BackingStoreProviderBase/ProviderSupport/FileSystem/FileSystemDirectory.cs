#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nivot.PowerShell.ProviderSupport.FileSystem
{
    public class FileSystemDirectory : FileSystemTarget<DirectoryInfo>
    {
        public FileSystemDirectory(DirectoryInfo storeObject)
            : base(storeObject)
        {
            this.RegisterAdder<DirectoryInfo>(
                delegate(DirectoryInfo source)
                {
                    CopyDirectory(source.FullName, NativeObject.FullName);
                });

            this.RegisterAdder<FileInfo>(
                delegate(FileInfo source)
                {
                    bool force = StoreProviderContext.Current.Force.IsPresent;
                    source.CopyTo(NativeObject.FullName, force);
                });
        }

        internal static void CopyDirectory(string src, string dst)
        {
            string[] files;
            if (!Directory.Exists(dst))
            {
                Directory.CreateDirectory(dst);
            }
            files = Directory.GetFileSystemEntries(src);
            foreach (string element in files)
            {
                if (Directory.Exists(element))
                {
                    CopyDirectory(element, Path.Combine(dst, Path.GetFileName(element)));
                }
                else
                {
                    File.Copy(element, Path.Combine(dst, Path.GetFileName(element)), true);
                }
            }
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
            get { return true; }
        }
    }
}
