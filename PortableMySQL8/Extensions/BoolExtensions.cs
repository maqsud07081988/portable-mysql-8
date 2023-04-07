using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableMySQL8
{
    public static class BoolExtensions
    {
        public static bool TranslateNullableBool(this bool? _bool)
        {
            return _bool == true ? true : false;
        }
    }
}
