using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace deployaCore.Common
{
    internal class ActionWorker
    {
        public string Action { get; set; }
        public int Progress { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }
    }
}
