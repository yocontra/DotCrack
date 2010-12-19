using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace ContraCrack
{
    interface ITransformer
    {
        bool Flag { get; set; }
        bool Changed { get; set; }
        void Load();
        void Transform();
        void Save();
    }
}
