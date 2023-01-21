using YamlDotNet.Serialization;

namespace SettingsScrapper.Core.Models
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