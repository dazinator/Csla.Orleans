using Csla.DataPortalClient;
using Orleans;
using System;

namespace Csla.Orleans
{
    public class OrlansGrainDataPortalProxyFactory : IDataPortalProxyFactory
    {
        private readonly Func<Type, IClusterClient> _clusterClientFactory;

        public OrlansGrainDataPortalProxyFactory(Func<Type, IClusterClient> clusterClientFactory)
        {
            _clusterClientFactory = clusterClientFactory;
        }

        public virtual IDataPortalProxy Create(Type objectType)
        {
            var clusterClient = _clusterClientFactory(objectType);
            return CreateProxy(clusterClient);
        }

        private IDataPortalProxy CreateProxy(IClusterClient clusterClient)
        {
            return new OrleansDataPortalProxy(clusterClient);
        }

        public void ResetProxyType()
        {
           // throw new NotImplementedException();
        }
    }
}
