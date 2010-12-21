using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ContraCrack.Util;
using Mono.Cecil;

namespace ContraCrack
{
    public partial class MainForm : Form
    {
        readonly LogHandler _mainLogger = new LogHandler("MainThread");
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
            if (!InvokeRequired)
            {
                Instance.crackLogTextBox.Text += value;
            }
            else
            {
                Invoke(new Action<string>(AddToCrackLog), new object[] {value});
                return;
            }
        }
        private void CrackButtonClick(object sender, EventArgs e)
        {
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
            if (Instance.fileSelectTextBox.Text == "")
            {
                MessageBox.Show("Please select an assembly first!");
            }
            else
            {
                ITransformer trans;
                switch (Instance.taskComboBox.GetItemText(Instance.taskComboBox.SelectedItem))
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
                trans.HasIssue = false;
                if (trans.HasIssue)
                {
                    _mainLogger.Log("Transformer has a problem, aborting!");
                    return;
                }
                _mainLogger.Log("Loading Assembly from " + trans.OriginalLocation);
                trans.Load();
                if (trans.HasIssue)
                {
                    _mainLogger.Log("Transformer has a problem, aborting!");
                    return;
                }
                _mainLogger.Log("Running Transformer...");
                trans.Transform();
                //This needs to be fixed to distinguish if workingassembly is different than original. moduledefinition.image maybe?
                if (trans.HasIssue)
                {
                    if (trans.HasIssue)
                    {
                        _mainLogger.Log("Transformer has a problem, aborting!");
                        return;
                    }
                    _mainLogger.Log("Transformer made no changes! Aborting save!");
                    return;
                }
                _mainLogger.Log("Saving Assembly to " + trans.NewLocation);
                trans.Save();
                _mainLogger.Log("Operation Completed!");

                #endregion
            }
        }
    }
}
