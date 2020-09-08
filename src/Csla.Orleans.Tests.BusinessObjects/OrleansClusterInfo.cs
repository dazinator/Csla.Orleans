using System;


//[assembly:KnownAssembly("Csla")]
//[assembly: KnownAssembly("Csla.Orleans.Tests.BusinessObjects")]
//[assembly: Orleans.CodeGeneration.KnownType(typeof(Root))]


namespace Csla.Orleans.Tests.BusinessObjects
{
    public static class OrleansClusterInfo
    {
        public const string ServiceId = "CslaApplicationServer";
        public static readonly Lazy<string> ClusterId = new Lazy<string>(() =>
        {
            return "1.0.0";
        });
    }
}
