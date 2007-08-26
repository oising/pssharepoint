#region License: GNU Library General Public License (LGPL)
// LGPL Version 2.1, February 1999
// See included License.txt

// Author: Oisin Grehan
// Contact: oising@gmail.com
// Company: Nivot Inc
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
    public abstract partial class StoreProviderBase : NavigationCmdletProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public new virtual StoreProviderInfo ProviderInfo
        {
            get
            {
                return (StoreProviderInfo)base.ProviderInfo;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public RuntimeDefinedParameterDictionary RuntimeDynamicParameters
        {
            get
            {
                object parameters = this.DynamicParameters;
                WriteDebug("DynamicParameters null: " + (parameters == null));

                return parameters as RuntimeDefinedParameterDictionary;
            }
        }

        /// <summary>
        /// Provides a handle to the runtime object model of the backing store
        /// </summary>
        public abstract IStoreObjectModel StoreObjectModel { get; }

        /// <summary>
        /// Sets the <see cref="ThreadStaticAttribute"/> decorated static member StoreProviderContext&lt;TProvider&gt;.Current to this instance for use as a temporary reference for other classes to acccess this provider's instance methods. 
        /// </summary>
        /// <returns>Returns an <see cref="IDisposable"/> "Cookie" that when disposed, frees the static GCRoot holding this provider instance.</returns>
        protected StoreProviderContext.Cookie EnterContext()
        {
            return StoreProviderContext.Enter(this);
        }

        private RuntimeDefinedParameterDictionary GetDynamicParametersForMethod(StoreProviderMethods method, string path)
        {
            WriteDebug(String.Format("GetDynamicParametersForMethod {0} for path {1}", method, path));

            RuntimeDefinedParameterDictionary parameters = null;

            using (EnterContext())
            {
                // new-item is special-cased
                if (method == StoreProviderMethods.NewItem)
                {
                    // dynamic properties are defined on the parent of the new item
                    // since properties are normally provided by the context object
                    // of the target path (which doesn't exist yet).
                    path = GetParentPath(path, null);
                }

                // read dynamic property information from the context object
                using (IStoreItem item = StoreObjectModel.GetItem(path))
                {
                    if (item != null)
                    {
                        IDynamicParametersProvider parameterProvider = item as IDynamicParametersProvider;
                        if (parameterProvider != null)
                        {
                            int count = 0;

                            parameters = parameterProvider.GetDynamicParameters(method);
                            if (parameters != null)
                            {
                                count = parameters.Count;
                            }
                            WriteDebug(String.Format("{0}: assigned {1} dynamic parameter(s).", method, count));
                        }
                        else
                        {
                            WriteDebug(
                                String.Format("{0} does not implement IDynamicParametersProvider.", item.GetType().Name));
                        }
                    }
                    else
                    {
                        WriteDebug(String.Format("GetDynamicParametersForMethod: path {0} does not exist.", path));
                    }
                }
            }

            return parameters;
        }

        #region Helper Methods

        private bool IsPathDrive(string path)
        {
            // Remove the drive name and first path separator.  If the 
            // path is reduced to nothing, it is a drive. Also if its
            // just a drive then there wont be any path separators
            string root = StoreObjectModel.Root;
            string pathSeparator = this.ProviderInfo.PathSeparator.ToString();

            if (String.IsNullOrEmpty(path.Replace(root, String.Empty)) ||
                String.IsNullOrEmpty(path.Replace(root + pathSeparator, String.Empty)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fix up whatever sort of path string Msh has thrown us
        /// <remarks>FIXME: assumes we're drive-qualified</remarks>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string NormalizePath(string path)
        {
            WriteDebug("NormalizePath: " + path);

            string result = path;

            Debug.Assert(this.StoreObjectModel != null, "StoreObjectModel != null");

            // get basics
            string root = StoreObjectModel.Root;
            string separator = ProviderInfo.PathSeparator.ToString();
            WriteDebug(String.Format("Root: {0}; Separator: {1}", root, separator));

            if (! String.IsNullOrEmpty(path))
            {                
                // ensure trailing slash on path
                if (! path.EndsWith(separator))
                {
                    path += separator;
                }

                // strip out root, if needed.
                if (! String.IsNullOrEmpty(root))
                {
                    WriteDebug("Stripping root.");
                    path = path.Replace(root + separator, String.Empty);
                }

                // flip slashes
                result = path.Replace("/", separator);
            }

            //path = EnsureDriveIsRooted(path);

            //Debug.Assert(path != String.Empty, "path != String.Empty");

            //// ensure drive is rooted
            if (result == String.Empty)
            {
                WriteDebug("Path is empty: ensuring rooted.");
                result = ProviderInfo.PathSeparator.ToString();
            }

            WriteDebug("NormalizePath: returning " + result);

            return result;
        }

        //private static string EnsureDriveIsRooted(string path)
        //{                        
        //    string text = path;
        //    int index = path.IndexOf(':');
        //    if ((index != -1) && ((index + 1) == path.Length))
        //    {
        //        text = path + '\\';
        //    }
        //    return text;
        //}

        private bool ArePathsEquivalent(string pathA, string pathB)
        {
            return NormalizePath(pathA).Equals(NormalizePath(pathB), ProviderInfo.PathComparison);
        }
        #endregion
    }
}