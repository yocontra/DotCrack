namespace ContraCrack.Util
{
    class LogHandler
    {
        private string _name;
        public LogHandler(string identifier)
        {
            _name = identifier;
        }
        public void Log(string input)
        {
            MainForm.Instance.AddToCrackLog("[" + Identifier + "] " + input + "\r\n");
        }
        public string Identifier
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
    }
}
