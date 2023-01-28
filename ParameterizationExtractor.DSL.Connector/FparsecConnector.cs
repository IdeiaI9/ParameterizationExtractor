using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quipu.ParameterizationExtractor.Logic.Interfaces;
using Quipu.ParameterizationExtractor.DSL;
using static Quipu.ParameterizationExtractor.DSL.ParserResult;
using ParameterizationExtractor.DSL.Connector;

namespace Quipu.ParameterizationExtractor.DSL.Connector
{
    public class FparsecConnector : IDSLConnector
    {
        public IPackage Parse(string source)
        {
            var result = DSLParser.parse(source);

            if (result is DslOK ok)
                return ok.GetResult;

            throw new DSLParseException((result as Fail).ErrorMessage);
        }
    }
}
