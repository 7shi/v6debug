using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PDP11Lib;

namespace V6
{
    public partial class Form1 : Form
    {
        private delegate void Action();

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            OpenFile(GetFileName("hello"));
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) != DialogResult.OK)
                return;

            var cur = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            OpenFile(openFileDialog1.FileName);
            Cursor.Current = cur;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private static string GetFileName(string fn)
        {
            return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), fn);
        }

        private AOut aout;

        private void OpenFile(string fn)
        {
            textBox1.Clear();

            var cur = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            aout = new AOut(File.ReadAllBytes(fn)) { UseOct = octToolStripMenuItem.Checked };
            textBox1.Text = aout.GetDisassemble();
            textBox1.SelectionStart = 0;
            Cursor.Current = cur;
        }

        private void setView()
        {
            if (aout == null) return;

            var cur = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            aout.UseOct = octToolStripMenuItem.Checked;
            textBox1.Text = aout.GetDisassemble();
            textBox1.SelectionStart = 0;
            Cursor.Current = cur;
        }

        private void hexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hexToolStripMenuItem.Checked = true;
            octToolStripMenuItem.Checked = false;
            setView();
        }

        private void octToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hexToolStripMenuItem.Checked = false;
            octToolStripMenuItem.Checked = true;
            setView();
        }
    }
}
