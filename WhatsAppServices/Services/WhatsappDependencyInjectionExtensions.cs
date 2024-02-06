using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsAppServices.Interfaces;
using WhatsAppServices.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WhatsappDependencyInjectionExtensions
    {
        public static IServiceCollection AddWhatsApp<T>(this IServiceCollection services, Action<WhatsAppOptions> options = null)
            where T : IWhatsAppEventExecutor
        {

            services.AddOptions();
            if (options != null)
            {
                services.Configure(options);
            }

            services.TryAddSingleton<IHostedService, WhatsAppHostedService>();
            services.TryAddSingleton<IWhatsAppConnector, WhatsAppConnector>();
            services.TryAddSingleton<IWhatsAppCookies, WhatsAppCookies>();

            services.TryAddSingleton(typeof(IWhatsAppEventExecutor), typeof(T));

            return services;
        }
    }
}
