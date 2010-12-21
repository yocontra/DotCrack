using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContraCrack.Util;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace ContraCrack
{
    interface ITransformer
    {
        AssemblyDefinition OriginalAssembly { get; set; }
        AssemblyDefinition WorkingAssembly { get; set; }
        LogHandler Logger { get; set; }
        bool HasIssue { get; set; }
        string OriginalLocation { get; set; }
        string NewLocation { get; set; }
        void Load();
        void Transform();
        void Save();
    }
}
