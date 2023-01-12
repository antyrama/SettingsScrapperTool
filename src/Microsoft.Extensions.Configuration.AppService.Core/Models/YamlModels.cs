using YamlDotNet.Serialization;

namespace Microsoft.Extensions.Configuration.AppService.Core.Models
{
    internal class Root
    {
        [YamlMember(Alias = "variables")]
        public Variables Variables { get; set; }
    }

    internal class Variables
    {
        public string AppConfig { get; set; }
    }
}