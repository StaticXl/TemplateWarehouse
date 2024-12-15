using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateWarehouse2
{
    public class UserState
    {
        public string Name { get; set; }
        public bool AwaitingName { get; set; }
        public bool AwaitingEmail { get; set; }
        public bool AwaitingAddress { get; set; }
        public bool awaitingCatString { get; set; }
    }
}
