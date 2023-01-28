using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor.Common
{
    [Flags]
    public enum ExitCode : int
    {
        Success = 0,
        Fail = -1
    }

    public interface IApp
    {
        Task<ExitCode> Run(CancellationToken cancellationToken);

        IConfiguration Configuration { get; }
        IServiceProvider ServiceProvider { get; }
    }

    public interface IAppBuilder
    {
        IApp Build();
        IAppBuilder ConfigureServices(Action<IServiceCollection> configureServices);
        IAppBuilder ConfigureServices(Action<IServiceCollection,IAppArgs> configureServices);
        IAppBuilder ConfigureAppConfiguration(Action<IConfigurationBuilder,IAppArgs> configureDelegate);
        IAppBuilder ConfigureLogging(Action<IServiceCollection, IConfiguration> configureDelegate);
        IAppBuilder ConfigureLogging(Action<IServiceCollection, IAppArgs> configureDelegate);
        IAppBuilder ConfigureLogging(Action<ILoggingBuilder> configureDelegate);
        IAppBuilder ConfigureLogging(Action<IServiceCollection, IAppArgs, IConfiguration> configureDelegate);        
    }
}
