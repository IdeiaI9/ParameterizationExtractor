using Fclp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ParameterizationExtractor.Logic.MSSQL;
using Quipu.ParameterizationExtractor.Common;
using Quipu.ParameterizationExtractor.Configs;
using Quipu.ParameterizationExtractor.DSL.Connector;
using Quipu.ParameterizationExtractor.Logic.Interfaces;
using Quipu.ParameterizationExtractor.Logic.MSSQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor
{
    public class AppArgs : IAppArgs
    {
        public AppArgs()
        {

        }

        public string DBName { get; set; }
        public string ServerName { get; set; }
        public string PathToPackage { get; set; }
        public string ConnectionName { get; set; }
        public string OutputFolder { get; set; }
        public bool Interactive { get; set; }

        public static IAppArgs GetAppArgs(string[] args)
        {
            var p = new FluentCommandLineParser<AppArgs>();

            p.Setup<string>(_ => _.ConnectionName)
                .As('n', "connectionName")
                .SetDefault("SourceDB");

            p.Setup<string>(_ => _.PathToPackage)
                .As('p', "package")
                .WithDescription("Path to package")
                .Required();

            p.Setup<string>(_ => _.DBName)
                .As('d', "database");

            p.Setup<string>(_ => _.ServerName)
                .As('s', "serverName");

            p.Setup<string>(_ => _.OutputFolder)
                .As('o', "outputFolder")
                .SetDefault("Output");

            p.Setup<bool>(_ => _.Interactive)
                .As('i', "Interactive")
                .SetDefault(false);            

            p.Parse(args);

            if (string.IsNullOrEmpty(p.Object.ServerName) && !string.IsNullOrEmpty(p.Object.DBName))
                throw new Exception("Please specify DBName!");

            if (string.IsNullOrEmpty(p.Object.DBName) && !string.IsNullOrEmpty(p.Object.ServerName))
                throw new Exception("Please specify ServerName!");

            return p.Object;
        }
    }

    public static class AppBootstrap
    {
        public static IAppBuilder CreateAppBuilder(string[] args)
        {
            var a = AppArgs.GetAppArgs(args);
            var ser = new ConfigSerializer(new FparsecConnector());

            return new AppBuilder(a).ConfigureServices(_ =>_
                                                            .AddSingleton<IFileService, FileService>()
                                                            .AddSingleton<IExtractConfiguration>(ser.GetGlobalConfig())
                                                            .AddSingleton<ICanSerializeConfigs, ConfigSerializer>()
                                                            .AddSingleton<PackageProcessor>()
                                                            .AddSingleton<IDSLConnector, FparsecConnector>()
                                                            ); 
        }
    }

    public static class DI
    {

        public static IAppBuilder AddExecutor(this IAppBuilder appBuilder)
        {
            return appBuilder.ConfigureServices((services, args) =>
            {
                if (args.Interactive)
                    services.AddSingleton<IExecutor, DSLExecutor>();
                else
                    services.AddSingleton<IExecutor, FromFileExecutor>();
            });                       
        }

        public static IAppBuilder AddMSSQL(this IAppBuilder appBuilder)
        {
            return appBuilder.ConfigureServices(_ => _.AddTransient<ISqlBuilder, MSSqlBuilder>() //must be transient, MSSqlBuilder is no thread safe
                                                     .AddSingleton<ISourceSchema, MSSQLSourceSchema>()
                                                     .AddSingleton<IObjectMetaDataProvider, ObjectMetaDataProvider>()
                                                     .AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>()
                                                     .AddTransient<IDependencyBuilder, DependencyBuilder>() //must be transient, DependencyBuilder is no thread safe
                                                     .AddTransient<IMetaDataInitializer, MetaDataInitializer>()
                                                     .AddSingleton<IConnectionStringResolver, ConnectionStringResolver>()
                                                );
        }
       
    }
   
}
