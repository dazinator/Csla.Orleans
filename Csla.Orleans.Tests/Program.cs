using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Runtime.Configuration;
using System;
using System.Reflection;
using Xunit;


namespace Csla.Orleans.Tests
{
    public class Class1
    {
        [Fact]
        public void PassingTest()
        {

            var services = new ServiceCollection();

            var config = ClientConfiguration.LoadFromFile("OrleansClientConfiguration.dev.xml");
            config.SerializationProviders.Add(typeof(CslaOrleansSerialiser).GetTypeInfo());         
            //  config.GatewayProvider = ClientConfiguration.GatewayProviderType.SqlServer;

            
            IClusterClient client = new ClientBuilder()
                .ConfigureApplicationParts(ConfigureApplicationParts)
               // .LoadConfiguration("OrleansConfiguration.dev.xml")
               .ConfigureLogging(ConfigureLogging)
               .UseConfiguration(config)
               .UseServiceProviderFactory(ConfigureServices)
               .Build();

            var proxyFactory = new OrlansGrainDataPortalProxyFactory((t) => { return client; });


            services.ConfigureCsla((a, b) => { a.DataPortalProxyFactory = proxyFactory; });
            var sp = services.BuildServiceProvider();           

        }


        private static void ConfigureApplicationParts(IApplicationPartManager partManger)
        {
            partManger.AddApplicationPart(typeof(IOrleansGrainDataPortalServer).Assembly);
            partManger.AddApplicationPart(typeof(Class1).Assembly);
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
