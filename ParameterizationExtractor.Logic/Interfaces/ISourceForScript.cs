using Quipu.ParameterizationExtractor.Logic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParameterizationExtractor.Logic.Model;

namespace Quipu.ParameterizationExtractor.Logic.Interfaces
{
    public interface ISourceForScript
    {
        IList<RecordsToExtract> RootRecords { get; }
        IList<TableToExtract> TablesToProcess { get; }
        int Order { get; set; }
        string ScriptName { get; set; }
        string Comments { get; set; }
    }

    public interface IExtractConfiguration
    {
        IList<string> FieldsToExclude { get; }
        IList<UniqueColumnsCollection> UniqueColums { get; }
        ExtractStrategy DefaultExtractStrategy { get; }
        SqlBuildStrategy DefaultSqlBuildStrategy { get; }
        ResultingScriptOptions ResultingScriptOptions { get; }
    }
}
