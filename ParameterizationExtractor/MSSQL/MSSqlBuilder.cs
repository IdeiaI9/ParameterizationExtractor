using Quipu.ParameterizationExtractor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quipu.ParameterizationExtractor.Model;
using Quipu.ParameterizationExtractor.Templates;
using System.Composition;

namespace Quipu.ParameterizationExtractor.MSSQL
{
    [Export(typeof(ISqlBuilder))]
    public class MSSqlBuilder : ISqlBuilder
    {
        public string Build(IEnumerable<PRecord> tables, ISourceSchema schema)
        {
            var template = new DefaultTemplate();
            template.Session = new Dictionary<string, object>();
            template.Session.Add("source", tables);
            template.Session.Add("schema", schema);

            return template.TransformText();
        }        
    }
}
