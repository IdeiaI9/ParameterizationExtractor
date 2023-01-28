using Quipu.ParameterizationExtractor.Logic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor.Logic.Interfaces
{
    public interface ISqlBuilder
    {
        string Build(IEnumerable<PRecord> tables, ISourceSchema schema, ISourceForScript source);      
    }
}
