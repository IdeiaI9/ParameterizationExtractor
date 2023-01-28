using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quipu.ParameterizationExtractor.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor.Common
{
    public class SqlBuldozerApp : IApp
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SqlBuldozerApp> _logger;
        private readonly IAppArgs _appArgs;
        public SqlBuldozerApp(IConfiguration configuration, 
                              IServiceProvider serviceProvider, 
                              ILogger<SqlBuldozerApp> logger,
                              IAppArgs appArgs)
        {
            Affirm.ArgumentNotNull(configuration, "configuration");
            Affirm.ArgumentNotNull(serviceProvider, "serviceProvider");
            Affirm.ArgumentNotNull(logger, "logger");
            Affirm.ArgumentNotNull(appArgs, "appArgs");

            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _appArgs = appArgs;
        }
        public IConfiguration Configuration => _configuration;

        public IServiceProvider ServiceProvider => _serviceProvider;

        public async Task<ExitCode> Run(CancellationToken cancellationToken)
        {            
            try
            {
                _logger.InfoFormat("Starting...");
                _logger.InfoFormat("Path to package {0}", _appArgs.PathToPackage);

                await _serviceProvider.GetService<ISourceSchema>()
                  .Init(cancellationToken);

                await _serviceProvider.GetService<IExecutor>()
                   .Execute(cancellationToken);

                _logger.InfoFormat("Finished.");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, e.Message);

                return ExitCode.Fail;
            }

            return ExitCode.Success;
        }
    }

    public class AppBuilder : IAppBuilder
    {
        IConfigurationRoot _configuration;
        IServiceCollection _services;
        IAppArgs _appArgs;
        public AppBuilder(IAppArgs appArgs)
        {
            _appArgs = appArgs;
            _services = new ServiceCollection().AddSingleton<IAppArgs>(_appArgs);
        }
      
        public IApp Build()
        {                       
            _services.AddSingleton<IApp, SqlBuldozerApp>();
           
            var serviceProvider = _services.BuildServiceProvider();


            return serviceProvider.GetService<IApp>();
        }

        public IAppBuilder ConfigureAppConfiguration(Action<IConfigurationBuilder, IAppArgs> configureDelegate)
        {
            var cnf = new ConfigurationBuilder();
            configureDelegate?.Invoke(cnf, _appArgs);

            _configuration = cnf.Build();

            _services.AddSingleton<IConfiguration>(_configuration);         

            return this;
        }

        public IAppBuilder ConfigureLogging(Action<IServiceCollection, IConfiguration> configureDelegate)
        {
            if (_configuration == null)
                throw new Exception("Configuration must be configured first!");

            configureDelegate?.Invoke(_services, _configuration);

            return this;
        }

        public IAppBuilder ConfigureLogging(Action<IServiceCollection, IAppArgs> configureDelegate)
        {            
            configureDelegate?.Invoke(_services, _appArgs);
            return this;
        }

        public IAppBuilder ConfigureLogging(Action<ILoggingBuilder> configureDelegate)
        {
            _services.AddLogging(configureDelegate);
            return this;
        }

        public IAppBuilder ConfigureLogging(Action<IServiceCollection,IAppArgs,IConfiguration> configureDelegate)
        {
            if (_configuration == null)
                throw new Exception("Configuration must be configured first!");

            configureDelegate?.Invoke(_services, _appArgs, _configuration);
            return this;
        }

        public IAppBuilder ConfigureServices(Action<IServiceCollection, IAppArgs> configureServices)
        {
            configureServices?.Invoke(_services, _appArgs);

            return this;
        }

        public IAppBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            configureServices?.Invoke(_services);

            return this;
        }
    }
}
