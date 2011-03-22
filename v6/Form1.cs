using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PDP11Lib;

namespace v6
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
            OpenFile(GetFileName("a.out"));
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                OpenFile(openFileDialog1.FileName);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private static string GetFileName(string fn)
        {
            return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), fn);
        }

        private void OpenFile(string fn)
        {
            textBox1.Clear();
            using (var sw = new StringWriter())
            {
                using (var fs = new FileStream(fn, FileMode.Open))
                using (var br = new BinaryReader(fs))
                {
                    var fh = new AOut();
                    fh.Read(br);
                    fh.Write(sw);
                }
                textBox1.Text = sw.ToString();
                textBox1.SelectionStart = 0;
            }
        }
    }
}
