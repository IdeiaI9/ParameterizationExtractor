using Quipu.ParameterizationExtractor.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ParameterizationExtractor.Logic.Model;

namespace Quipu.ParameterizationExtractor.Logic.Model
{
    [Serializable]
    public class GlobalExtractConfiguration : IExtractConfiguration
    {
        public GlobalExtractConfiguration()
        {
            FieldsToExclude = new List<string>();
            UniqueColums = new List<UniqueColumnsCollection>();
            DefaultExtractStrategy = new FKDependencyExtractStrategy();
        }

        public List<string> FieldsToExclude { get; set; }
        [XmlIgnore]
        IList<string> IExtractConfiguration.FieldsToExclude
        {
            get
            {
                return FieldsToExclude;
            }
        }

        public List<UniqueColumnsCollection> UniqueColums { get; set; }
        [XmlIgnore]
        IList<UniqueColumnsCollection> IExtractConfiguration.UniqueColums
        {
            get
            {
                return UniqueColums;
            }
        }
        
        public ExtractStrategy DefaultExtractStrategy { get; set; }
        public SqlBuildStrategy DefaultSqlBuildStrategy { get; set; }
        public ResultingScriptOptions ResultingScriptOptions { get; set; }
    }
}
