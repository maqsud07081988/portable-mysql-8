using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableMySQL8
{
    public class Version
    {
        public const string NAME = "Portable MySQL 8";

        public const int MAJOR   = 0;
        public const int MINOR   = 0;
        public const int RELEASE = 0;
        public const int BUILD   = 0;

        public static string VersionPretty
        {
            get { return $"v{MAJOR}.{MINOR}.{RELEASE}.{BUILD}"; }
        }
    }
}
