using Microsoft.Extensions.Configuration;
using Quipu.ParameterizationExtractor.Common;
using Quipu.ParameterizationExtractor.Logic.Interfaces;
using Quipu.ParameterizationExtractor.Logic.MSSQL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor
{    
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IAppArgs _args;
        private readonly IConnectionStringResolver _connectionStringResolver;
        public UnitOfWorkFactory(IAppArgs args, IConfiguration configuration, IConnectionStringResolver connectionStringResolver)
        {
            Affirm.ArgumentNotNull(args, "args");
            Affirm.ArgumentNotNull(configuration, "configuration");
            Affirm.ArgumentNotNull(connectionStringResolver, "connectionStringResolver");

            _args = args;
            _configuration = configuration;
            _connectionStringResolver = connectionStringResolver;
        }

        public IUnitOfWork GetUnitOfWork(string source)
        {
            Affirm.NotNullOrEmpty(source, "source");

            return new UnitOfWork(source);
        }

        private string GetConnectionString()
        {
            if (string.IsNullOrEmpty(_args.ServerName) || string.IsNullOrEmpty(_args.DBName))
                return null;

            return _connectionStringResolver.GetConnectionString(_args.ServerName, _args.DBName);
        }

        public IUnitOfWork GetUnitOfWork()
        {
            var connection = GetConnectionString() ?? _configuration.GetConnectionString(_args.ConnectionName);

            Affirm.NotNullOrEmpty(connection, "Connection string can not be null or empty!");

            return GetUnitOfWork(connection);
        }
    }
}
