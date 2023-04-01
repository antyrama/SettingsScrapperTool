using System.Collections.Generic;
using CommandLine;

namespace Antyrama.Tools.Scribe.Core;

[Verb("generate", HelpText = "Generates JSON .NET app settings consumable by Azure DevOps step (AzureAppServiceSettings@1) configuring your app service. Configuration could be also wrapped in a YAML file as variables.")]
public class ToolOptions
{
    [Option('a', "assembly", Required = true, HelpText = "Startup assembly file path and name. Can be obtained by $(OutputPath)\\$(AssemblyName).dll")]
    public string Assembly { get; set; }

    [Option('p', "providers", Required = false, Default = new[] { "JsonConfigurationProvider" },
        HelpText = "A list of configuration providers from which all setting keys are taken. All types derived from 'IConfigurationProvider'")]
    public IEnumerable<string> Providers { get; set; }

    [Option('i', "include", Required = false, Default = new string[0],
        HelpText = "A list of keys to include despite of providers list configuration. Example: Logging:LogLevel:Microsoft.AspNetCore")]
    public IEnumerable<string> IncludeKeys { get; set; }

    [Option('x', "exclude", Required = false, Default = new string[0],
        HelpText = "A list of keys to exclude from all collected settings. Example: Logging:LogLevel:Microsoft.AspNetCore")]
    public IEnumerable<string> ExcludeKeys { get; set; }

    [Option('s', "separator", Required = false, Default = "__",
        HelpText = "Setting nesting separator")]
    public string PathSeparator { get; set; }

    [Option('v', "yaml-variable-name", Required = false, Default = "app_config", HelpText = "YAML variable name")]
    public string YamlVariableName { get; set; }

    [Option('y', "to-yaml", Required = false, Default = false, HelpText = "Indicates whether configuration wrapped in YAML Azure DevOps variables file")]
    public bool WrapInYaml { get; set; }

    [Option('f', "file-path-template", Default = "./configuration.json", Required = false,
        HelpText = "File name template for output. Template may contain a placeholder for environment name. Example: configuration.{0}.json")]
    public string FilePathTemplate { get; set; }

    [Option('e', "environments", Required = false, Default = new string[0],
        HelpText = "A list of environment names. Separate configuration file will be created per each environment. Required when file name template contains placeholder.")]
    public IEnumerable<string> EnvironmentsList { get; set; }
}

[Verb("_generate", Hidden = true)]
public class ToolInternalOptions
{
    [Option('a', "assembly", Required = true, HelpText = "Startup assembly file path and name. Can be obtained by $(OutputPath)\\$(AssemblyName).dll")]
    public string Assembly { get; set; }

    [Option('p', "providers", Required = false, Default = new[] { "JsonConfigurationProvider" },
        HelpText = "A list of configuration providers from which all setting keys are taken. All types derived from 'IConfigurationProvider'")]
    public IEnumerable<string> Providers { get; set; }

    [Option('i', "include", Required = false, Default = new string[0],
        HelpText = "A list of keys to include despite of providers list configuration. Example: Logging:LogLevel:Microsoft.AspNetCore")]
    public IEnumerable<string> IncludeKeys { get; set; }

    [Option('x', "exclude", Required = false, Default = new string[0],
        HelpText = "A list of keys to exclude from all collected settings. Example: Logging:LogLevel:Microsoft.AspNetCore")]
    public IEnumerable<string> ExcludeKeys { get; set; }

    [Option('s', "separator", Required = false, Default = "__",
        HelpText = "Setting nesting separator")]
    public string PathSeparator { get; set; }

    [Option('v', "yaml-variable-name", Required = false, Default = "app_config", HelpText = "YAML variable name")]
    public string YamlVariableName { get; set; }

    [Option('y', "to-yaml", Required = false, Default = false, HelpText = "Indicates whether configuration wrapped in YAML Azure DevOps variables file")]
    public bool WrapInYaml { get; set; }

    [Option('f', "file-path-template", Default = "./configuration.json", Required = false,
        HelpText = "File name template for output. Template may contain a placeholder for environment name. Example: configuration.{0}.json")]
    public string FilePathTemplate { get; set; }

    [Option('e', "environments", Required = false, Default = new string[0],
        HelpText = "A list of environment names. Separate configuration file will be created per each environment. Required when file name template contains placeholder.")]
    public IEnumerable<string> Environments { get; set; }
}
