using Microsoft.Extensions.Logging;
using Quipu.ParameterizationExtractor.Common;
using Quipu.ParameterizationExtractor.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Quipu.ParameterizationExtractor
{
    public class PackageProcessor
    {
        private readonly ISourceSchema _schema;
        private readonly ILogger _log;
       // private readonly ISqlBuilder _sqlBuilder; not threadsafe
        private readonly IFileService _fileService;
        private readonly IAppArgs _args;
        //  private readonly IDependencyBuilder _dependencyBuilder; not threadsafe
        private readonly IServiceProvider _serviceProvider;

        public PackageProcessor(ISourceSchema schema, 
            ILogger<PackageProcessor> log, 
            ISqlBuilder sqlBuilder, 
            IFileService fileService, 
            IAppArgs args,
            IDependencyBuilder dependencyBuilder,
            IServiceProvider serviceProvider)
        {
            Affirm.ArgumentNotNull(schema, "schema");
            Affirm.ArgumentNotNull(log, "log");
            Affirm.ArgumentNotNull(sqlBuilder, "sqlBuilder");
            Affirm.ArgumentNotNull(fileService, "fileService");
            Affirm.ArgumentNotNull(args, "args");
            Affirm.ArgumentNotNull(dependencyBuilder, "dependencyBuilder");
            Affirm.ArgumentNotNull(serviceProvider, "serviceProvider");

            _schema = schema;            
            _fileService = fileService;
            _log = log;
            _args = args;           
            _serviceProvider = serviceProvider;
        }

        public async Task ExecuteAsync(CancellationToken token, IPackage pckg)
        {
            _log.InfoFormat("Starting processing of package...");
            if (!_fileService.DirectoryExists(_args.OutputFolder))
                _fileService.CreateDirectory(_args.OutputFolder);

            var tasks = new List<Task>();

            foreach (var scriptSource in pckg.Scripts)
            {
                tasks.Add(Task.Run(async () =>
                {
                    _log.InfoFormat("Building Dependencies for {0} ...", scriptSource.ScriptName);
                    var pTables = await _serviceProvider.GetService<IDependencyBuilder>()
                                                        .PrepareAsync(token, scriptSource);
                    _log.InfoFormat("Done");

                    _log.InfoFormat("Preparing SQL to save...");
                    var sqlBuilder = _serviceProvider.GetService<ISqlBuilder>();

                    _fileService.Save(sqlBuilder.Build(pTables, _schema, scriptSource), string.Format(".\\{0}\\{1}_p_{2}.sql", _args.OutputFolder, scriptSource.Order.ToString("D3"), scriptSource.ScriptName));
                }));
                              
            }            

            await Task.WhenAll(tasks);

            _log.InfoFormat("Finished processing of package.");
        }
    }
}
