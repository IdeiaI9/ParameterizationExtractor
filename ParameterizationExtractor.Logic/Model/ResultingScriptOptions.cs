using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ParameterizationExtractor.Logic.Model
{
    [Serializable]
    public class ResultingScriptOptions
    {
        public ResultingScriptOptions()
        {
            TargetDatabase = string.Empty;
        }
        public ResultingScriptOptions(string targetDatabase)
        {
            TargetDatabase = targetDatabase;
        }

        [XmlAttribute()]
        public string TargetDatabase { get; set; }
        [XmlAttribute()]
        public bool Rollback { get; set; } = true;
    }
}
