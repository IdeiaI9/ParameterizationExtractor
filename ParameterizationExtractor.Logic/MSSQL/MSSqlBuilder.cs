using Microsoft.Extensions.Logging;
using Quipu.ParameterizationExtractor.Common;
using Quipu.ParameterizationExtractor.Logic.Interfaces;
using Quipu.ParameterizationExtractor.Logic.Model;
using Quipu.ParameterizationExtractor.Logic.Templates;
using System.Collections.Generic;
using System.Linq;
//using Quipu.ParameterizationExtractor.Logic.Templates;

namespace Quipu.ParameterizationExtractor.Logic.MSSQL
{
    public class MSSqlBuilder : ISqlBuilder
    {
        private readonly IExtractConfiguration _config;
        private readonly ILogger _log;
        public MSSqlBuilder(IExtractConfiguration config, ILogger<MSSqlBuilder> log)
        {
            Affirm.ArgumentNotNull(config, nameof(config));
            Affirm.ArgumentNotNull(log, nameof(log));

            _config = config;
            _log = log;
        }
      
        public string Build(IEnumerable<PRecord> tables, ISourceSchema schema, ISourceForScript scriptSource)
        {
            if ((_config.ResultingScriptOptions?.Rollback).GetValueOrDefault())
                _log.LogWarning("Please note that 'rollback' will be placed at the end of generated scripts!");

            var deleterNeeded = scriptSource.TablesToProcess.Any(_ => _.SqlBuildStrategy.DeleteExistingRecords);

            var template = new DefaultTemplate();
            template.Session = new Dictionary<string, object>();
            template.Session.Add("source", tables);
            template.Session.Add("schema", schema);
            template.Session.Add("config", _config);
            template.Session.Add("log", _log);
            template.Session.Add("DeleterNeeded", deleterNeeded);

            return template.TransformText();
        }        
    }
}
