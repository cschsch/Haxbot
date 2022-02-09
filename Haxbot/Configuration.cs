using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haxbot;

public class Configuration
{
    public string DatabasePath { get; set; }
    public string ConnectionStringTemplate { get; set; }

    public Configuration(string databasePath, string connectionStringTemplate)
    {
        DatabasePath = databasePath;
        ConnectionStringTemplate = connectionStringTemplate;
    }

    public Configuration() : this("", "") { }
}
