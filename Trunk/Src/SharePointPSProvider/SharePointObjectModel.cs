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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Nivot.PowerShell;
using Nivot.PowerShell.ObjectModel;

namespace Nivot.PowerShell.SharePoint
{
    public interface ISharePointObjectModelFactory
    {
        SharePointObjectModel GetInstance(Uri siteCollectionUrl, bool remote);
    }

	/// <summary>
	/// Base factory class for getting access to the SharePoint Object Model, either local or remote.
	/// </summary>
    public abstract class SharePointObjectModel : IndexerObjectModelBase<SharePointProvider>
	{        
        private readonly Uri m_internalRoot = null;
	    private readonly StsVersion m_stsVersion;

        // TODO: move this into v2, make new for v3
		private readonly static Regex s_pathRegex = new Regex(
            @"(\\(?:[^!\\]+\\?)*)(?:(!users|!groups|!roles|!alerts|!lists)\\?([^!\\]+)?)?",
            (RegexOptions.IgnoreCase | RegexOptions.Compiled));
        
        protected SharePointObjectModel() {}

        protected SharePointObjectModel(Uri siteCollectionUrl, StsVersion version) : base(ToRootString(siteCollectionUrl))
        {
            m_internalRoot = siteCollectionUrl;
            m_stsVersion = version;
        }

        internal static string ToRootString(Uri url)
        {
            // http://my.server.com:8080/site/web
            // => my.server.com.8080\site\web
            string root = String.Format("{0}.{1}{2}", url.Host, url.Port, url.AbsolutePath);
            root = root.Replace("/", @"\"); // flip slashes
            return root;
        }

	    protected internal Uri InternalRoot
	    {
            get { return m_internalRoot; }
	    }

		internal static SharePointObjectModel GetSharePointObjectModel(Uri siteCollectionUrl, StsVersion version, bool remote)
		{			            
		    string assemblyFile;

            switch (version)
            {
                case StsVersion.Sts2:
                    assemblyFile = SharePointConstants.Sts2AssemblyFile;
                    break;

                case StsVersion.Sts3:
                    assemblyFile = SharePointConstants.Sts3AssemblyFile;
                    break;

                default:
                    throw new NotSupportedException("StsVersion not supported.");
            }
            
		    ISharePointObjectModelFactory factory;

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                assemblyFile = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                            assemblyFile);

                factory = (ISharePointObjectModelFactory)
                  AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(
                      assemblyFile, SharePointConstants.FactoryTypeName);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }

		    return factory.GetInstance(siteCollectionUrl, remote);
		}

		public abstract Version SharePointVersion
		{
			get;
		}
        
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Debug.WriteLine("Resolving " + args.Name);
            return Assembly.Load(new AssemblyName(args.Name));
        }

		#region IStoreObjectModel Members

		public override bool IsValidPath(string path)
		{
			return s_pathRegex.IsMatch(path);
		}

		#endregion
    }

    public enum StsVersion
    {
        Sts2, Sts3
    }
}