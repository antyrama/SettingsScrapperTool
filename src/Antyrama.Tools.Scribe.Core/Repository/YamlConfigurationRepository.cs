using System;
using System.Collections.Generic;
using System.IO;
using Antyrama.Tools.Scribe.Core.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Antyrama.Tools.Scribe.Core.Repository;

internal class YamlConfigurationRepository : ConfigurationRepository
{
    private readonly Serializer _serializer;
    private readonly Deserializer _deserializer;

    public YamlConfigurationRepository(ToolInternalOptions options)
        : base(options)
    {
        _serializer = new SerializerBuilder()
            .WithAttributeOverride<Variables>(variables => variables.AppConfig,
                new YamlMemberAttribute { Alias = options.YamlVariableName, ScalarStyle = ScalarStyle.Literal })
            .Build();
        _deserializer = new DeserializerBuilder()
            .WithAttributeOverride<Variables>(variables => variables.AppConfig,
                new YamlMemberAttribute { Alias = options.YamlVariableName, ScalarStyle = ScalarStyle.Literal })
            .Build();
    }

    public override IReadOnlyDictionary<string, object>[] Load(Stream stream)
    {
        var reader = new StreamReader(stream);

        try
        {
            var variablesRoot = _deserializer.Deserialize<Root>(reader);

            if (variablesRoot?.Variables == null || string.IsNullOrWhiteSpace(variablesRoot.Variables.AppConfig))
            {
                return Array.Empty<IReadOnlyDictionary<string, object>>();
            }

            return Deserialize(variablesRoot.Variables.AppConfig);
        }
        catch (YamlException)
        {
            return Array.Empty<IReadOnlyDictionary<string, object>>();
        }
    }

    public override void Save(Stream stream, IEnumerable<IReadOnlyDictionary<string, object>> settings)
    {
        var root = new Root
        {
            Variables = new Variables
            {
                AppConfig = Serialize(settings)
            }
        };

        var yaml = _serializer.Serialize(root);

        if (Environment.NewLine != Eol)
        {
            yaml = yaml.Replace(Environment.NewLine, Eol);
        }

        var writer = new StreamWriter(stream);
        writer.Write(yaml);
        writer.Flush();
    }
}
