using YamlDotNet.Serialization;

namespace Antyrama.Tools.Scribe.Core.Models
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