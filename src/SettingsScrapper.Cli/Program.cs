using CommandLine;
using CommandLine.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SettingsScrapper.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace SettingsScrapper.Cli
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            var parser = Parser.Default;
            var result = parser.ParseArguments<ToolOptions, ToolInternalOptions>(args);

            try
            {
                result
                    .WithParsed<ToolOptions>(options => Generate(parser, options))
                    .WithParsed<ToolInternalOptions>(InnerGenerate)
                    .WithNotParsed(errors => DisplayHelp(result, errors));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return -1;
            }

            return 0;
        }

        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var helpText = errs.IsVersion()
                ? HelpText.AutoBuild(result)
                : HelpText.AutoBuild(result, h => HelpText.DefaultParsingErrorsHandler(result, h), e => e);

            Console.WriteLine(helpText);
        }

        private static void InnerGenerate(ToolInternalOptions options)
        {
            Console.WriteLine($"Assembly to load: {options.Assembly}");

            var startupAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(
                Path.Combine(Directory.GetCurrentDirectory(), options.Assembly));

            var serviceProvider = GetServiceProvider(startupAssembly);

            var generator = new AppServiceConfigurationGenerator(serviceProvider, options);

            generator.Generate();
        }

        private static void Generate(Parser parser, ToolOptions options)
        {
            if (!File.Exists(options.Assembly))
            {
                throw new FileNotFoundException(options.Assembly);
            }

            var depsFile = options.Assembly.Replace(".dll", ".deps.json");
            var runtimeConfig = options.Assembly.Replace(".dll", ".runtimeconfig.json");

            var commandLine = string.Format(
                            "exec --depsfile {0} --runtimeconfig {1} {2} _{3}",
                            EscapePath(depsFile),
                            EscapePath(runtimeConfig),
                            EscapePath(typeof(Program).GetTypeInfo().Assembly.Location),
                            string.Join(' ', parser.FormatCommandLineArgs(options))
                        );

            Console.WriteLine($"Executing: {commandLine}");

            var process = Process.Start("dotnet", commandLine);

            process?.WaitForExit();

            if (process?.ExitCode < 0)
            {
                throw new InvalidOperationException(
                    $"Failed to generate configuration file with exit code:{process.ExitCode}");
            }
        }

        private static IServiceProvider GetServiceProvider(Assembly startupAssembly)
        {
            try
            {
                return WebHost.CreateDefaultBuilder()
                   .UseStartup(startupAssembly.GetName().Name)
                   .Build()
                   .Services;
            }
            catch
            {
                var serviceProvider = HostingApplication.GetServiceProvider(startupAssembly);

                if (serviceProvider != null)
                {
                    return serviceProvider;
                }

                throw;
            }
        }

        private static string EscapePath(string path)
        {
            return path.Contains(' ')
                ? "\"" + path + "\""
                : path;
        }
    }
}