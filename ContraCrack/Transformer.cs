using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace ContraCrack
{
    interface Transformer
    {
        bool flag { get; set; }
        bool changed { get; set; }
        void load();
        void transform();
        void save();
    }
}
