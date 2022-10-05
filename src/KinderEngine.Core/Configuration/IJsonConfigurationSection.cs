using Microsoft.Extensions.Configuration;

namespace KinderEngine.Core.Configuration
{
    public interface IJsonConfigurationSection
    {
        public IConfiguration? RootConfiguration { get; set; }
    }
}
