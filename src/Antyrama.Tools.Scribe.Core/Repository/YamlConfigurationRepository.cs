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

    public YamlConfigurationRepository(string variableName)
    {
        _serializer = new SerializerBuilder()
            .WithAttributeOverride<Variables>(variables => variables.AppConfig,
                new YamlMemberAttribute { Alias = variableName, ScalarStyle = ScalarStyle.Literal })
            .Build();
        _deserializer = new DeserializerBuilder()
            .WithAttributeOverride<Variables>(variables => variables.AppConfig,
                new YamlMemberAttribute { Alias = variableName, ScalarStyle = ScalarStyle.Literal })
            .Build();
    }

    public override IReadOnlyDictionary<string, object>[] Load(Stream stream)
    {
        try
        {
            var reader = new StreamReader(stream);

            var variablesRoot = _deserializer.Deserialize<Root>(reader);

            if (variablesRoot?.Variables == null || string.IsNullOrWhiteSpace(variablesRoot.Variables.AppConfig))
            {
                return Array.Empty<IReadOnlyDictionary<string, object>>();
            }

            return Deserialize(variablesRoot.Variables.AppConfig);
        }
        catch (FileNotFoundException)
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

        var writer = new StreamWriter(stream);
        _serializer.Serialize(writer, root);
        writer.Flush();
    }
}