using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ContraCrack
{
    public partial class MainForm : Form
    {
        string task = "";
        public MainForm()
        {
            InitializeComponent();
        }
        private void fileSelectButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter ="EXE files (*.exe)|*.exe|DLL files (*.dll)|*.dll";
            dialog.InitialDirectory = "C:/";
            dialog.Title = "Select an Assembly";
            this.fileSelectTextBox.Text = (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : "";
        }
        public void log(string value)
        {
            value += Environment.NewLine;
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(log), new object[] { value });
                return;
            }
            crackLogTextBox.Text += value;
        }
        private void crackButton_Click(object sender, EventArgs e)
        {
            task = taskComboBox.GetItemText(taskComboBox.SelectedItem);
            crackWorker.RunWorkerAsync();
            this.taskComboBox.Enabled = false;
            this.crackButton.Enabled = false;
            this.fileSelectButton.Enabled = false;
        }
        private void crackWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.taskComboBox.Enabled = true;
            this.crackButton.Enabled = true;
            this.fileSelectButton.Enabled = true;
        }
        private void crackWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (fileSelectTextBox.Text != "")
            {
                crackLogTextBox.Text = "";
                Transformer trans;
                switch (task)
                {

                    case "ExploitN Cracker":
                        trans = new Transformers.ENCracker(fileSelectTextBox.Text);
                        break;

                    case "BottingNation Cracker":
                        trans = new Transformers.BNCracker(fileSelectTextBox.Text);
                        break;

                    case "RSCBTagger":
                        trans = new Transformers.RSCBTagger(fileSelectTextBox.Text);
                        break;

                    default:
                        MessageBox.Show("No task selected, please select one!");
                        return;
                }
                trans.load();
                trans.transform();
                trans.save();
                log("Operation Completed!");
            }
            else
            {
                MessageBox.Show("Please select an assembly first!");
            }
        }
    }
}
