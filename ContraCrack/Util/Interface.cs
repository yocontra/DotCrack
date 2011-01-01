using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ContraCrack.Util
{
    static class Interface
    {
        //I'm planning on putting some more stuff in here
        public static DialogResult GetYesNoDialog(string prompt, string title)
        {
            return MessageBox.Show(prompt, title, MessageBoxButtons.YesNoCancel);
        }
        public static string GetUserInputDialog(string prompt, string title, string defaultText = "")
        {
            return Microsoft.VisualBasic.Interaction.InputBox(prompt, title, defaultText);
        }
        public static void Vibrate(this Form inputForm)
        {
            //This still shows up funny, I need to fix this.
            for (int i = 20; i > -1; i =- 2)
            {
                for (int j = 1; j < 21; j++)
                {
                    inputForm.Top = inputForm.Top + i;
                    inputForm.Left = inputForm.Left + i;
                    inputForm.Top = inputForm.Top - i;
                    inputForm.Left = inputForm.Left - i;
                }
            }
        }
    }
}
