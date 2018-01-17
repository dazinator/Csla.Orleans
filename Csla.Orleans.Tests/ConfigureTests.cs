using Csla.Orleans.Tests.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Runtime.Configuration;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Csla.Orleans.ServiceCollectionExtensions;

namespace Csla.Orleans.Tests
{
    public class ConfigureTests
    {

        private static readonly ManualResetEvent _serverStartedEvent = new ManualResetEvent(false);      

        public ConfigureTests()
        {
            // TODO: start test host automatically..
        }

        [Fact]
        public async Task Can_Configure_ProxyFactory()
        {

            var services = new ServiceCollection();

            IClusterClient orleansClient = GetClient();
            var proxyFactory = new OrlansGrainDataPortalProxyFactory((t) => { return orleansClient; });
            services.ConfigureCsla((a, b) => { a.DataPortalProxyFactory = proxyFactory; });
            var sp = services.BuildServiceProvider();

            var configuredCslaOptions = sp.GetRequiredService<CslaOptions>();
            Assert.Same(Csla.DataPortal.ProxyFactory, proxyFactory);

        }

        [Fact]
        public async Task Can_Fetch()
        {

            var services = new ServiceCollection();

            IClusterClient orleansClient = GetClient();
            var proxyFactory = new OrlansGrainDataPortalProxyFactory((t) => { return orleansClient; });
            services.ConfigureCsla((a, b) => { a.DataPortalProxyFactory = proxyFactory; });
            var sp = services.BuildServiceProvider();

            var configuredCslaOptions = sp.GetRequiredService<CslaOptions>();

            Root.NewRoot();



        }

        [Fact]
        public async Task Can_Create()
        {

            var services = new ServiceCollection();

            IClusterClient orleansClient = GetClient();
            var proxyFactory = new OrlansGrainDataPortalProxyFactory((t) => { return orleansClient; });
            services.ConfigureCsla((a, b) => { a.DataPortalProxyFactory = proxyFactory; });
            var sp = services.BuildServiceProvider();

            var configuredCslaOptions = sp.GetRequiredService<CslaOptions>();

            var root = Root.NewRoot();
            root.Data = "ya";
            root = await root.SaveAsync();
            Assert.Equal("ya", root.Data);

        }

        private IClusterClient GetClient()
        {
            //  var config = ClientConfiguration.LoadFromFile("OrleansClientConfiguration.dev.xml");
            var config = ClientConfiguration.LocalhostSilo(30000);
            config.SerializationProviders.Add(typeof(CslaOrleansSerialiser).GetTypeInfo());

            IClusterClient client = new ClientBuilder()
               .ConfigureApplicationParts(ConfigureApplicationParts)
               .ConfigureLogging(ConfigureLogging)
               .UseConfiguration(config)               
               .UseServiceProviderFactory(ConfigureServices)
               .Build();

            // Orleans clients are designed to be built once, and then a single instance used application wide, across many threads.
            return client;
        }

        private static void ConfigureApplicationParts(IApplicationPartManager partManger)
        {
            partManger.AddApplicationPart(typeof(IOrleansGrainDataPortalServer).Assembly);
            partManger.AddApplicationPart(typeof(ConfigureTests).Assembly);
            partManger.AddApplicationPart(typeof(Root).Assembly);
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddLogging();

            return services.BuildServiceProvider();
        }

        private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddConsole();
        }


    }
}
