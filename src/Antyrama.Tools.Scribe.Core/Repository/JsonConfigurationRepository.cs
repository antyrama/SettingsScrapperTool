﻿using System.Collections.Generic;
using System.IO;

namespace Antyrama.Tools.Scribe.Core.Repository;

internal class JsonConfigurationRepository : ConfigurationRepository
{
    public JsonConfigurationRepository(ToolInternalOptions options) : base(options)
    {
    }

    public override IReadOnlyDictionary<string, object>[] Load(Stream stream)
    {
        var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        return Deserialize(json);
    }

    public override void Save(Stream stream, IEnumerable<IReadOnlyDictionary<string, object>> settings)
    {
        var writer = new StreamWriter(stream);
        writer.Write(Serialize(settings));
        writer.Flush();
    }
}
