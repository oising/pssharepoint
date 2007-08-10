using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nivot.PowerShell.ProviderSupport.FileSystem
{
    public abstract class FileSystemTarget<TDestination> : StoreItem<TDestination> 
        where TDestination : FileSystemInfo
    {
        protected FileSystemTarget(TDestination storeObject) : base(storeObject)
        {            
        }

        public void RegisterConverter<TInput, TOutput>(Converter<TInput, TOutput> convert)
            where TInput : IStoreItem
            where TOutput : IStoreItem
        {
            this.RegisterAdder<TInput>(
                delegate(TInput sourceObject)
                {
                    Provider.WriteDebug(String.Format("Invoking conversion from {0} to {1}.",
                        typeof (TInput), typeof (TOutput)));

                    TOutput convertedSourceObject = convert(sourceObject);

                    Provider.WriteDebug(String.Format("Attempting to invoke adder for {0}.",
                        typeof(TOutput)));

                    AddChildItem(convertedSourceObject);
                });            
        }
    }
}
