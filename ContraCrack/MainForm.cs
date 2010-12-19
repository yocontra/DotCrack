using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ContraCrack.Util;

namespace ContraCrack
{
    public partial class MainForm : Form
    {
        LogHandler logger = new LogHandler("MainForm");
        string _task = "";
        public MainForm()
        {
            InitializeComponent();
        }

        #region Singleton
        static MainForm _instance;
        static readonly object PadLock = new object();

        public static MainForm Instance
        {
            get
            {
                lock (PadLock)
                {
                    return _instance ?? (_instance = new MainForm());
                }
            }
        }
        #endregion

        private void FileSelectButtonClick(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
                                        {
                                            Filter = "EXE files (*.exe)|*.exe|DLL files (*.dll)|*.dll",
                                            InitialDirectory = "C:/",
                                            Title = "Select an Assembly"
                                        };
            Instance.fileSelectTextBox.Text = (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : "";
        }
        public void AddToCrackLog(string value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddToCrackLog), new object[] { value });
                return;
            }
            Instance.crackLogTextBox.Text += value;
        }
        private void CrackButtonClick(object sender, EventArgs e)
        {
            Instance._task = taskComboBox.GetItemText(Instance.taskComboBox.SelectedItem);
            Instance.taskComboBox.Enabled = false;
            Instance.crackButton.Enabled = false;
            Instance.fileSelectButton.Enabled = false;
            Instance.crackLogTextBox.Clear();
            Instance.crackWorker.RunWorkerAsync();
        }
        private void CrackWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Instance.taskComboBox.Enabled = true;
            Instance.crackButton.Enabled = true;
            Instance.fileSelectButton.Enabled = true;
        }
        private void CrackWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            //CheckForIllegalCrossThreadCalls = false;
            if (Instance.fileSelectTextBox.Text != "")
            {
                ITransformer trans;
                switch (Instance._task)
                {

                    case "ENCracker":
                        trans = new Transformers.ENCracker(Instance.fileSelectTextBox.Text);
                        break;

                    case "BNCracker":
                        trans = new Transformers.BNCracker(Instance.fileSelectTextBox.Text);
                        break;

                    case "RSCBTagger":
                        trans = new Transformers.RSCBTagger(Instance.fileSelectTextBox.Text);
                        break;

                    case "TBNCracker":
                        trans = new Transformers.TBNCracker(Instance.fileSelectTextBox.Text);
                        break;

                    case "StringReplacer":
                        trans = new Transformers.StringReplacer(Instance.fileSelectTextBox.Text);
                        break;

                    default:
                        MessageBox.Show("No task selected, please pick one!");
                        return;
                }
                #region Run transformer/check for flags
                //This is way fucking messy but it cleans up code within the transformers
                trans.Flag = false;
                if (!trans.Flag)
                {
                    trans.Load();
                }
                else
                {
                    logger.Log("Transformer threw flag, aborting!");
                    return;
                }
                if (!trans.Flag)
                {
                    trans.Transform();
                }
                else
                {
                    logger.Log("Transformer threw flag, aborting!");
                    return;
                }
                if (!trans.Flag && trans.Changed)
                {
                    trans.Save();
                }
                else
                {
                    if (trans.Flag)
                    {
                        logger.Log("Transformer threw flag, aborting!");
                        return;
                    }
                    logger.Log("Transformer made no changes! Aborting save...");
                    return;
                }
                logger.Log("Operation Completed!");
                #endregion
            }
            else
            {
                MessageBox.Show("Please select an assembly first!");
            }
        }
    }
}
