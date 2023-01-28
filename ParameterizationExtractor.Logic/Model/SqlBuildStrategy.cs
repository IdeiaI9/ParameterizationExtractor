using Quipu.ParameterizationExtractor.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Quipu.ParameterizationExtractor.Logic.Model
{
    [Serializable]
    public class SqlBuildStrategy: IAmDSLFriendly
    {
        public SqlBuildStrategy()
        {
            ThrowExecptionIfNotExists = false;
            NoInserts = false;
            AsIsInserts = false;
            IdentityInsert = false;

            FieldsToExclude = new List<string>();
        }
        public SqlBuildStrategy(bool throwExecptionIfNotExists, bool noInserts, bool asIsInserts)
        {
            ThrowExecptionIfNotExists = throwExecptionIfNotExists;
            NoInserts = noInserts;
            AsIsInserts = asIsInserts;
        }

        [XmlAttribute()]
        public bool ThrowExecptionIfNotExists { get; set; }
        [XmlAttribute()]
        public bool NoInserts { get; set; }
        [XmlAttribute()]
        public bool AsIsInserts { get; set; }
        [XmlAttribute()]
        public bool IdentityInsert { get; set; }
        [XmlAttribute()]
        public List<string> FieldsToExclude { get; set; }
        [XmlAttribute()]
        public bool DeleteExistingRecords { get; set; } = false;

        public string AsString()
        {
            if (!ThrowExecptionIfNotExists && !ThrowExecptionIfNotExists && !AsIsInserts)
                return string.Empty;

            var builder = new StringBuilder("build sql");

            if (ThrowExecptionIfNotExists)
                builder.Append(" with throw");
            if (NoInserts)
                builder.Append(" with NoInserts");
            if (AsIsInserts)
                builder.Append(" with asIs");

            return builder.ToString();
        }
    }
}
