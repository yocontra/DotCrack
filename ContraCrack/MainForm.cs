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

        #region Singleton
        static MainForm _Instance = null;
        static readonly object PadLock = new object();

        public static MainForm Instance
        {
            get
            {
                lock (PadLock)
                {
                    if (_Instance == null)
                    {
                        _Instance = new MainForm();
                    }
                    return _Instance;
                }
            }
        }
        #endregion

        private void fileSelectButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter ="EXE files (*.exe)|*.exe|DLL files (*.dll)|*.dll";
            dialog.InitialDirectory = "C:/";
            dialog.Title = "Select an Assembly";
            MainForm.Instance.fileSelectTextBox.Text = (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : "";
        }
        public void log(string value)
        {
            value += "\r\n";
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(log), new object[] { value });
                return;
            }
            MainForm.Instance.crackLogTextBox.Text += value;
        }
        private void crackButton_Click(object sender, EventArgs e)
        {
            MainForm.Instance.task = MainForm.Instance.taskComboBox.GetItemText(MainForm.Instance.taskComboBox.SelectedItem);
            MainForm.Instance.crackWorker.RunWorkerAsync();
            MainForm.Instance.taskComboBox.Enabled = false;
            MainForm.Instance.crackButton.Enabled = false;
            MainForm.Instance.fileSelectButton.Enabled = false;
            MainForm.Instance.crackLogTextBox.Clear();
        }
        private void crackWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            MainForm.Instance.taskComboBox.Enabled = true;
            MainForm.Instance.crackButton.Enabled = true;
            MainForm.Instance.fileSelectButton.Enabled = true;
        }
        private void crackWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //CheckForIllegalCrossThreadCalls = false;
            if (MainForm.Instance.fileSelectTextBox.Text != "")
            {
                
                Transformer trans;
                switch (MainForm.Instance.task)
                {

                    case "ExploitN Cracker":
                        trans = new Transformers.ENCracker(MainForm.Instance.fileSelectTextBox.Text);
                        break;

                    case "BottingNation Cracker":
                        trans = new Transformers.BNCracker(MainForm.Instance.fileSelectTextBox.Text);
                        break;

                    case "RSCBTagger":
                        trans = new Transformers.RSCBTagger(MainForm.Instance.fileSelectTextBox.Text);
                        break;

                    case "TheBotNet Cracker":
                        trans = new Transformers.TBNCracker(MainForm.Instance.fileSelectTextBox.Text);
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
