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

        public Task<DataPortalResult> Create(Type objectType, object criteria, DataPortalContext context, bool isSync)
        {
            return DataPortal.Create(objectType, criteria, context, isSync);
        }

        public Task<DataPortalResult> Delete(Type objectType, object criteria, DataPortalContext context, bool isSync)
        {
            return DataPortal.Delete(objectType, criteria, context, isSync);
        }

        public Task<DataPortalResult> Fetch(Type objectType, object criteria, DataPortalContext context, bool isSync)
        {
            return DataPortal.Fetch(objectType, criteria, context, isSync);
        }

        public Task<DataPortalResult> Update(object obj, DataPortalContext context, bool isSync)
        {
            return DataPortal.Update(obj, context, isSync);
        }
    }
}
