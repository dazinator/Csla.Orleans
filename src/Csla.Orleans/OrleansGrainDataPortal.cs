using System;
using System.Threading.Tasks;
using Csla.Server;
using Orleans;

namespace Csla.Orleans
{
    public class OrleansGrainDataPortal : Grain, IOrleansGrainDataPortalServer
    {
        public OrleansGrainDataPortal()
        {
           
        }

        public Server.DataPortal DataPortal { get; set; }

        public override Task OnActivateAsync()
        {
            DataPortal = new Server.DataPortal();
            return base.OnActivateAsync();
        }

        public async Task<DataPortalResult> Create(Type objectType, object criteria, DataPortalContext context, bool isSync)
        {
            return await DataPortal.Create(objectType, criteria, context, isSync);
        }

        public async Task<DataPortalResult> Delete(Type objectType, object criteria, DataPortalContext context, bool isSync)
        {
            return await DataPortal.Delete(objectType, criteria, context, isSync);
        }

        public async Task<DataPortalResult> Fetch(Type objectType, object criteria, DataPortalContext context, bool isSync)
        {
            return await DataPortal.Fetch(objectType, criteria, context, isSync);
        }

        public async Task<DataPortalResult> Update(object obj, DataPortalContext context, bool isSync)
        {
            var result = await DataPortal.Update(obj, context, isSync);
            return result;
        }
    }
}
