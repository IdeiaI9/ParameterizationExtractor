using Quipu.ParameterizationExtractor.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Quipu.ParameterizationExtractor.Logic.Configs
{
    public class Package : IPackage, IAmDSLFriendly
    {
        public Package()
        {
            Scripts = new List<SourceForScript>();
        }
        public List<SourceForScript> Scripts { get; set; }
        [XmlIgnore]
        IList<ISourceForScript> IPackage.Scripts
        {
            get
            {
                return Scripts.Select(_ => _ as ISourceForScript).ToList();
            }
        }

        public string AsString()
        {
            var retString = new StringBuilder();

            foreach(IAmDSLFriendly s in Scripts.Where(_=> _ is IAmDSLFriendly))
            {
                retString.AppendLine(s.AsString());
            }
            return retString.ToString();
        }
    }
}
