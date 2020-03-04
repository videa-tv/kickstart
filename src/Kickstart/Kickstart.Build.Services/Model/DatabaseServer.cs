using System;
using System.Collections.Generic;
using System.Text;

namespace Kickstart.Build.Services.Model
{
    public class DatabaseServer
    {
        public DbmsType DbmsType { get; set; }

        public ServerLocation Location { get; set; }
        public IEnumerable<Database> Databases { get; set; }
    }
}
