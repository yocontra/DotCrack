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
        LogHandler logger = new LogHandler("MainForm");
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
        public void addToCrackLog(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(addToCrackLog), new object[] { value });
                return;
            }
            MainForm.Instance.crackLogTextBox.Text += value;
        }
        private void crackButton_Click(object sender, EventArgs e)
        {
            MainForm.Instance.task = taskComboBox.GetItemText(MainForm.Instance.taskComboBox.SelectedItem);
            MainForm.Instance.taskComboBox.Enabled = false;
            MainForm.Instance.crackButton.Enabled = false;
            MainForm.Instance.fileSelectButton.Enabled = false;
            MainForm.Instance.crackLogTextBox.Clear();
            MainForm.Instance.crackWorker.RunWorkerAsync();
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

                    case "ENCracker":
                        trans = new Transformers.ENCracker(MainForm.Instance.fileSelectTextBox.Text);
                        break;

                    case "BNCracker":
                        trans = new Transformers.BNCracker(MainForm.Instance.fileSelectTextBox.Text);
                        break;

                    case "RSCBTagger":
                        trans = new Transformers.RSCBTagger(MainForm.Instance.fileSelectTextBox.Text);
                        break;

                    case "TBNCracker":
                        trans = new Transformers.TBNCracker(MainForm.Instance.fileSelectTextBox.Text);
                        break;

                    case "StringReplacer":
                        trans = new Transformers.StringReplacer(MainForm.Instance.fileSelectTextBox.Text);
                        break;

                    default:
                        MessageBox.Show("No task selected, please pick one!");
                        return;
                }
                #region Run transformer/check for flags
                //This is way fucking messy but it cleans up code within the transformers
                trans.flag = false;
                if (!trans.flag)
                {
                    trans.load();
                }
                else
                {
                    logger.Log("Transformer threw flag, aborting!");
                    return;
                }
                if (!trans.flag)
                {
                    trans.transform();
                }
                else
                {
                    logger.Log("Transformer threw flag, aborting!");
                    return;
                }
                if (!trans.flag && trans.changed)
                {
                    trans.save();
                }
                else
                {
                    if (trans.flag)
                    {
                        logger.Log("Transformer threw flag, aborting!");
                        return;
                    }
                    else
                    {
                        logger.Log("Transformer made no changes! Aborting save...");
                        return;
                    }
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
