//using Quipu.ParameterizationExtractor.Logic.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Quipu.ParameterizationExtractor.Common
//{
//    public class ConsoleLogger : ILog
//    {
//        private string FormatMessage(string message)
//        {
//            return string.Format("{0} - {1}", DateTime.Now, message);
//        }

//        public void Debug(string message)
//        {         
//            Trace.WriteLine(FormatMessage(message));
//            Trace.Flush();
//        }

//        public void DebugFormat(string message, params object[] arg)
//        {
//            Debug(string.Format(message,arg));
//            Trace.Flush();           
//        }

//        public void Error(Exception e)
//        {
//            var s = new StringBuilder();
//            while (e != null)
//            {
//                DebugFormat("Err message {0} Err Stack: {1}", e.Message, e.StackTrace);
//                e = e.InnerException;
//            }
//        }
//    }
//}
