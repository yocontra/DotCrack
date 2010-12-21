using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContraCrack.Util
{
    static class Misc
    {
        public static string GetNewFileName(this string originalFile)
        {
            return originalFile.Replace(".exe", "-new.exe");
        }
    }
}
