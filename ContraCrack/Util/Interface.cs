using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ContraCrack.Util
{
    class Interface
    {
        //I'm planning on putting some more stuff in here
        public static DialogResult getYesNoDialog(string prompt, string title)
        {
            return MessageBox.Show(prompt, title, MessageBoxButtons.YesNoCancel);
        }
        public static string getUserInputDialog(string prompt, string title, string defaultText = "")
        {
            return Microsoft.VisualBasic.Interaction.InputBox(prompt, title, defaultText);
        }
    }
}
