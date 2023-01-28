using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quipu.ParameterizationExtractor.Common
{
    public interface IAppArgs
    {
        string DBName { get; set; }
        string ServerName { get; set; }
        string PathToPackage { get; set; }
        string ConnectionName { get; set; }
        string OutputFolder { get; set; }
        bool Interactive { get; set; }
    }
}
