﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormEnvSaver : Form
    {
        //class variables
        private readonly FormGPS mf;

        public FormEnvSaver(Form callingForm)
        {
            //get copy of the calling main form
            mf = callingForm as FormGPS;
            InitializeComponent();

            //this.bntOK.Text = gStr.gsForNow;
            //this.btnSave.Text = gStr.gsToFile;

            this.Text = gStr.gsSaveEnvironment;
        }

        private void FormFlags_Load(object sender, EventArgs e)
        {
            lblLast.Text = gStr.gsCurrent + mf.envFileName;
            DirectoryInfo dinfo = new DirectoryInfo(mf.envDirectory);
            FileInfo[] Files = dinfo.GetFiles("*.txt");

            if (Files.Length == 0) cboxEnv.Enabled = false;

            foreach (FileInfo file in Files)
            {
                cboxEnv.Items.Add(Path.GetFileNameWithoutExtension(file.Name));
            }
        }

        private void CboxVeh_SelectedIndexChanged(object sender, EventArgs e)
        {
            mf.FileSaveEnvironment(mf.envDirectory + cboxEnv.SelectedItem.ToString() + ".txt");
            Close();
        }

        private void TboxName_TextChanged(object sender, EventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, glm.fileReg, "");

            textboxSender.SelectionStart = cursorPosition;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (tboxName.Text.Trim().Length > 0)
            {
                mf.FileSaveEnvironment(mf.envDirectory + tboxName.Text.Trim() + ".txt");
                Close();
            }
        }

        private void tboxName_Click(object sender, EventArgs e)
        {
            if (mf.isKeyboardOn)
            {
                mf.KeyboardToText((TextBox)sender);
                btnSave.Focus();
            }
        }
    }
}