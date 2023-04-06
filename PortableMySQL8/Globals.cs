using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PortableMySQL8
{
    public class Globals
    {
        public static string PathMySqlBase = ".\\mysql";
        public static string PathMySqlData = Path.Combine(PathMySqlBase, "data");
        public static string PathMySqlConfig = Path.Combine(PathMySqlBase, "config");
        public static string PathMyIniFile = Path.Combine(PathMySqlConfig, "my.ini");
    }
}
