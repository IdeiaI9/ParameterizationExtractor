﻿using Quipu.ParameterizationExtractor.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor.Model
{
    [DebuggerDisplay("{PRecord.TableName} {FK.ParentTable}-{FK.ReferencedTable}")]
    public class PTableDependency
    {
        public PRecord PRecord { get; set; }
        public PDependentTable FK { get; set; }
    }

    [DebuggerDisplay("{TableName} {PK}")]
    public class PRecord : List<PField>
    {
        private readonly PTableMetadata _metaData;
        public PRecord(IDataRecord dataRow, PTableMetadata metaData)
        {
            _metaData = metaData;

            foreach (var field in metaData)
            {
                if (dataRow[field.FieldName] != null
                    && !dataRow.IsDBNull(dataRow.GetOrdinal(field.FieldName)))
                    Add(new PField(field) { FieldName = field.FieldName, Value = dataRow[field.FieldName] });
            }

            PkField = (from f in _metaData
                       join v in this
                             on f.FieldName equals v.FieldName
                       where f.IsPK
                       select v
                  ).FirstOrDefault();

            UniqueFields = (from f in this
                            join v in _metaData.UniqueColumnsCollection
                                  on f.FieldName equals v
                            select f).ToList();
    
            _parents = new List<PTableDependency>();
            _children = new List<PTableDependency>();
        }

        private readonly IList<PTableDependency> _parents;
        private readonly IList<PTableDependency> _children;
        public IList<PTableDependency> Childern { get { return _children; } }
        public IList<PTableDependency> Parents { get { return _parents; } }
        public IList<PField> UniqueFields { get; private set; }
        public string PK { get { return PkField?.Value.ToString(); } }
        public PField PkField { get; private set; }
        public bool IsStartingPoint { get; set; }
        public string TableName { get { return _metaData.TableName; } }
        public PTableMetadata MetaData { get { return _metaData; } }
        public string Source { get; set; }
        public override bool Equals(object obj)
        {
            var p = obj as PRecord;
            if (p != null)
                return p.ToString() == this.ToString();

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} {1}",TableName,PK);
        }        

        public string GetUniqueSqlWhere()
        {
            string s = string.Empty;

            var list = new List<PField>();
            if (UniqueFields != null
                && UniqueFields.Any())
                list.AddRange(UniqueFields);
            else if (PkField != null)
                list.Add(PkField);

            if (list.Any())
            {
                return string.Format("where {0}", SqlHelper.GetNameValueString(list));
            }

            return s;
        }

        public string GetPKVarName()
        {
            return string.Format("@{0}_{1}_{2}", TableName, PkField?.FieldName, PK);
        }
    }

    [DebuggerDisplay("{FieldName}")]
    public class PField
    {
        private readonly PFieldMetadata _metaData;
        public PField(PFieldMetadata metaData)
        {
            _metaData = metaData;
        }
        public override string ToString()
        {
            return string.Format("{0}", FieldName);
        }
        public PFieldMetadata MetaData { get { return _metaData; } }
        public string FieldName { get; set; }
        public object Value { get; set; }
        public string Expression { get; set; }
        public string ValueToSqlString()
        {
            if (!string.IsNullOrEmpty(Expression))
                return Expression;

            if (_metaData.FieldType == typeof(string))
            {
                return string.Format("'{0}'", Value.ToString());
            }
            else if (_metaData.FieldType == typeof(bool))
            {
                return Value.ToString() == bool.TrueString ? "1" : "0";
            }
            else if (_metaData.FieldType == typeof(DateTime))
            {
                var date = Convert.ToDateTime(Value);

                return string.Format("'{0}'", date.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
            else
                return Value.ToString();
        }
    }
}
