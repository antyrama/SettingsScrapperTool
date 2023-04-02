# Settings Scrapper Tool

[![Build Status](https://github.com/antyrama/SettingsScrapperTool/workflows/Build%20and%20test%20each%20commit/badge.svg)](https://github.com/antyrama/SettingsScrapperTool/actions?query=workflow%3ABuild%20and%20test%20each%20commit) 
[![NuGet](https://img.shields.io/nuget/v/Antyrama.Tools.Scribe.Cli.svg)](https://nuget.org/packages/Antyrama.Tools.Scribe.Cli) 
[![Nuget](https://img.shields.io/nuget/dt/Antyrama.Tools.Scribe.Cli.svg)](https://nuget.org/packages/Antyrama.Tools.Scribe.Cli)

A smart CLI tool that automates the tedious task of configuring application services in the cloud.

## What it does?
Once executed, tool will hook up to host of `Microsoft.NET.Sdk.Web` application (NET6&7 only), collect all application settings and update your configuration JSON or YAML file.

## Getting started
Tool can be installed using the `Nuget package manager` or the `dotnet` CLI.

### 1. Create tool manifest
Open you web application project folder in terminal and run the following
``` ps
dotnet new tool-manifest
```

### 2. Install tool
Install `Antyrama.Tools.Scribe.Cli` tool package into your project
``` ps
dotnet tool install Antyrama.Tools.Scribe.Cli
```

### 3. Install core library
Install `Antyrama.Tools.Scribe.Core` package into your project
``` ps
dotnet add package Antyrama.Tools.Scribe.Core
```

### 4. Configure project
For full automation, add below entry in `.csproj` file. Configuration files will be generated/updated after each build.
``` xml
  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="dotnet restore" />
    <Exec Command="dotnet tool restore" />
    <Exec Command="dotnet tool run app-settings-to-file generate --assembly $(OutputPath)\$(AssemblyName).dll " />
  </Target>
```

### 5. Final touches
According to the Microsoft [documentation](https://learn.microsoft.com/en-us/dotnet/core/versions/selection#the-sdk-uses-the-latest-installed-version), the SDK uses the latest version that is installed:
> The .NET CLI must choose an SDK version for every dotnet command. It uses the latest SDK installed on the machine by default, even if the project targets an earlier version of the .NET runtime.

If latest version of your installed SDK is higher than your project target framework, add anywhere in your project hierarchy `global.json` file with below content:
``` json
{
  "sdk": {
    "version": "6.0.0",
    "rollForward": "latestFeature"
  }
}
```

### 6. Build your project
The first time you build your project, configuration file(s) will be created. 
Changes made to your values in `appSettings.json` file, after the next build will not affect previous values in generated file.
New `appSettings.json` entries and changes to the structure will create or remove entries in generated configuration files.

## Advanced configuration
Tool is customisable, take a look at list of all arguments that can be passed. Each time when a list type of give option is mentioned, you separate items using space.

| Short name | Full name | Is optional | Description | Default value |
|:----------:|-----------|:-----------:|-------------|---------------|
| -a | --assembly | :x: | Startup assembly file path and name. Can be obtained by `$(OutputPath)\$(AssemblyName).dll` | |
| -p | --providers | :heavy_check_mark: | A list of configuration providers from which all setting keys are taken. All types derived from `IConfigurationProvider` | `JsonConfigurationProvider` |
| -i | --include | :heavy_check_mark: | A list of keys to include despite of providers list configuration. Example: `PROCESSOR_ARCHITECTURE` | |
| -x | --exclude | :heavy_check_mark: | A list of keys to exclude from all collected settings. Example: `Logging:LogLevel:Microsoft.AspNetCore` | |
| -s | --separator | :heavy_check_mark: | Setting nesting separator | `__` (double underscore) |
| -v | &#x2011;&#x2011;yaml&#x2011;variable&#x2011;name | :heavy_check_mark: | YAML variable name | `app_config` |
| -y | --to-yaml | :heavy_check_mark: | Indicates whether configuration wrapped in YAML Azure DevOps variables file | `false` |
| -f | --file-path-template | :heavy_check_mark: | File name template for output. Template may contain a placeholder for environment name. Example:                     `configuration.{0}.json` | `./configuration.json` |
| -e | --environments | :heavy_check_mark: | A list of environment names. Separate configuration file will be created per each environment. Required when file name template contains placeholder | |

### Configuration providers
If you'd like to include all the settings collected by different configuration provider, add them all by `--providers` option. More information about [configuration providers](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers).

### YAML v.s. JSON
You may want to keep you your service configuration in JSON and then load it in your pipeline to configure service. If you use option `--to-yaml`, then JSON content will be wrapped in YAML, issuing a variable with your JSON, like on the below example.
``` yaml
# File: configuration.yaml
variables:
  app_config: |-
    [
      {"name": "AllowedHosts", "value": "*", "slotSetting": false},
      {"name": "Logging__LogLevel__Default", "value": "Information", "slotSetting": false},
      {"name": "Logging__LogLevel__Microsoft.AspNetCore", "value": "Warning", "slotSetting": false}
    ]
```
Then you can easily use it in your pipeline
``` yaml
# File: azure-pipeline.yaml
variables:
  - template: configuration.yaml  # Template reference

steps:
  - step ...
```

### Where are my files?
By default, the configuration file(s) are placed in the same directory as your `.csproj`. Depending on the folder structure of your solution you may want to place the files elsewhere. 
To do this, set option `--file-path-template` to desired value. The option allows you to pass relative paths, e.g. `../../configuration.yaml`

### Multiple environment setup
Tool is able to create one configuration file per environment. Set option `--environments` to `dev test prod` and `--file-path-template` to `./configuration.{0}.yaml`, will create three files
``` ps
configuration.dev.yaml 
configuration.prod.yaml
configuration.test.yaml
```
