﻿using System;
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
            value += "\r\n";
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(log), new object[] { value });
                return;
            }
            crackLogTextBox.Text += value;
        }
        private void crackButton_Click(object sender, EventArgs e)
        {
            crackWorker.RunWorkerAsync();
            this.fileSelectTextBox.Enabled = false;
            this.taskComboBox.Enabled = false;
            this.crackButton.Enabled = false;
            this.fileSelectButton.Enabled = false;
        }

        private void crackWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (fileSelectTextBox.Text != "")
            {
                crackLogTextBox.Text = "";
                Transformer trans;
                switch (taskComboBox.GetItemText(taskComboBox.SelectedItem))
                {

                    case "ExploitN Cracker":
                        trans = new ENCracker(fileSelectTextBox.Text);
                        break;

                    case "RSCBTagger":
                        trans = new RSCBTagger(fileSelectTextBox.Text);
                        break;

                    default:
                        MessageBox.Show("No task selected, please select one!");
                        return;
                }
                log("Starting Process...");
                trans.load();
                log("Running Transformer...");
                trans.transform();
                log("Saving Assembly...");
                trans.save();
                log("Finished!");
            }
            else
            {
                MessageBox.Show("Please select an assembly first!");
            }
        }
    }
}
