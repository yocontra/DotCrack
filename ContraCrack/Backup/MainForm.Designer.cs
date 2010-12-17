namespace ContraCrack
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.crackButton = new System.Windows.Forms.Button();
            this.fileSelectLabel = new System.Windows.Forms.Label();
            this.fileSelectTextBox = new System.Windows.Forms.TextBox();
            this.fileSelectButton = new System.Windows.Forms.Button();
            this.crackLogTextBox = new System.Windows.Forms.TextBox();
            this.taskComboBox = new System.Windows.Forms.ComboBox();
            this.taskLabel = new System.Windows.Forms.Label();
            this.crackWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // crackButton
            // 
            this.crackButton.Location = new System.Drawing.Point(197, 39);
            this.crackButton.Name = "crackButton";
            this.crackButton.Size = new System.Drawing.Size(75, 23);
            this.crackButton.TabIndex = 0;
            this.crackButton.Text = "Run";
            this.crackButton.UseVisualStyleBackColor = true;
            this.crackButton.Click += new System.EventHandler(this.crackButton_Click);
            // 
            // fileSelectLabel
            // 
            this.fileSelectLabel.AutoSize = true;
            this.fileSelectLabel.Location = new System.Drawing.Point(13, 16);
            this.fileSelectLabel.Name = "fileSelectLabel";
            this.fileSelectLabel.Size = new System.Drawing.Size(29, 13);
            this.fileSelectLabel.TabIndex = 1;
            this.fileSelectLabel.Text = "File: ";
            // 
            // fileSelectTextBox
            // 
            this.fileSelectTextBox.Enabled = false;
            this.fileSelectTextBox.Location = new System.Drawing.Point(48, 13);
            this.fileSelectTextBox.Name = "fileSelectTextBox";
            this.fileSelectTextBox.Size = new System.Drawing.Size(143, 20);
            this.fileSelectTextBox.TabIndex = 3;
            // 
            // fileSelectButton
            // 
            this.fileSelectButton.Location = new System.Drawing.Point(197, 10);
            this.fileSelectButton.Name = "fileSelectButton";
            this.fileSelectButton.Size = new System.Drawing.Size(75, 23);
            this.fileSelectButton.TabIndex = 4;
            this.fileSelectButton.Text = "Select...";
            this.fileSelectButton.UseVisualStyleBackColor = true;
            this.fileSelectButton.Click += new System.EventHandler(this.fileSelectButton_Click);
            // 
            // crackLogTextBox
            // 
            this.crackLogTextBox.Location = new System.Drawing.Point(13, 68);
            this.crackLogTextBox.Multiline = true;
            this.crackLogTextBox.Name = "crackLogTextBox";
            this.crackLogTextBox.ReadOnly = true;
            this.crackLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.crackLogTextBox.Size = new System.Drawing.Size(259, 211);
            this.crackLogTextBox.TabIndex = 5;
            // 
            // taskComboBox
            // 
            this.taskComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.taskComboBox.FormattingEnabled = true;
            this.taskComboBox.Items.AddRange(new object[] {
            "RSCBTagger",
            "ExploitN Cracker",
            "TheBotNet Cracker",
            "BottingNation Cracker",
            "TrueCheaters Cracker",
            "CheatingNetwork Cracker"});
            this.taskComboBox.Location = new System.Drawing.Point(48, 39);
            this.taskComboBox.Name = "taskComboBox";
            this.taskComboBox.Size = new System.Drawing.Size(143, 21);
            this.taskComboBox.TabIndex = 6;
            // 
            // taskLabel
            // 
            this.taskLabel.AutoSize = true;
            this.taskLabel.Location = new System.Drawing.Point(8, 44);
            this.taskLabel.Name = "taskLabel";
            this.taskLabel.Size = new System.Drawing.Size(34, 13);
            this.taskLabel.TabIndex = 7;
            this.taskLabel.Text = "Task:";
            // 
            // crackWorker
            // 
            this.crackWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.crackWorker_DoWork);
            this.crackWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.crackWorker_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 291);
            this.Controls.Add(this.taskLabel);
            this.Controls.Add(this.taskComboBox);
            this.Controls.Add(this.crackLogTextBox);
            this.Controls.Add(this.fileSelectButton);
            this.Controls.Add(this.fileSelectTextBox);
            this.Controls.Add(this.fileSelectLabel);
            this.Controls.Add(this.crackButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "ContraCrack";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button crackButton;
        private System.Windows.Forms.Label fileSelectLabel;
        private System.Windows.Forms.TextBox fileSelectTextBox;
        private System.Windows.Forms.Button fileSelectButton;
        private System.Windows.Forms.TextBox crackLogTextBox;
        private System.Windows.Forms.ComboBox taskComboBox;
        private System.Windows.Forms.Label taskLabel;
        private System.ComponentModel.BackgroundWorker crackWorker;
    }
}

