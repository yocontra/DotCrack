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
        void load();
        void transform();
        void save();
    }
}
