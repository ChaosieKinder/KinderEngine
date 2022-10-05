using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KinderEngine.Core.Configuration
{
    public static class Extensions
    {
        public static IServiceCollection AddConfigurationSection<TSectionType>(this IServiceCollection services, HostBuilderContext context, string section)
            where TSectionType : class
        {
            var configSection = services.AddSingleton<TSectionType>(context.Configuration.GetSection(section).Get<TSectionType>());
            if (configSection is IJsonConfigurationSection jsec)
                jsec.RootConfiguration = context.Configuration;
            return configSection;
        }
    }
}
