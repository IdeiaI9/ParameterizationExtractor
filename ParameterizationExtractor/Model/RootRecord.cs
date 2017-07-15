﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor.Model
{
    public class TableToExtract
    {
        public TableToExtract(string tableName, ExtractStrategy extractStrategy) : this(tableName, extractStrategy, new SqlBuildStrategy())
        {

        }

        public TableToExtract(string tableName) : this(tableName,new FKDependencyExtractStrategy(), new SqlBuildStrategy())
        {

        }

        public TableToExtract(string tableName,  ExtractStrategy extractStrategy, SqlBuildStrategy sqlBuildStrategy)
        {
            Affirm.NotNullOrEmpty(tableName, "tableName");
            Affirm.ArgumentNotNull(extractStrategy, "extractStrategy");
            Affirm.ArgumentNotNull(sqlBuildStrategy, "sqlBuildStrategy");

            TableName = tableName;
            ExtractStrategy = extractStrategy;
            SqlBuildStrategy = sqlBuildStrategy;
        }
        public string TableName { get; set; }

        public ExtractStrategy ExtractStrategy { get; private set; }
        public SqlBuildStrategy SqlBuildStrategy { get; private set; }
    }

    public class RecordsToExtract 
    {
        public RecordsToExtract(string tableName, string where)
        {
            Affirm.NotNullOrEmpty(tableName, "tableName");          

            TableName = tableName;
            Where = where;
        }
        public string TableName { get; set; }
        public string Where { get; set; }

    }
}