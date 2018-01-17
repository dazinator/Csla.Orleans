using Csla.Server;
using Orleans.Runtime.Configuration;
using Orleans.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Csla.Orleans
{

    public class OrleansGrainDataPortal : Csla.Server.DataPortal, IOrleansGrainDataPortalServer
    {
        public OrleansGrainDataPortal()
        {
           
        }
    }
}
