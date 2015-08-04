using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

namespace WebkologRtfEditor
{
    public partial class FormFind : Form
    {
        public FormFind()
        {
            InitializeComponent();
        }

        Form1 frm;
        RichTextBox rtb;
        bool isError;
        int countWord, remPos;

        private int GetWordCount()
        {
            MatchCollection wordColl;
            if (checkBoxCase.Checked)
                wordColl = Regex.Matches(rtb.Text, Regex.Escape(textBoxFind.Text));
            else
                wordColl = Regex.Matches(rtb.Text, Regex.Escape(textBoxFind.Text), RegexOptions.IgnoreCase);
            return wordColl.Count;
        }

        private RichTextBoxFinds FindOptions
        {
            get
            {
                RichTextBoxFinds options = RichTextBoxFinds.None;
                if (checkBoxCase.Checked)
                {
                    options |= RichTextBoxFinds.MatchCase;
                }
                if (radioButtonUp.Checked)
                {
                    options |= RichTextBoxFinds.Reverse;
                }
                return options;
            }
        }

        private void countWords()
        {
            int iStart = -1;
            RichTextBoxFinds Options = FindOptions;
            int iPos = -1;
            int iEnd = 0;

            if ((Options & RichTextBoxFinds.Reverse) == RichTextBoxFinds.Reverse)
            {
                iStart = 0;
                iEnd = rtb.SelectionStart;
            }
            else
            {
                iStart = rtb.SelectionStart + rtb.SelectionLength;
                iEnd = rtb.Text.Length;
            }
            iPos = rtb.Find(textBoxFind.Text, iStart, iEnd, Options);

            if (iPos >= 0)
            {
                rtb.Select(iPos, textBoxFind.Text.Length);
                countWord++;
                countWords();
            }
            else { }
        }

        private void LoadLanguage(bool IsInternalFile)
        {
            try
            {
                StreamReader sr;
                if (IsInternalFile)
                {
                    sr = new StreamReader(new MemoryStream(Properties.Resources.English));
                }
                else
                {
                    string lngFile = frm.LanguageFile;
                    string LngFilePath = Application.StartupPath + @"\Languages\" + lngFile + ".lng";
                    if (File.Exists(LngFilePath))
                        sr = new StreamReader(LngFilePath);
                    else
                        sr = new StreamReader(new MemoryStream(Properties.Resources.English));
                }
                Configuration conf = new Configuration(sr);
                sr.Close();
                this.Text = conf.GetValue("FINFORM_TITLE");
                labelFind.Text = conf.GetValue("FINFORM_TB_FINDTXT");
                checkBoxCase.Text = conf.GetValue("FINFORM_CB_CASESEN");
                groupBox1.Text = conf.GetValue("FINFORM_GB_DIRECTION");
                radioButtonUp.Text = conf.GetValue("FINFORM_RB_UP");
                radioButtonDown.Text = conf.GetValue("FINFORM_RB_DOWN");
                buttonFindNext.Text = conf.GetValue("FINFORM_BTN_FINDNEXT");
                buttonCount.Text = conf.GetValue("FINFORM_BTN_COUNT");
                buttonClose.Text = conf.GetValue("FINFORM_BTN_CLOSE");
            }
            catch
            {
                LoadLanguage(true);
            }
        }

        private void buttonFindNext_Click(object sender, EventArgs e)
        {
            if (textBoxFind.Text.Length == 0)
                return;
            if (isError)
                return;
            int iStart = -1;
            RichTextBoxFinds Options = FindOptions;
            int iPos = -1;
            int iEnd = 0;

            if ((Options & RichTextBoxFinds.Reverse) == RichTextBoxFinds.Reverse)
            {
                iStart = 0;
                iEnd = rtb.SelectionStart;
            }
            else
            {
                iStart = rtb.SelectionStart + rtb.SelectionLength;
                iEnd = rtb.Text.Length;
            }
            iPos = rtb.Find(textBoxFind.Text, iStart, iEnd, Options);
            if (iPos >= 0)
            {
                rtb.Select(iPos, textBoxFind.Text.Length);
                rtb.Focus();
            }
            else
            {
                MessageBox.Show("No more occurences found", "Find complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void buttonCount_Click(object sender, EventArgs e)
        {
            if (textBoxFind.Text.Length == 0)
                return;
            if (isError)
                return;
            countWord = GetWordCount();
            MessageBox.Show("Found: " + countWord.ToString());
        }

        private void buttonClose_Click(object sender, EventArgs e) { this.Close(); }

        private void FormFind_Load(object sender, EventArgs e)
        {
            try
            {
                frm = Application.OpenForms["Form1"] as Form1;
                TabPage tp = frm.tabControl1.SelectedTab;
                rtb = tp.Controls[0] as RichTextBox;
            }
            catch { isError = true; }
            LoadLanguage(false);
        }
    }
}
