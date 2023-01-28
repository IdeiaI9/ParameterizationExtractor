using Quipu.ParameterizationExtractor.Common;
using Quipu.ParameterizationExtractor.Interfaces;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor.MSSQL
{
    [Export(typeof(IUnitOfWorkFactory))]
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        public IUnitOfWork GetUnitOfWork(string source)
        {
            Affirm.NotNullOrEmpty(source, "source");

            return new UnitOfWork(source);
        }

        public IUnitOfWork GetUnitOfWork()
        {
            var connection = ConfigurationManager.ConnectionStrings["SourceDB"]?.ConnectionString;

            Affirm.NotNullOrEmpty(connection, "Connection string can not be null or empty!");

            return GetUnitOfWork(connection);
        }
    }
}
