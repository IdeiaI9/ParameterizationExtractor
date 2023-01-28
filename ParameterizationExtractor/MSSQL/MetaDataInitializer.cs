using Quipu.ParameterizationExtractor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quipu.ParameterizationExtractor.Model;
using System.Data;
using System.Composition;

namespace Quipu.ParameterizationExtractor.MSSQL
{
    [Export]
    public class MetaDataInitializer : IMetaDataInitializer
    {
        private readonly Dictionary<string, Type> sqlToNetTypes = new Dictionary<string, Type>();

        public MetaDataInitializer()
        {
            sqlToNetTypes.Add("int", typeof(int));
            sqlToNetTypes.Add("bigint", typeof(long));
            sqlToNetTypes.Add("varchar", typeof(string));
            sqlToNetTypes.Add("char", typeof(string));
            sqlToNetTypes.Add("nvarchar", typeof(string));
            sqlToNetTypes.Add("datetime", typeof(DateTime));
            sqlToNetTypes.Add("bit", typeof(bool));
            sqlToNetTypes.Add("numeric", typeof(float));

            sqlToNetTypes.Add("money", typeof(decimal));
            sqlToNetTypes.Add("smalldatetime", typeof(DateTime));
            sqlToNetTypes.Add("tinyint", typeof(Byte));
            sqlToNetTypes.Add("xml", typeof(string));
        }

        private Type GetNETType(string sqlType)
        {
            if (sqlToNetTypes.ContainsKey(sqlType))
            {
                return sqlToNetTypes[sqlType];
            }
            else
            {
                return typeof(object); 
            }
        }

        public PFieldMetadata InitTableMetaData(DataRow metaData)
        {           
            var result = new PFieldMetadata()
            {
                FieldName = metaData["ColumnName"].ToString(),
                FieldType = GetNETType(metaData["BaseTypeName"].ToString()),
                SqlType = metaData["base_type_name_str"].ToString(),
                IsPK = bool.Parse(metaData["IsInPK"].ToString()),
                IsIdentity = bool.Parse(metaData["IsIdentity"].ToString())
            };

            return result;
        }
    }
}
