using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContraCrack
{
    class LogHandler
    {
        private string name;
        public LogHandler(string identifier)
        {
            name = identifier;
        }
        public void Log(string input)
        {
            MainForm.Instance.log("[" + Identifier + "] " + input);
        }
        public string Identifier
        {
            get
            {
                return name;
            }
            set
            {
                name = Identifier;
            }
        }
    }
}
