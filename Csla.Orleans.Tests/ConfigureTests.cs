using Csla.Orleans.Tests.BusinessObjects;
using Csla.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.CodeGeneration;
using Orleans.Configuration;
using Orleans.Runtime.Configuration;
using Orleans.Serialization;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static Csla.Orleans.ServiceCollectionExtensions;



//[assembly: KnownAssembly("Csla")]
//[assembly: KnownAssembly("Csla.Orleans.Tests.BusinessObjects")]
//[assembly: KnownType(typeof(Root))]

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
        public async Task Can_Serialise_and_Deserialise()
        {

            var root = Root.NewRoot();
            root.Data = "ya";

            var formatter = SerializationFormatterFactory.GetFormatter();

            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, root);
                memoryStream.Position = 0;

                var originalBytes = memoryStream.ToArray();

                memoryStream.Position = 0;
                var result = formatter.Deserialize(memoryStream);

                var newRoot = result as Root;
                Assert.NotNull(newRoot);
            }
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

            Csla.ApplicationContext.User = new Csla.Security.UnauthenticatedPrincipal();
            root = await root.SaveAsync();
            Assert.Equal("ya", root.Data);



        }


        private IClusterClient GetClient()
        {

            var client = new ClientBuilder()
                    .ConfigureLogging(ConfigureLogging)
                    // Clustering information
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = OrleansClusterInfo.ClusterId.Value;
                        options.ServiceId = OrleansClusterInfo.ServiceId;
                    })
                    .Configure<SerializationProviderOptions>(a => a.SerializationProviders.Add(typeof(CslaOrleansSerialiser).GetTypeInfo()))
                    .UseLocalhostClustering()

                    // Clustering provider
                    // .UseAzureStorageClustering(options => options.ConnectionString = connectionString)
                    // Application parts: just reference one of the grain interfaces that we use
                    .ConfigureApplicationParts(ConfigureApplicationParts)
                    .ConfigureServices(ConfigureServices)
                    .Build();

            // Orleans clients are designed to be built once, and then a single instance used application wide, across many threads.
            return client;
        }

        private static void ConfigureApplicationParts(IApplicationPartManager partManger)
        {
            partManger.AddApplicationPart(typeof(IOrleansGrainDataPortalServer).Assembly);
            partManger.AddApplicationPart(typeof(ConfigureTests).Assembly);
            partManger.AddApplicationPart(typeof(Root).Assembly);
            partManger.AddApplicationPart(typeof(Csla.ApplicationContext).Assembly);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddLogging();

            //return services.BuildServiceProvider();
        }

        private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {

            Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
        .CreateLogger();

            loggingBuilder.AddConsole();
            loggingBuilder.AddSerilog();
        }

    }
}
