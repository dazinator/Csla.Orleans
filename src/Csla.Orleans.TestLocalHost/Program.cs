using Csla.Orleans.Tests.BusinessObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.CodeGeneration;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;


//[assembly:KnownAssembly("Csla")]
//[assembly: KnownAssembly("Csla.Orleans.Tests.BusinessObjects")]
//[assembly: Orleans.CodeGeneration.KnownType(typeof(Root))]


namespace Csla.Orleans.TestLocalHost
{

    class Program
    {
        private static readonly ManualResetEvent _exitEvent = new ManualResetEvent(false);

        //  public static IConfiguration Configuration { get; set; }

        private static ISiloHost SiloHost { get; set; }


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

        public static Task Main(string[] args)
        {

            //var currentDir = Path.Combine(Environment.CurrentDirectory, "bin/debug/netcoreapp3.1");
            //Environment.CurrentDirectory = currentDir;
            //Console.WriteLine(Environment.CurrentDirectory);

            return new HostBuilder()
                .ConfigureHostConfiguration(c => c.AddEnvironmentVariables(prefix: "ASPNETCORE_"))
                //.ConfigureAppConfiguration(c=>c.AddEnvironmentVariables())
                .UseOrleans((ctx, builder) =>
                {
                    bool isDev = ctx.HostingEnvironment.IsDevelopment();

                    if (isDev)
                    {
                        builder.UseLocalhostClustering(1111, 30000)
                               .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback);
                    }
                    else
                    {
                        // todo production config:
                        builder.UseLocalhostClustering(1111, 30000)
                               .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback);

                    }

                    builder.Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = OrleansClusterInfo.ClusterId.Value;
                            options.ServiceId = OrleansClusterInfo.ServiceId;
                        })
                        .ConfigureApplicationParts(ConfigureApplicationParts)
                        .ConfigureLogging(ConfigureLogging)
                        .Configure<SerializationProviderOptions>(a => a.SerializationProviders.Add(typeof(CslaOrleansSerialiser).GetTypeInfo()));
                    // .ConfigureServices(ConfigureServices)

                    // .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
                    //.AddMemoryGrainStorage(name: "ArchiveStorage")
                    //.AddAzureBlobGrainStorage(
                    //    name: "profileStore",
                    //    configureOptions: options =>
                    //    {
                    //        // Use JSON for serializing the state in storage
                    //        options.UseJson = true;

                    //        // Configure the storage connection key
                    //        options.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=data1;AccountKey=SOMETHING1";
                    //    });
                })
                .ConfigureServices(services =>
                {
                    services.Configure<ConsoleLifetimeOptions>(options =>
                    {
                        options.SuppressStatusMessages = true;
                    });

                    ConfigureServices(services);
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                })

                .RunConsoleAsync();
        }

        //static async Task MainAsync(string[] args)
        //{




        //    var builder = new SiloHostBuilder()
        //        .ConfigureLogging(ConfigureLogging);





        //       .Configure<ClusterOptions>(options =>
        //       {
        //           // https://github.com/dotnet/orleans/issues/5696
        //           options.ClusterId = "0.0.1";
        //           options.ServiceId = serviceId;
        //       })
        //       //.


        //        // .UseConfiguration(LoadClusterConfiguration)
        //        .UseServiceProviderFactory(ConfigureServices)



        //    SiloHost = builder.Build();
        //    await StartAsync();
        //    //Console.WriteLine("Press Ctrl+C to terminate...");
        //    //Console.CancelKeyPress += (s, e) => _exitEvent.Set();

        //    //_exitEvent.WaitOne();

        //    //Console.WriteLine("Stopping...");
        //    //await SiloHost.StopAsync();
        //    //await SiloHost.Stopped;
        //    //Console.WriteLine("Stopped.");

        //}

        //private static ClusterConfiguration LoadClusterConfiguration()
        //{

        //    // var conString = Configuration["SqlServerConnextionString"];
        //    var cluster = new ClusterConfiguration();
        //    //cluster.StandardLoad();
        //    cluster.LoadFromFile("OrleansConfiguration.dev.xml");
        //    // cluster.Globals.SerializationProviders.Add(typeof(CslaOrleansSerialiser).GetTypeInfo());

        //    return cluster;


        //    //cluster.Globals.AdoInvariant = "System.Data.SqlClient";
        //    //cluster.Globals.ClusterId = "OrleansTest";

        //    //cluster.Globals.SeedNodes.Add(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11111));
        //    //cluster.Globals.LivenessEnabled = true;
        //    //cluster.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.SqlServer;

        //    //cluster.Defaults.ProxyGatewayEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30000);
        //    //cluster.Defaults.HostNameOrIPAddress = "localhost";
        //    //cluster.Defaults.Port = 11111;

        //    //  cluster.Globals.DataConnectionStringForReminders = conString;
        //    //  cluster.Globals.AdoInvariantForReminders = "System.Data.SqlClient";

        //    //   cluster.AddAdoNetStorageProvider("AdoNetStorage", connectionString: conString, serializationFormat: AdoNetSerializationFormat.Json);
        //    //  cluster.AddAzureTableStorageProvider("AzureStore", "UseDevelopmentStorage=true");
        //}

        //private static async Task StartAsync()
        //{
        //    //  Serializers.RegisterAll(_siloHost.Services);
        //    await SiloHost.StartAsync();
        //}

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


    }
}
