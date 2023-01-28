using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor.Common
{
    public interface IExecutor
    {
        Task Execute(CancellationToken token);
    }
}
