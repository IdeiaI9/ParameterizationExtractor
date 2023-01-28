using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParameterizationExtractor;
using System.Threading;
using Quipu.ParameterizationExtractor.Logic.Interfaces;
using Quipu.ParameterizationExtractor.Common;
using Quipu.ParameterizationExtractor.Logic.Model;
using System.Text.RegularExpressions;
using Quipu.ParameterizationExtractor.Logic.Helpers;
using Microsoft.Extensions.Logging;

namespace Quipu.ParameterizationExtractor.Logic.MSSQL
{
    public class DependencyBuilder : IDependencyBuilder
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISourceSchema _schema;
        private readonly ILogger _log;
        //private readonly ISourceForScript _template;
        private readonly IExtractConfiguration _configuration;
        
        public DependencyBuilder(IUnitOfWorkFactory unitOfWorkFactory, ISourceSchema schema, ILogger<DependencyBuilder> log, IExtractConfiguration configuration)
        {
            Affirm.ArgumentNotNull(unitOfWorkFactory, "unitOfWorkFactory");
            Affirm.ArgumentNotNull(schema, "schema");
            Affirm.ArgumentNotNull(log, "log");
            Affirm.ArgumentNotNull(configuration, "configuration");

            _unitOfWorkFactory = unitOfWorkFactory;
            _schema = schema;
            _log = log;
            _configuration = configuration;
        }

        private HashSet<PRecord> processedTables;

        public async Task<IEnumerable<PRecord>> PrepareAsync(CancellationToken cancellationToken, ISourceForScript template)
        {
            processedTables = new HashSet<PRecord>();
            var queue = new Queue<PRecord>();

            Func<string, string, Task> processTable = async (tableName, where) =>
             {
                 foreach (var rootTable in await GetPTables(tableName, where, cancellationToken, template))
                 {
                     cancellationToken.ThrowIfCancellationRequested();
                     if (rootTable != null)
                     {
                         rootTable.IsStartingPoint = true;
                         queue.Enqueue(rootTable);
                     }
                 }
             };

            foreach (var root in template.RootRecords.OrderBy(_ => _.ProcessingOrder))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (ConfigHelper.IsRegExp(root.TableName))
                {
                    var tables = ConfigHelper.GetTablesByRawName(_schema, root.TableName);

                    foreach (var table in tables)
                        await processTable(table.TableName, root.Where);
                }
                else
                {
                    await processTable(root.TableName, root.Where);
                }
            }

            while (queue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _log.Debug("Start iteration");
                var record = queue.Dequeue();
                _log.DebugFormat("{0} is processing now. {1}", record.TableName, record.Source);
                if (!processedTables.Any(_ => _.Equals(record)))
                {
                    processedTables.Add(record);

                    foreach (var item in await GetRelatedTables(record, cancellationToken, template))
                    {
                        queue.Enqueue(item);
                    }

                }
                _log.Debug("End iteration");
            }

