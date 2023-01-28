using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quipu.ParameterizationExtractor;
using Quipu.ParameterizationExtractor.Common;
using Serilog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ParameterizationExtractor
{
    //todo add Raiserror clause "row inserted"/"row updated"
    //todo ass optional parameter "TargetDatabase" and add "use TargetDatabase" in each script
    //todo add indent as nest_level*tab to generate scripts
    //todo remove unused char(13)
    class Program
    {
        // very default logger, which will be able to write logs in console before actual logger is configured 
        static Serilog.ILogger Logger => new LoggerConfiguration().WriteTo.Console().CreateLogger(); 
        static IApp app = null;
        static async Task Main(string[] args)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            System.AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                // will try to log in configured logger, if any
                var logger = app?.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger<Program>();

                logger?.LogCritical(e.ExceptionObject.ToString());
            };

            try
            {
                app = BuildApp(args);       
            }
            catch(Exception ex)
            {
                Logger.Fatal(ex, "Fatal error during bootstrapping the app");
                Environment.Exit((int)ExitCode.Fail);
            }

            var res = await app.Run(cts.Token);

            Environment.Exit((int)res);
        }

        static IApp BuildApp(string[] args) =>
            AppBootstrap.CreateAppBuilder(args)
                .ConfigureAppConfiguration((conf,args) => conf.SetBasePath(Directory.GetCurrentDirectory())
                                                              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                                              .AddEnvironmentVariables())
                .ConfigureLogging( (l,c) => l.AddLogging(p=>p.AddSerilog(new LoggerConfiguration()
                                                                             .ReadFrom
                                                                             .Configuration(c)
                                                                             .CreateLogger())
                                                         )
                                 )
                .AddMSSQL()
                .AddExecutor()
                .Build();
    }
}
