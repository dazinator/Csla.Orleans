using Csla.DataPortalClient;
using Csla.Server;
using Orleans;
using System;
using System.Threading.Tasks;

namespace Csla.Orleans
{
    public class OrleansDataPortalProxy : IDataPortalProxy
    {
        private IClusterClient _client;

        public OrleansDataPortalProxy(IClusterClient client)
        {
            _client = client;
        }

        public bool IsServerRemote => true; // Todo: maybe some way to tell if using an orleans cluster hosted in same process?

        protected async virtual Task<IOrleansGrainDataPortalServer> GetProxy(Type objectType)
        {
            await _client.Connect();

            //  var imageName = "Windows Server 2012 R2";
            var grain = _client.GetGrain<IOrleansGrainDataPortalServer>(0);
            return grain;
        }

        public async Task<DataPortalResult> Create(Type objectType, object criteria, DataPortalContext context, bool isSync)
        {
            // see https://github.com/MarimerLLC/csla/blob/158cede6e12bdb0b6fb738fe8eba50206ccba485/Samples/NET/cs/ExtendableWcfPortalForDotNet/ExtendableWcfPortalForDotNet/Client/WcfProxy.cs          
            var svr = await GetProxy(objectType);
            var response = await svr.Create(objectType, criteria, context, isSync);
            if (response.Error != null)
            {
                throw response.Error;
            }

            return response;
        }

        public async Task<DataPortalResult> Delete(Type objectType, object criteria, DataPortalContext context, bool isSync)
        {
            var svr = await GetProxy(objectType);
            var response = await svr.Delete(objectType, criteria, context, isSync);
            if (response.Error != null)
            {
                throw response.Error;
            }

            return response;
        }

        public async Task<DataPortalResult> Fetch(Type objectType, object criteria, DataPortalContext context, bool isSync)
        {
            var svr = await GetProxy(objectType);
            var response = await svr.Fetch(objectType, criteria, context, isSync);
            if (response.Error != null)
            {
                throw response.Error;
            }

            return response;
        }

        public async Task<DataPortalResult> Update(object obj, DataPortalContext context, bool isSync)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var svr = await GetProxy(obj.GetType());
            var response = await svr.Update(obj, context, isSync);
            if (response.Error != null)
            {
                throw response.Error;
            }

            return response;
        }
    }
}
