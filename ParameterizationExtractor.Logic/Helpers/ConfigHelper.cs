using Quipu.ParameterizationExtractor.Common;
using Quipu.ParameterizationExtractor.Logic.Interfaces;
using Quipu.ParameterizationExtractor.Logic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor.Logic.Helpers
{
    public static class ConfigHelper
    {
        private const string regExp = "RegExp:";

        public static bool IsRegExp(string s)
        {
            return s.StartsWith(regExp);
        }

        public static string ExtractPattern(string s)
        {
            return s.Substring(s.IndexOf(':') + 1);
        }

        public static IList<PTableMetadata> GetTablesByRawName(ISourceSchema schema, string name)
        {
            Affirm.ArgumentNotNull(schema, nameof(schema));

            return GetTablesByPattern(schema, ExtractPattern(name));
        }

        public static IList<PTableMetadata> GetTablesByPattern(ISourceSchema schema, string pattern)
        {
            Affirm.ArgumentNotNull(schema, nameof(schema));

            return schema.Tables.Where(_ => Regex.IsMatch(_.TableName, pattern)).ToList();
        }

        public static Func<TableToExtract, string, bool> PredicateByName = (t, tn) => t.TableName.Equals(tn, StringComparison.InvariantCultureIgnoreCase);
        public static Func<TableToExtract, string, bool> PredicateByRegExp = (t, tn) => Regex.IsMatch(tn, ExtractPattern(t.TableName));
        public static Func<TableToExtract, string, bool> GetPredicateForTable(string tableName)
        {
            return IsRegExp(tableName) ? PredicateByRegExp : PredicateByName;
        }

        public static TableToExtract GetTableToExtract(string tableName, ISourceForScript template)
        {
            var directName = template.TablesToProcess.Where(_ => !IsRegExp(_.TableName)).FirstOrDefault(_ => PredicateByName(_, tableName));

            if (directName == null)
            {
                var matched = template.TablesToProcess.Where(_ => IsRegExp(_.TableName))?.Where(_ => PredicateByRegExp(_, tableName));

                if (matched.Count() > 1)
                    throw new InvalidOperationException($"For table {tableName} exists more than 1 table to process with matched RegExp");

                directName = matched.FirstOrDefault();
            }

            return directName;


        }
    }
}
