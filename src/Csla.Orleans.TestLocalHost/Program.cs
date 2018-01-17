using Csla.Orleans.Tests.BusinessObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Csla.Orleans.TestLocalHost
{
    class Program
    {
        private static readonly ManualResetEvent _exitEvent = new ManualResetEvent(false);

        //  public static IConfiguration Configuration { get; set; }

        private static ISiloHost SiloHost { get; set; }


        private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddConsole();
        }

        static void Main(string[] args)
        {
            var currentDir = Path.Combine(Environment.CurrentDirectory, "bin/debug/netcoreapp2.0");
            Environment.CurrentDirectory = currentDir;
            Console.WriteLine(Environment.CurrentDirectory);
           
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {

            var builder = new SiloHostBuilder()
                .ConfigureLogging(ConfigureLogging)
                .UseConfiguration(LoadClusterConfiguration())
                .UseServiceProviderFactory(ConfigureServices)
                .ConfigureApplicationParts(ConfigureApplicationParts);


            SiloHost = builder.Build();
            await StartAsync();
            Console.WriteLine("Press Ctrl+C to terminate...");
            Console.CancelKeyPress += (s, e) => _exitEvent.Set();

            _exitEvent.WaitOne();

            Console.WriteLine("Stopping...");
            await SiloHost.StopAsync();
            await SiloHost.Stopped;
            Console.WriteLine("Stopped.");

        }


        private static async Task StartAsync()
        {
            //  Serializers.RegisterAll(_siloHost.Services);
            await SiloHost.StartAsync();
        }

        private static void ConfigureApplicationParts(IApplicationPartManager partManger)
        {
            partManger.AddApplicationPart(typeof(IOrleansGrainDataPortalServer).Assembly);
            partManger.AddApplicationPart(typeof(Program).Assembly);
            partManger.AddApplicationPart(typeof(Root).Assembly);
            partManger.AddApplicationPart(typeof(Csla.ApplicationContext).Assembly);
        }

        private static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddLogging();


            return services.BuildServiceProvider();

        }

        private static ClusterConfiguration LoadClusterConfiguration()
        {

            // var conString = Configuration["SqlServerConnextionString"];
            var cluster = new ClusterConfiguration();
            //cluster.StandardLoad();
            cluster.LoadFromFile("OrleansConfiguration.dev.xml");
            cluster.Globals.SerializationProviders.Add(typeof(CslaOrleansSerialiser).GetTypeInfo());
            //cluster.Globals.AdoInvariant = "System.Data.SqlClient";
            //cluster.Globals.ClusterId = "OrleansTest";

            //cluster.Globals.SeedNodes.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11111));
            //cluster.Globals.LivenessEnabled = true;
            //cluster.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.SqlServer;

            //cluster.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30000);
            //cluster.Defaults.HostNameOrIPAddress = "localhost";
            //cluster.Defaults.Port = 11111;

            //  cluster.Globals.DataConnectionStringForReminders = conString;
            //  cluster.Globals.AdoInvariantForReminders = "System.Data.SqlClient";
           
            //   cluster.AddAdoNetStorageProvider("AdoNetStorage", connectionString: conString, serializationFormat: AdoNetSerializationFormat.Json);
            //  cluster.AddAzureTableStorageProvider("AzureStore", "UseDevelopmentStorage=true");
            return cluster;
        }
      
    }
}
