using Quipu.ParameterizationExtractor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quipu.ParameterizationExtractor.Model;
using Quipu.ParameterizationExtractor.Common;
using ParameterizationExtractor;
using System.Threading;
using System.Composition;

namespace Quipu.ParameterizationExtractor.MSSQL
{
    [Export(typeof(IDependencyBuilder))]
    public class DependencyBuilder : IDependencyBuilder
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ISourceSchema _schema;
        private readonly ILog _log;
        //private readonly ISourceForScript _template;
        private readonly IExtractConfiguration _configuration;

        [ImportingConstructor]
        public DependencyBuilder(IUnitOfWorkFactory unitOfWorkFactory, ISourceSchema schema, ILog log, IExtractConfiguration configuration)
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
            var stack = new Stack<PRecord>();

            foreach (var root in template.RootRecords)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var rootTable in await GetPTables(root.TableName, root.Where,cancellationToken,template))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (rootTable != null)
                    {
                        rootTable.IsStartingPoint = true;
                        stack.Push(rootTable);
                    }
                }
            }

            while (stack.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _log.Debug("Start iteration");
                var record = stack.Pop();
                _log.DebugFormat("{0} is processing now. {1}", record.TableName, record.Source);
                if (!processedTables.Any(_ => _.Equals(record)))
                {
                    processedTables.Add(record);

                    foreach (var item in await GetRelatedTables(record, cancellationToken,template))
                    {
                        stack.Push(item);
                    }

                }
                _log.Debug("End iteration");
            }

            return processedTables.Where(_ => _.IsStartingPoint).ToList();
        }

        private ExtractStrategy GetExtractStrategy(string tableName, ISourceForScript template)
        {
            var fromTemplate = template.TablesToProcess.FirstOrDefault(_ => _.TableName == tableName)?.ExtractStrategy;

            return fromTemplate ?? _configuration.DefaultExtractStrategy;
        }

        private SqlBuildStrategy GetSqlBuildStrategy(string tableName, ISourceForScript template)
        {
            var fromTemplate = template.TablesToProcess.FirstOrDefault(_ => _.TableName == tableName)?.SqlBuildStrategy;

            return fromTemplate ?? _configuration.DefaultSqlBuildStrategy;
        }

        private async Task<IEnumerable<PRecord>> GetRelatedTables(PRecord table, CancellationToken cancellationToken, ISourceForScript template)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = new List<PRecord>();

            var tables = _schema.DependentTables.Where(_ => _.ParentTable == table.TableName)
                                    .Union(_schema.DependentTables.Where(_ => _.ReferencedTable == table.TableName));

            foreach (var item in tables)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _log.DebugFormat("parent table: {0} referenced table {1}", item.ParentTable, item.ReferencedTable);
                var extractStrategy = GetExtractStrategy(table.TableName, template);

                Func<string, string, string, Task<IEnumerable<PRecord>>> insertTable = async (tableName, columnName, pkColumn) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!template.TablesToProcess.Any(_ => _.TableName == tableName))
                        return await Task.FromResult<IEnumerable<PRecord>>(null);

                    if (extractStrategy.DependencyToExclude.Any(_ => _ == tableName))
                        return await Task.FromResult<IEnumerable<PRecord>>(null);

                    var value = table.FirstOrDefault(_ => _.FieldName == columnName)?.ValueToSqlString();
                    if (value != null)
                    {
                        var str = string.Format("{0} = {1}", pkColumn, value);
                        return await GetPTables(tableName, str,cancellationToken,template);
                    }

                    return await Task.FromResult<IEnumerable<PRecord>>(null);
                };
               
                if (item.ParentTable == table.TableName
                    && extractStrategy.ProcessParents)
                {
                    var i = (await insertTable(item.ReferencedTable, item.ParentColumn, item.ReferencedColumn))?.FirstOrDefault();
                    if (i != null)
                    {
                        table.Parents.Add(new PTableDependency() { PRecord = i, FK = item });
                        result.Add(i);
                    }
                }
                if (item.ReferencedTable == table.TableName
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

            var tableMetaData = _schema.GetTableMetaData(tableName);

            var sql = string.Format("select * from {0} where [{1}] = {2}", tableName, tableMetaData.PK.FieldName, objectId);

            _log.DebugFormat("GetPTable : {0}", sql);
            var processed = processedTables.FirstOrDefault(_ => _.TableName == tableName && _.PK == objectId);
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
                    result.Add(new PRecord(reader, tableMetaData) { Source = sql.Trim(), ExtractStrategy = GetExtractStrategy(tableName,template), SqlBuildStrategy = GetSqlBuildStrategy(tableName,template) });
                }
            }

            return result.FirstOrDefault();
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

                    var record = new PRecord(reader, _schema.GetTableMetaData(tableName)) { Source = sql.Trim(), ExtractStrategy = GetExtractStrategy(tableName,template) , SqlBuildStrategy = GetSqlBuildStrategy(tableName,template) };
                    var processed = processedTables.FirstOrDefault(_ => _.Equals(record));     
                    result.Add(processed ?? record);
                }
            }

            return result;
        }                
    }
}
