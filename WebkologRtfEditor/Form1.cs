using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace WebkologRtfEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int selTabIndex, fileAutoNameCounter, holdTabIndex;
        bool isFileLoading, wordWrap = true, showStatusBar = true;
        internal bool isActiveExtraForm = false;
        Font defFont;
        internal string LanguageFile;
        string lngNewFile = "New File ", lngDiaTitleOpenFile, lngMsgBoxNotFoundFile, lngDiaTitleSaveFile, lngDiaAlreadyFileOpen, lngMsgBoxSave, lngMsgBoxSaveHeader, lngMsgBoxDelete, lngMsgBoxDeleteHeader, lngDiaTitleOpenPic, lngStsBarLineCol;

        private void statusClean()
        {
            toolStripLabelTxtColRow.Text = "";
        }

        private void statusFollow()
        {
            if (showStatusBar)
            {
                RichTextBox rb = (tabControl1.SelectedTab.Controls[0] as RichTextBox);
                int txtLine = rb.GetLineFromCharIndex(rb.SelectionStart);
                int txtCol = rb.SelectionStart;
                int firstIndex = rb.Text.LastIndexOf("\n", txtCol);
                txtCol = txtCol - firstIndex;
                if (txtCol == 0)
                {
                    try
                    {
                        txtCol = rb.Lines[txtLine].Length + 1;
                    }
                    catch { }
                }
                txtLine++;
                string stsBarText = lngStsBarLineCol.Replace("%l", txtLine.ToString());
                toolStripLabelTxtColRow.Text = stsBarText.Replace("%c", txtCol.ToString());
            }
        }
        
        private void menuUpdate()
        {
            bool enabled = false;
            int tabCount = tabControl1.Controls.Count;
            if (tabCount > 0)
                enabled = true;
            else
                enabled = false;
            saveToolStripMenuItem.Enabled = enabled;
            saveAsToolStripMenuItem.Enabled = enabled;
            closeToolStripMenuItem.Enabled = enabled;
            closeAllToolStripMenuItem.Enabled = enabled;
            printToolStripMenuItem.Enabled = enabled;
            printPreviewToolStripMenuItem.Enabled = enabled;
            undoToolStripMenuItem.Enabled = enabled;
            redoToolStripMenuItem.Enabled = enabled;
            cutToolStripMenuItem.Enabled = enabled;
            copyToolStripMenuItem.Enabled = enabled;
            pasteToolStripMenuItem.Enabled = enabled;
            selectAllToolStripMenuItem.Enabled = enabled;
            findToolStripMenuItem.Enabled = enabled;
            findAndReplaceToolStripMenuItem.Enabled = enabled;
            insertImageToolStripMenuItem.Enabled = enabled;
            insertDateAndTimeToolStripMenuItem.Enabled = enabled;
            fontToolStripMenuItem.Enabled = enabled;
            boldToolStripMenuItem.Enabled = enabled;
            ıtalicToolStripMenuItem.Enabled = enabled;
            underlineToolStripMenuItem.Enabled = enabled;
            strikeToolStripMenuItem.Enabled = enabled;
            toolStripTop.Enabled = enabled;
        }

        private void menuCheckUpdate()
        {
            wordWrapToolStripMenuItem.Checked = wordWrap;
            statusBarToolStripMenuItem.Checked = showStatusBar;
            wordWrapper();
            if (wordWrap)
            {
                toolStripBottom.Visible = false;
                statusBarToolStripMenuItem.Enabled = false;
            }
            else
            {
                toolStripBottom.Visible = showStatusBar;
                statusBarToolStripMenuItem.Enabled = true;
            }
        }

        private void wordWrapper()
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            foreach (TabPage tp in tabControl1.TabPages)
                foreach (RichTextBox rb in tp.Controls)
                    rb.WordWrap = wordWrap;
        }

        private void toolCheckUpdate()
        {
            int tabCount = tabControl1.Controls.Count;
            if (tabCount == 0)
                return;
            TabPage tp = tabControl1.SelectedTab;
            RichTextBox rb = tp.Controls[0] as RichTextBox;
            Font font = rb.SelectionFont;
            toolStripComboBoxFontName.Text = font.Name;
            toolStripComboBoxFontSize.Text = font.Size.ToString();
            toolStripButtonColor.BackColor = rb.SelectionColor;
            toolStripButtonBold.Checked = font.Bold;
            toolStripButtonItalic.Checked = font.Italic;
            toolStripButtonUnderline.Checked = font.Underline;
            toolStripButtonStrike.Checked = font.Strikeout;
            toolStripButtonJustifyLeft.Checked = false;
            toolStripButtonJustifyCenter.Checked = false;
            toolStripButtonJustifyRight.Checked = false;
            switch (rb.SelectionAlignment)
            {
                case HorizontalAlignment.Center:
                    toolStripButtonJustifyCenter.Checked = true;
                    break;
                case HorizontalAlignment.Left:
                    toolStripButtonJustifyLeft.Checked = true;
                    break;
                case HorizontalAlignment.Right:
                    toolStripButtonJustifyRight.Checked = true;
                    break;
                default:
                    break;
            }
        }

        private void toolMakeDef()
        {
            toolStripButtonBold.Checked = false;
            toolStripButtonItalic.Checked = false;
            toolStripButtonUnderline.Checked = false;
            toolStripButtonStrike.Checked = false;
            toolStripButtonJustifyLeft.Checked = false;
            toolStripButtonJustifyCenter.Checked = false;
            toolStripButtonJustifyRight.Checked = false;
        }

        private void LoadLanguageFiles()
        {
            try
            {
                string langFilePath = Application.StartupPath + @"\Languages\";
                string[] lngFiles = Directory.GetFiles(langFilePath, "*.lng");
                Image img = new Bitmap(1, 1);
                foreach (string lngFile in lngFiles)
                {
                    string lngName = Path.GetFileNameWithoutExtension(lngFile);
                    ToolStripMenuItem tsmi = new ToolStripMenuItem(lngName, img, englishToolStripMenuItem_Click);
                    tsmi.DisplayStyle = ToolStripItemDisplayStyle.Text;
                    languageToolStripMenuItem.DropDownItems.Add(tsmi);
                }
            }
            catch { }
        }

        private void SelectLanguage(string lngFile)
        {
            try
            {
                string LngFilePath = Application.StartupPath + @"\Languages\" + lngFile + ".lng";
                StreamReader sr;
                if (File.Exists(LngFilePath))
                {
                    sr = new StreamReader(LngFilePath);
                    LanguageFile = lngFile;
                }
                else
                {
                    sr = new StreamReader(new MemoryStream(Properties.Resources.English));
                    LanguageFile = "English";
                }
                foreach (ToolStripMenuItem item in languageToolStripMenuItem.DropDownItems)
                {
                    item.Checked = false;
                    if (item.Text == LanguageFile)
                        item.Checked = true;
                }
                Configuration conf = new Configuration(sr);
                sr.Close();
                fileToolStripMenuItem.Text = conf.GetValue("MNU_FILE");
                newToolStripMenuItem.Text = conf.GetValue("MMU_NEW");
                openToolStripMenuItem.Text = conf.GetValue("MNU_OPEN");
                saveToolStripMenuItem.Text = conf.GetValue("MNU_SAVE");
                saveAsToolStripMenuItem.Text = conf.GetValue("MNU_SAVEAS");
                closeToolStripMenuItem.Text = conf.GetValue("MNU_CLOSE");
                closeAllToolStripMenuItem.Text = conf.GetValue("MNU_CLOSEALL");
                printToolStripMenuItem.Text = conf.GetValue("MNU_PRINT");
                printPreviewToolStripMenuItem.Text = conf.GetValue("MNU_PRINTPRV");
                exitToolStripMenuItem.Text = conf.GetValue("MNU_EXIT");
                editToolStripMenuItem.Text = conf.GetValue("MNU_EDIT");
                undoToolStripMenuItem.Text = conf.GetValue("MNU_UNDO");
                cutToolStripMenuItem.Text = conf.GetValue("MNU_CUT");
                copyToolStripMenuItem.Text = conf.GetValue("MNU_COPY");
                pasteToolStripMenuItem.Text = conf.GetValue("MNU_PASTE");
                selectAllToolStripMenuItem.Text = conf.GetValue("MNU_SELALL");
                formatToolStripMenuItem.Text = conf.GetValue("MNU_FORMAT");
                wordWrapToolStripMenuItem.Text = conf.GetValue("MNU_WORDWRAP");
                fontToolStripMenuItem.Text = conf.GetValue("MNU_FONT");
                viewToolStripMenuItem.Text = conf.GetValue("MNU_VIEW");
                statusBarToolStripMenuItem.Text = conf.GetValue("MNU_STSBAR");
                languageToolStripMenuItem.Text = conf.GetValue("MNU_LANG");
                helpToolStripMenuItem.Text = conf.GetValue("MNU_HELP");
                documentationToolStripMenuItem.Text = conf.GetValue("MNU_HELPDOC");
                supportToolStripMenuItem.Text = conf.GetValue("MNU_SUPPORT");
                checkForUpdatesToolStripMenuItem.Text = conf.GetValue("MNU_UPDATE");
                aboutToolStripMenuItem.Text = conf.GetValue("MNU_ABOUT");
                cutToolStripMenuItem1.Text = conf.GetValue("NP_CON_CUT");
                copyToolStripMenuItem1.Text = conf.GetValue("NP_CON_COPY");
                pasteToolStripMenuItem1.Text = conf.GetValue("NP_CON_PASTE");
                selectAllToolStripMenuItem1.Text = conf.GetValue("NP_CON_SELALL");
                closeToolStripMenuItem.Text = conf.GetValue("TP_CON_CLOSE");
                closeOthersToolStripMenuItem.Text = conf.GetValue("TP_CON_OTHERS");
                closeAllToTheLeftToolStripMenuItem.Text = conf.GetValue("TP_CON_LEFT");
                closeAllToTheRightToolStripMenuItem.Text = conf.GetValue("TP_CON_RIGHT");
                saveToolStripMenuItem1.Text = conf.GetValue("TP_CON_SAVE");
                saveAsToolStripMenuItem1.Text = conf.GetValue("TP_CON_SAVEAS");
                deleteToolStripMenuItem.Text = conf.GetValue("TP_CON_DELETE");
                printToolStripMenuItem1.Text = conf.GetValue("TP_CON_PRINT");
                openContainingFolderToolStripMenuItem.Text = conf.GetValue("TP_CON_OPENCONFOLDER");
                readOnlyToolStripMenuItem.Text = conf.GetValue("TP_CON_READONLY");
                copyFullFilePathToClipboardToolStripMenuItem.Text = conf.GetValue("TP_CON_COPYFILEPATH");
                copyFilenameToClipboardToolStripMenuItem.Text = conf.GetValue("TP_CON_COPYFILENAME");
                copyCurrentToolStripMenuItem.Text = conf.GetValue("TP_CON_COPYDIRECTORY");
                //lngNewFile = conf.GetValue("TXT_NEWFILE");
                lngDiaTitleOpenFile = conf.GetValue("DIA_OPEN");
                lngDiaTitleSaveFile = conf.GetValue("DIA_SAVE");
                lngMsgBoxNotFoundFile = conf.GetValue("MSG_NOTTXTFILE");
                lngMsgBoxSaveHeader = conf.GetValue("MSG_SAVE_HEADER");
                lngMsgBoxSave = conf.GetValue("MSG_SAVE");
                lngMsgBoxDeleteHeader = conf.GetValue("MSG_DELETE_HEADER");
                lngMsgBoxDelete = conf.GetValue("MSG_DELETE");
                lngStsBarLineCol = conf.GetValue("STS_TXTPOS");
                lngDiaAlreadyFileOpen = "Alread file open!";

                lngDiaTitleOpenPic = conf.GetValue("DIA_OPEN_IMG");

                redoToolStripMenuItem.Text = conf.GetValue("MNU_REDO");
                findToolStripMenuItem.Text = conf.GetValue("MNU_FIND");
                findAndReplaceToolStripMenuItem.Text = conf.GetValue("MNU_REPLACE");

                ınsertToolStripMenuItem.Text = conf.GetValue("MNU_INSERT");
                insertImageToolStripMenuItem.Text = conf.GetValue("MNU_INS_IMG");
                insertDateAndTimeToolStripMenuItem.Text = conf.GetValue("MNU_INS_DAT_TIME");
                ınsertDateAndTimeToolStripMenuItem.Text = conf.GetValue("MNU_INS_DATIME");
                ınsertDateToolStripMenuItem.Text = conf.GetValue("MNU_INS_DATE");
                ınsertTimeToolStripMenuItem.Text = conf.GetValue("MNU_INS_TIME");

                boldToolStripMenuItem.Text = conf.GetValue("MNU_BOLD");
                ıtalicToolStripMenuItem.Text = conf.GetValue("MNU_ITALIC");
                underlineToolStripMenuItem.Text = conf.GetValue("MNU_UNDERLINE");
                strikeToolStripMenuItem.Text = conf.GetValue("MNU_STRIKE");

                toolStripButtonColor.Text = conf.GetValue("TS_FONT_COLOR");
                toolStripButtonBold.Text = conf.GetValue("TS_BOLD");
                toolStripButtonItalic.Text = conf.GetValue("TS_ITALIC");
                toolStripButtonUnderline.Text = conf.GetValue("TS_UNDERLINE");
                toolStripButtonStrike.Text = conf.GetValue("TS_STRIKE");

                toolStripButtonJustifyLeft.Text = conf.GetValue("TS_ALIGNL");
                toolStripButtonJustifyCenter.Text = conf.GetValue("TS_ALIGNC");
                toolStripButtonJustifyRight.Text = conf.GetValue("TS_ALIGNR");

                toolStripButtonImage.Text = conf.GetValue("TS_INS_IMG");
                toolStripButtonDateDDB.Text = conf.GetValue("TS_INS_DAT_TIME");
                ınsertDateAndTimeToolStripMenuItem1.Text = conf.GetValue("TS_INS_DATIME");
                addToDateToolStripMenuItem.Text = conf.GetValue("TS_INS_DATE");
                addToTimeToolStripMenuItem.Text = conf.GetValue("TS_INS_TIME");

                toolStripButtonFind.Text = conf.GetValue("TS_FIND");
                toolStripButtonReplace.Text = conf.GetValue("TS_REPLACE");
            }
            catch
            {
                MessageBox.Show(lngFile + " language file is damaged or not found!");
                SelectLanguage("English");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            defFont = new Font("Arial", 8f, FontStyle.Regular);
            lngDiaTitleOpenFile = "Open a RTF File";
            lngDiaAlreadyFileOpen = "Already File Open!";
            lngDiaTitleOpenPic = "Open an Image";
            lngMsgBoxNotFoundFile = "Not found RTF file!";
            lngDiaTitleSaveFile = "Save a RTF File";
            lngMsgBoxSaveHeader = "Save";
            lngMsgBoxSave = "Save the file \"%f\"?";
            lngMsgBoxDelete = "Do you want to delete this file?";
            lngMsgBoxDeleteHeader = "Delete";
            lngStsBarLineCol = "Line %l, Col: %c";
            this.MinimumSize = new Size(400, 300);
            AllowDrop = true;
            wordWrap = Properties.Settings.Default.WordWrap;
            showStatusBar = Properties.Settings.Default.StatusBar;
            this.WindowState = Properties.Settings.Default.WindowsState;
            this.Width = Properties.Settings.Default.Width;
            this.Height = Properties.Settings.Default.Height;
            LoadLanguageFiles();
            SelectLanguage(Properties.Settings.Default.Language);
            menuUpdate();
            menuCheckUpdate();
            foreach (FontFamily font in System.Drawing.FontFamily.Families)
                toolStripComboBoxFontName.Items.Add(font.Name);
            isFileLoading = true;
            string[] args = Environment.GetCommandLineArgs();
            string proPath = Application.ExecutablePath;
            foreach (string file in args)
            {
                if (Path.GetExtension(file) == ".rtf")
                {
                    try
                    {
                        StreamReader rdr = new StreamReader(file);
                        newToolStripMenuItem_Click(null, null);
                        (tabControl1.SelectedTab.Controls[0] as RichTextBox).Text = "";
                        TabPage tp = tabControl1.SelectedTab;
                        RichTextBox rb = tp.Controls[0] as RichTextBox;
                        rb.Text = rdr.ReadToEnd();
                        rdr.Close();
                        tp.Tag = new RtfDoc(file, true);
                        tp.Text = (tp.Tag as RtfDoc).FileName;
                    }
                    catch { }
                }
            }
            isFileLoading = false;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            isFileLoading = true;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                if (Path.GetExtension(file) == ".rtf")
                {
                    try
                    {
                        StreamReader rdr = new StreamReader(file);
                        newToolStripMenuItem_Click(null, null);
                        (tabControl1.SelectedTab.Controls[0] as TextBox).Text = "";
                        TabPage tp = tabControl1.SelectedTab;
                        TextBox tb = tp.Controls[0] as TextBox;
                        tb.Text = rdr.ReadToEnd();
                        rdr.Close();
                        tp.Tag = new RtfDoc(file, true);
                        tp.Text = (tp.Tag as RtfDoc).FileName;
                    }
                    catch { }
                }
            }
            isFileLoading = false;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            closeAllToolStripMenuItem_Click(null, null);
            Properties.Settings.Default.WordWrap = wordWrap;
            Properties.Settings.Default.StatusBar = showStatusBar;
            Properties.Settings.Default.WindowsState = this.WindowState;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Height = this.Height;
            AllowDrop = true;
            LoadLanguageFiles();
            Properties.Settings.Default.Language = LanguageFile;
            Properties.Settings.Default.Save();
            Application.Exit();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isFileLoading == false)
                fileAutoNameCounter++;
            string fileName = lngNewFile.Replace("%n", fileAutoNameCounter.ToString());
            int tabCount = tabControl1.TabPages.Count;
            tabControl1.TabPages.Add(fileName);
            TabPage tp = tabControl1.TabPages[tabCount];
            tp.Controls.Add(
                new RichTextBox()
                {
                    Multiline = true,
                    ScrollBars = RichTextBoxScrollBars.Both,
                    Dock = DockStyle.Fill,
                    Font = defFont,
                    SelectionFont = defFont,
                    Name = "rtf_" + tabCount,
                    ContextMenuStrip = contextMenuStripTextBox,
                    WordWrap = wordWrap
                }
            );
            (tp.Controls[0] as RichTextBox).TextChanged += new EventHandler(richTextBox_TextChanged);
            (tp.Controls[0] as RichTextBox).GotFocus += new EventHandler(richTextBox_GotFocus);
            (tp.Controls[0] as RichTextBox).Click += new EventHandler(richTextBox_Click);
            (tp.Controls[0] as RichTextBox).KeyUp += new KeyEventHandler(richTextBox_KeyUp);
            (tp.Controls[0] as RichTextBox).LostFocus += new EventHandler(richTextBox_LostFocus);
            tp.Tag = new RtfDoc(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + fileName + ".rtf");
            tabControl1.SelectTab(tp);
            if (selTabIndex == -1)
                selTabIndex = 0;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "RTF File|*.rtf";
            openFileDialog1.Title = lngDiaTitleOpenFile;
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    foreach (TabPage tabPage in tabControl1.TabPages)
                    {
                        RtfDoc td = tabPage.Tag as RtfDoc;
                        if (openFileDialog1.FileName == td.FilePath)
                        {
                            MessageBox.Show(lngDiaAlreadyFileOpen);
                            return;
                        }
                    }
                    isFileLoading = true;
                    newToolStripMenuItem_Click(null, null);
                    int tabCount = tabControl1.TabPages.Count;
                    int lastTabIndex = tabCount - 1;
                    TabPage tp = tabControl1.TabPages[lastTabIndex];
                    RichTextBox rtb = tp.Controls[0] as RichTextBox;
                    rtb.LoadFile(openFileDialog1.FileName);
                    tp.Tag = new RtfDoc(openFileDialog1.FileName, true);
                    tp.Text = (tp.Tag as RtfDoc).FileName;
                }
                catch { MessageBox.Show(lngMsgBoxNotFoundFile); }
            }
            isFileLoading = false;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabCount == 0)
                return;
            TabPage tp = tabControl1.SelectedTab;
            RtfDoc rd = (tp.Tag as RtfDoc);
            if (rd.Saved)
            {
                RichTextBox rtb = (RichTextBox)tabControl1.SelectedTab.Controls[0];
                rtb.SaveFile(rd.FilePath);
                rd.Changed = false;
                tp.Text = tp.Text.Replace("* ", "");
            }
            else
            {
                saveFileDialog1.Filter = "RTF File|*.rtf";
                saveFileDialog1.Title = lngDiaTitleSaveFile;
                saveFileDialog1.FileName = rd.FilePath;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    RichTextBox rtb = (RichTextBox)tabControl1.SelectedTab.Controls[0];
                    rtb.SaveFile(saveFileDialog1.FileName);
                    rd.Changed = false;
                    tp.Text = tp.Text.Replace("* ", "");
                    tp.Tag = new RtfDoc(saveFileDialog1.FileName, true);
                    tp.Text = (tp.Tag as RtfDoc).FileName;
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabCount == 0)
                return;
            TabPage tp = tabControl1.SelectedTab;
            RtfDoc rd = (tp.Tag as RtfDoc);
            saveFileDialog1.Filter = "RTF File|*.rtf";
            saveFileDialog1.Title = lngDiaTitleSaveFile;
            saveFileDialog1.FileName = rd.FilePath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                RichTextBox rtb = (RichTextBox)tabControl1.SelectedTab.Controls[0];
                rtb.SaveFile(saveFileDialog1.FileName);
                rd.Changed = false;
                tp.Text = tp.Text.Replace("* ", "");
                tp.Tag = new RtfDoc(saveFileDialog1.FileName, true);
                tp.Text = (tp.Tag as RtfDoc).FileName;
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            TabPage tp = tabControl1.TabPages[selTabIndex];
            RtfDoc rd = tp.Tag as RtfDoc;
            if (rd.Changed)
            {
                if (MessageBox.Show(lngMsgBoxSave.Replace("%f", rd.FileName), lngMsgBoxSaveHeader, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    saveToolStripMenuItem_Click(null, null);
            }
            tp.Controls.Clear();
            tabControl1.TabPages.RemoveAt(selTabIndex);
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            bool checkTabs = true;
            while (checkTabs)
            {
                int tabCount = tabControl1.TabPages.Count;
                if (tabCount > 0)
                {
                    TabPage tp = tabControl1.TabPages[tabCount - 1];
                    tabControl1.SelectTab(tp);
                    closeToolStripMenuItem_Click(null, null);
                }
                else
                {
                    checkTabs = false;
                }
            }
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            TabPage tp = tabControl1.SelectedTab;
            RtfDoc rd = tp.Tag as RtfDoc;
            printDialog1.Document = printDocument1;
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                printDocument1.DocumentName = rd.FileName;
                printDocument1.Print();
            }
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            pageSetupDialog1.Document = printDocument1;
            pageSetupDialog1.AllowMargins = true;
            pageSetupDialog1.AllowOrientation = true;
            pageSetupDialog1.AllowPaper = true;
            pageSetupDialog1.AllowPrinter = true;
            pageSetupDialog1.ShowNetwork = true;
            pageSetupDialog1.ShowHelp = true;
            pageSetupDialog1.EnableMetric = true;
            if (pageSetupDialog1.ShowDialog() == DialogResult.OK)
                printDocument1.DefaultPageSettings = pageSetupDialog1.PageSettings;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.SelectAll();
        }

        void showForm(Form form)
        {
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.ShowIcon = false;
            form.TopMost = true;
            form.Show();
            isActiveExtraForm = true;
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isActiveExtraForm)
                return;
            FormFind form = new FormFind();
            showForm(form);
        }

        private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isActiveExtraForm)
                return;
            FormReplace form = new FormReplace();
            showForm(form);
        }

        private void insertImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            openFileDialog1.Filter = "Image Files|*.jpg|*.jpeg|*.png|*.gif|*.bmp";
            openFileDialog1.Title = lngDiaTitleOpenPic;
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Image img = Image.FromFile(openFileDialog1.FileName);
                    Clipboard.SetImage(img);
                    RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                    rtb.Paste();
                    Clipboard.Clear();
                }
                catch { MessageBox.Show(lngMsgBoxNotFoundFile); }
            }
        }

        private void insertDateAndTimeToolStripMenuItem_Click(object sender, EventArgs e){}

        private void ınsertDateAndTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.SelectedText = DateTime.Now.ToString();
        }

        private void ınsertDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.SelectedText = DateTime.Now.Date.ToString().Replace(" 00:00:00", "");
        }

        private void ınsertTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.SelectedText = DateTime.Now.ToString("HH:mm:ss");
        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wordWrap = wordWrap ? false : true;
            menuCheckUpdate();
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            fontDialog1.Font = defFont;
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                rtb.SelectionFont = fontDialog1.Font;
            }
        }

        private void boldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            Font selFont = rtb.SelectionFont;
            if (selFont.Bold)
                rtb.SelectionFont = new Font(selFont, selFont.Style & ~FontStyle.Bold);
            else
                rtb.SelectionFont = new Font(selFont, selFont.Style | FontStyle.Bold);
            toolCheckUpdate();
        }

        private void ıtalicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            Font selFont = rtb.SelectionFont;
            if (selFont.Italic)
                rtb.SelectionFont = new Font(selFont, selFont.Style & ~FontStyle.Italic);
            else
                rtb.SelectionFont = new Font(selFont, selFont.Style | FontStyle.Italic);
            toolCheckUpdate();
        }

        private void underlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            Font selFont = rtb.SelectionFont;
            if (selFont.Underline)
                rtb.SelectionFont = new Font(selFont, selFont.Style & ~FontStyle.Underline);
            else
                rtb.SelectionFont = new Font(selFont, selFont.Style | FontStyle.Underline);
            toolCheckUpdate();
        }

        private void strikeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            Font selFont = rtb.SelectionFont;
            if (selFont.Strikeout)
                rtb.SelectionFont = new Font(selFont, selFont.Style & ~FontStyle.Strikeout);
            else
                rtb.SelectionFont = new Font(selFont, selFont.Style | FontStyle.Strikeout);
            toolCheckUpdate();
        }

        private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showStatusBar = showStatusBar ? false : true;
            menuCheckUpdate();
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectLanguage((sender as ToolStripItem).Text);
        }

        private void openLink(string url)
        {
            try { System.Diagnostics.Process.Start(url); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void documentationToolStripMenuItem_Click(object sender, EventArgs e) { openLink("https://www.webkolog.net/p/webkolog-rtf-editor.html"); }

        private void supportToolStripMenuItem_Click(object sender, EventArgs e) { openLink("https://www.webkolog.net/p/contact.html"); }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e) { documentationToolStripMenuItem_Click(null, null); }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog(this);
        }

        private void toolStripComboBoxFontName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            Font selFont = rtb.SelectionFont;
            rtb.SelectionFont = new Font(toolStripComboBoxFontName.Text, selFont.Size);
        }

        private void toolStripComboBoxFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            try
            {
                float fontSize = Convert.ToSingle(toolStripComboBoxFontSize.Text);
                RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
                Font selFont = rtb.SelectionFont;
                rtb.SelectionFont = new Font(selFont.FontFamily, fontSize);
            }
            catch { }
        }

        private void toolStripComboBoxFontSize_TextChanged(object sender, EventArgs e)
        {
            toolStripComboBoxFontSize_SelectedIndexChanged(null, null);
        }

        private void toolStripButtonColor_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            colorDialog1.Color = rtb.SelectionColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                rtb.SelectionColor = colorDialog1.Color;
                toolStripButtonColor.BackColor = colorDialog1.Color;
            }
        }

        private void toolStripButtonBold_Click(object sender, EventArgs e)
        {
            boldToolStripMenuItem_Click(null, null);
        }

        private void toolStripButtonItalic_Click(object sender, EventArgs e)
        {
            ıtalicToolStripMenuItem_Click(null, null);
        }

        private void toolStripButtonUnderline_Click(object sender, EventArgs e)
        {
            underlineToolStripMenuItem_Click(null, null);
        }

        private void toolStripButtonStrike_Click(object sender, EventArgs e)
        {
            strikeToolStripMenuItem_Click(null, null);
        }

        private void toolStripButtonJustifyLeft_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.SelectionAlignment = HorizontalAlignment.Left;
            toolCheckUpdate();
        }

        private void toolStripButtonJustifyCenter_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.SelectionAlignment = HorizontalAlignment.Center;
            toolCheckUpdate();
        }

        private void toolStripButtonJustifyRight_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            RichTextBox rtb = tabControl1.SelectedTab.Controls[0] as RichTextBox;
            rtb.SelectionAlignment = HorizontalAlignment.Right;
            toolCheckUpdate();
        }

        private void toolStripButtonImage_Click(object sender, EventArgs e)
        {
            insertImageToolStripMenuItem_Click(null, null);
        }

        private void ınsertDateAndTimeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ınsertDateAndTimeToolStripMenuItem_Click(null, null);
        }

        private void addToDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ınsertDateToolStripMenuItem_Click(null, null);
        }

        private void addToTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ınsertTimeToolStripMenuItem_Click(null, null);
        }

        private void toolStripButtonFind_Click(object sender, EventArgs e)
        {
            findToolStripMenuItem_Click(null, null);
        }

        private void toolStripButtonReplace_Click(object sender, EventArgs e)
        {
            findAndReplaceToolStripMenuItem_Click(null, null);
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                return;
            TabPage tp = tabControl1.SelectedTab;
            RichTextBox rtb = tp.Controls[0] as RichTextBox;
            e.Graphics.DrawString(rtb.Text, rtb.Font, Brushes.Black, 10, 25);
        }

        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            cutToolStripMenuItem_Click(null, null);
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(null, null);
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Click(null, null);
        }

        private void selectAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            selectAllToolStripMenuItem_Click(null, null);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            closeToolStripMenuItem_Click(null, null);
        }

        private void closeOthersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            holdTabIndex = selTabIndex;
            bool checkTabs = true;
            while (checkTabs)
            {
                int tabCount = tabControl1.TabPages.Count;
                if (tabCount > 1)
                {
                    int currentTabIndex = tabCount - 1;
                    if (holdTabIndex == currentTabIndex)
                    {
                        currentTabIndex--;
                        holdTabIndex--;
                    }
                    TabPage tp = tabControl1.TabPages[currentTabIndex];
                    tabControl1.SelectTab(tp);
                    closeToolStripMenuItem_Click(null, null);
                }
                else
                {
                    checkTabs = false;
                }
            }
        }

        private void closeAllToTheLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            holdTabIndex = selTabIndex;
            bool checkTabs = true;
            while (checkTabs)
            {
                int tabCount = tabControl1.TabPages.Count;
                if (tabCount > 1)
                {
                    if (holdTabIndex > 0)
                    {
                        int currentTabIndex = holdTabIndex - 1;
                        holdTabIndex--;
                        TabPage tp = tabControl1.TabPages[currentTabIndex];
                        tabControl1.SelectTab(tp);
                        closeToolStripMenuItem_Click(null, null);
                    }
                    else
                    {
                        checkTabs = false;
                    }
                }
                else
                {
                    checkTabs = false;
                }
            }
            tabControl1.SelectTab(holdTabIndex);
        }

        private void closeAllToTheRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            holdTabIndex = selTabIndex;
            bool checkTabs = true;
            while (checkTabs)
            {
                int tabCount = tabControl1.TabPages.Count;
                if (tabCount > 1)
                {
                    int currentTabIndex = tabCount - 1;
                    if (holdTabIndex == currentTabIndex)
                    {
                        checkTabs = false;
                    }
                    else
                    {
                        TabPage tp = tabControl1.TabPages[currentTabIndex];
                        tabControl1.SelectTab(tp);
                        closeToolStripMenuItem_Click(null, null);
                    }
                }
                else
                {
                    checkTabs = false;
                }
            }
            tabControl1.SelectTab(holdTabIndex);
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveToolStripMenuItem_Click(null, null);
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveAsToolStripMenuItem_Click(null, null);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tp = tabControl1.SelectedTab;
            RtfDoc rd = tp.Tag as RtfDoc;
            if (rd.Saved)
            {
                if (MessageBox.Show(lngMsgBoxDelete, lngMsgBoxDeleteHeader, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Delete(rd.FilePath);
                    tabControl1.TabPages.Remove(tp);
                }
            }
        }

        private void printToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            printToolStripMenuItem_Click(null, null);
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tp = tabControl1.SelectedTab;
            RtfDoc rd = tp.Tag as RtfDoc;
            string filePath = rd.FilePath;
            if (!File.Exists(filePath))
                return;
            string argument = @"/select, " + filePath;
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private void readOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            TabPage tp = tabControl1.SelectedTab;
            RichTextBox rb = tp.Controls[0] as RichTextBox;
            tsmi.Checked = tsmi.Checked ? false : true;
            rb.ReadOnly = tsmi.Checked;
        }

        private void copyFullFilePathToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tp = tabControl1.SelectedTab;
            RtfDoc rd = tp.Tag as RtfDoc;
            Clipboard.SetText(rd.FilePath);
        }

        private void copyFilenameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tp = tabControl1.SelectedTab;
            RtfDoc rd = tp.Tag as RtfDoc;
            Clipboard.SetText(rd.FileName);
        }

        private void copyCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage tp = tabControl1.SelectedTab;
            RtfDoc rd = tp.Tag as RtfDoc;
            string dirPath = Path.GetDirectoryName(rd.FilePath);
            Clipboard.SetText(dirPath);
        }

        private void tabControl1_ControlAdded(object sender, ControlEventArgs e)
        {
            menuUpdate();
        }

        private void tabControl1_ControlRemoved(object sender, ControlEventArgs e)
        {
            menuUpdate();
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            selTabIndex = e.TabPageIndex;
        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < tabControl1.TabCount; ++i)
                {
                    if (tabControl1.GetTabRect(i).Contains(e.Location))
                    {
                        tabControl1.SelectTab(i);
                        selTabIndex = i;
                        contextMenuStripTab.Show(tabControl1, e.Location);
                        break;
                    }
                }
            }
            else if (e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < tabControl1.TabCount; ++i)
                {
                    if (tabControl1.GetTabRect(i).Contains(e.Location))
                    {
                        tabControl1.SelectTab(i);
                        selTabIndex = i;
                        break;
                    }
                }
                closeToolStripMenuItem_Click(null, null);
            }
        }

        private void richTextBox_TextChanged(object sender, EventArgs e)
        {
            if (isFileLoading == false)
            {
                TabPage tp = (TabPage)(sender as RichTextBox).Parent;
                RtfDoc rd = (tp.Tag as RtfDoc);
                if (rd.Changed != true)
                {
                    rd.Changed = true;
                    tp.Text = "* " + tp.Text;
                }
                statusFollow();
            }
        }

        void richTextBox_LostFocus(object sender, EventArgs e)
        {
            toolMakeDef();
            statusClean();
        }

        void richTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            toolCheckUpdate();
            statusFollow();
        }

        void richTextBox_Click(object sender, EventArgs e)
        {
            toolCheckUpdate();
            statusFollow();
        }

        void richTextBox_GotFocus(object sender, EventArgs e)
        {
            toolCheckUpdate();
            statusFollow();
        }

    }

    class RtfDoc
    {
        public bool Changed = false;
        public bool Saved = false;
        public string FileName;
        public string FilePath;

        public RtfDoc(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
        }

        public RtfDoc(string filePath, bool saved)
        {
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            Saved = saved;
        }

        public RtfDoc(string filePath, bool saved, bool changed)
        {
            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(filePath);
            Saved = saved;
            Changed = changed;
        }
    }
}
