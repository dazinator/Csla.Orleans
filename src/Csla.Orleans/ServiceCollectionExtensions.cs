using Csla.Core;
using Csla.DataPortalClient;
using Csla.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Csla.Orleans
{
    /// <summary>
    /// Extension methods for ASP.NET Core configuration.
    /// </summary>
    public static class WebConfiguration
    {
        /// <summary>
        /// Configure CSLA .NET options for ASP.NET Core.
        /// </summary>
        /// <param name="services">ASP.NET services</param>
        /// <param name="setupAction">Setup action</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureCsla(this IServiceCollection services, Action<CslaOptions, IServiceProvider> setupAction = null)
        {
            services.AddSingleton<CslaOptions>((sp) =>
            {
                var options = new CslaOptions();
                setupAction?.Invoke(options, sp);
                options.UseOptions();
                return options;
            });

            return services;
        }
              

        /// <summary>
        /// CSLA .NET configuration options.
        /// </summary>
        public class CslaOptions
        {
            /// <summary>
            /// Gets or sets the object factory loader.
            /// </summary>
            public IObjectFactoryLoader ObjectFactoryLoader { get; set; }

            /// <summary>
            /// Gets or sets the web application context manager.
            /// </summary>
            public IContextManager WebContextManager { get; set; }


            public IDataPortalProxyFactory DataPortalProxyFactory { get; set; }

            internal void UseOptions()
            {

                // configure csla according to options.
                Csla.Server.FactoryDataPortal.FactoryLoader = ObjectFactoryLoader;
                Csla.DataPortal.ProxyFactory = DataPortalProxyFactory;
                // Csla.ApplicationContext.WebContextManager = WebContextManager ?? new ApplicationContextManager(appBuilder.ApplicationServices);

            }

        }
    }
}