            return processedTables;
        }

        private ExtractStrategy GetExtractStrategy(string tableName, ISourceForScript template)
        {
            //var fromTemplate = template.TablesToProcess.FirstOrDefault(_ => _.TableName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))?.ExtractStrategy;

            var t = ConfigHelper.GetTableToExtract(tableName, template)?.ExtractStrategy;

            return t ?? _configuration.DefaultExtractStrategy;
        }

        private SqlBuildStrategy GetSqlBuildStrategy(string tableName, ISourceForScript template)
        {
            //var fromTemplate = template.TablesToProcess.FirstOrDefault(_ => _.TableName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))?.SqlBuildStrategy;

            return ConfigHelper.GetTableToExtract(tableName, template)?.SqlBuildStrategy
                        ?? _configuration.DefaultSqlBuildStrategy;
        }


        private async Task<IEnumerable<PRecord>> GetRelatedTables(PRecord table, CancellationToken cancellationToken, ISourceForScript template)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = new List<PRecord>();

            var tables = _schema.DependentTables.Where(_ => _.ParentTable.Equals(table.TableName, StringComparison.InvariantCultureIgnoreCase))
                                    .Union(_schema.DependentTables.Where(_ => _.ReferencedTable.Equals(table.TableName, StringComparison.InvariantCultureIgnoreCase)));

            foreach (var item in tables)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _log.DebugFormat("parent table: {0} referenced table {1}", item.ParentTable, item.ReferencedTable);
                var extractStrategy = GetExtractStrategy(table.TableName, template);

                Func<string, string, string, Task<IEnumerable<PRecord>>> insertTable = async (tableName, columnName, pkColumn) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    //if (!template.TablesToProcess.Any(_ => _.TableName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
                    //    return await Task.FromResult<IEnumerable<PRecord>>(null);

                    if (ConfigHelper.GetTableToExtract(tableName,template) == null)
                        return await Task.FromResult<IEnumerable<PRecord>>(null);

                    if (extractStrategy.DependencyToExclude.Any(_ => _.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)))
                        return await Task.FromResult<IEnumerable<PRecord>>(null);

                    var value = table.FirstOrDefault(_ => _.FieldName.Equals(columnName, StringComparison.InvariantCultureIgnoreCase))?.ValueToSqlString();
                    if (value != null)
                    {
                        var str = string.Format("{0} = {1}", pkColumn, value);

                        var tableExtractStrategy = GetExtractStrategy(tableName, template); // extract strategy of the child\parent table

                        if (!string.IsNullOrEmpty(tableExtractStrategy.Where))
                            str = $"{str} AND {tableExtractStrategy.Where}";

                        return await GetPTables(tableName, str, cancellationToken, template);
                    }

                    return await Task.FromResult<IEnumerable<PRecord>>(null);
                };

                if (item.ParentTable.Equals(table.TableName, StringComparison.InvariantCultureIgnoreCase)
                    && extractStrategy.ProcessParents)
                {
                    var i = await insertTable(item.ReferencedTable, item.ParentColumn, item.ReferencedColumn);
                    if (i != null)
                    {
                        foreach (var parent in i)
                        {
                            table.Parents.Add(new PTableDependency() { PRecord = parent, FK = item });
                            result.Add(parent);
                        }
                    }
                }
                if (item.ReferencedTable.Equals(table.TableName, StringComparison.InvariantCultureIgnoreCase)
                    && extractStrategy.ProcessChildren)
                {
                    var i = await insertTable(item.ParentTable, item.ReferencedColumn, item.ParentColumn);
                    if (i != null)
                    {
                        foreach (var child in i)
                        {
                            table.Childern.Add(new PTableDependency() { PRecord = child, FK = item });
                        }

                        result.AddRange(i);
                    }
                }
            }

            return result;
        }

        public async Task<PRecord> GetPTable(string tableName, string objectId, CancellationToken cancellationToken, ISourceForScript template)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new List<PRecord>();

            var tableMetaData = PrepareTableMetaData(template, tableName);

            var sql = string.Format("select * from {0} where [{1}] = {2}", tableName, tableMetaData.PK.FieldName, objectId);

            _log.DebugFormat("GetPTable : {0}", sql);
            var processed = processedTables.FirstOrDefault(_ => _.TableName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase) && _.PK == objectId);
            if (processed != null)
            {
                _log.DebugFormat("Object ({0}) with id {1} has been found in processedTables", processed.TableName, processed.PK);
                return processed;
            }
            using (var uow = _unitOfWorkFactory.GetUnitOfWork())
            {
                var reader = await uow.ExecuteReaderAsync(sql, cancellationToken);

                while (reader.Read())
                {
                    result.Add(new PRecord(reader, tableMetaData) { Source = sql.Trim(), ExtractStrategy = GetExtractStrategy(tableName, template), SqlBuildStrategy = GetSqlBuildStrategy(tableName, template) });
                }
            }

            return result.FirstOrDefault();
        }

        private PTableMetadata PrepareTableMetaData(ISourceForScript template, string tableName)
        {
            var tabMeta = _schema.GetTableMetaData(tableName);

            //var fromTemplate = template.TablesToProcess.FirstOrDefault(_ => _.TableName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase) && _.UniqueColumns != null && _.UniqueColumns.Any());
            var fromTemplate = ConfigHelper.GetTableToExtract(tableName, template);

            if (fromTemplate != null && fromTemplate.UniqueColumns != null && fromTemplate.UniqueColumns.Any())
                tabMeta.UniqueColumnsCollection = new List<string>(fromTemplate.UniqueColumns);

            return tabMeta;
        }

        public async Task<IEnumerable<PRecord>> GetPTables(string tableName, string where, CancellationToken cancellationToken, ISourceForScript template)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = new List<PRecord>();
            var sql = string.Format("select * from {0} ", tableName);
            if (!string.IsNullOrEmpty(where))
                sql = string.Format("{0} where {1} ", sql, where);

            _log.DebugFormat("GetPTables : {0}", sql);

            using (var uow = _unitOfWorkFactory.GetUnitOfWork())
            {
                var reader = await uow.ExecuteReaderAsync(sql, cancellationToken);

                while (reader.Read())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var record = new PRecord(reader, PrepareTableMetaData(template, tableName)) { Source = sql.Trim(), ExtractStrategy = GetExtractStrategy(tableName, template), SqlBuildStrategy = GetSqlBuildStrategy(tableName, template) };
                    var processed = processedTables.FirstOrDefault(_ => _.Equals(record));
                    result.Add(processed ?? record);
                }
            }        
            return result;
        }        
    }
}
