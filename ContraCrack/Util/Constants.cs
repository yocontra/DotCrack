using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace ContraCrack.Util
{
    class Constants
    {
        public const string MSILErrorMessage = "Issue reading MSIL. Assembly is obfuscated or corrupt.";
        public const string AssemblyErrorMessage = "Error loading assembly. Assembly is obfuscated, packed, or corrupt";
        public const TargetRuntime DefaultRuntime = TargetRuntime.Net_4_0;
    }
}
