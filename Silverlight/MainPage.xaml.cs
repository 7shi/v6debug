using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using PDP11Lib;

namespace V6
{
    public partial class MainPage : UserControl
    {
        private AOut aout;

        public MainPage()
        {
            InitializeComponent();
            ReadResource("Tests/hello");
        }

        private void ReadResource(string fn)
        {
            var uri = new Uri(fn, UriKind.Relative);
            var rs = Application.GetResourceStream(uri);
            if (rs != null) using (var s = rs.Stream) ReadStream(s);
        }

        private void ReadStream(Stream s)
        {
            var data = new byte[s.Length];
            s.Read(data, 0, data.Length);
            ReadBytes(data);
        }

        private void ReadBytes(byte[] data)
        {
            aout = new AOut(data) { UseOct = comboBox1.SelectedIndex == 1 };
            textBox1.Text = aout.GetDisassemble();
            textBox2.Text = aout.GetDump();
            btnSave.IsEnabled = true;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != true) return;

            textBox1.Text = "";
            try
            {
                var fi = ofd.File;
                if (fi.Length >= 64 * 1024)
                    throw new Exception("ファイルが大き過ぎます。上限は64KBです。");
                using (var fs = ofd.File.OpenRead())
                    ReadStream(fs);
            }
            catch (Exception ex)
            {
                textBox1.Text = ex.Message + Environment.NewLine +
                    "読み込みに失敗しました。" + Environment.NewLine;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != true) return;

            using (var fs = sfd.OpenFile())
                fs.Write(aout.Data, 0, aout.Data.Length);
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var cur = Cursor;
            Cursor = Cursors.Wait;
            ReadResource("Tests/" + button.Content.ToString());
            Cursor = cur;
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (aout == null) return;

            var f = comboBox1.SelectedIndex == 1;
            if (aout.UseOct == f) return;

            var cur = Cursor;
            Cursor = Cursors.Wait;
            aout.UseOct = f;
            textBox1.Text = aout.GetDisassemble();
            textBox2.Text = aout.GetDump();
            Cursor = cur;
        }
    }
}
