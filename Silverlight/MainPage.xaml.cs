﻿using System;
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
        private byte[] data;

        public MainPage()
        {
            InitializeComponent();
            ReadResource("a.out");
        }

        private void ReadResource(string fn)
        {
            var uri = new Uri(fn, UriKind.Relative);
            var rs = Application.GetResourceStream(uri);
            if (rs != null) using (var s = rs.Stream) ReadStream(s);
        }

        private void ReadStream(Stream s)
        {
            data = new byte[s.Length];
            s.Read(data, 0, data.Length);
            ReadBytes(data);
        }

        private void ReadBytes(byte[] data)
        {
            using (var sw = new StringWriter())
            {
                var fh = new AOut();
                fh.Read(data);
                fh.Write(sw);
                textBox1.Text = sw.ToString();
            }
            using (var sw = new StringWriter())
            {
                for (int i = 0; i < data.Length; i += 16)
                {
                    sw.Write("{0:X4}:", i);
                    var sb = new StringBuilder();
                    for (int j = 0; j < 16; j++)
                    {
                        if (i + j < data.Length)
                        {
                            if (j == 8) sw.Write(" -");
                            var b = data[i + j];
                            sw.Write(" {0:X2}", b);
                            sb.Append(32 <= b && b < 128 ? (char)b : '.');
                        }
                        else
                        {
                            if (j == 8) sw.Write("  ");
                            sw.Write("   ");
                        }
                    }
                    sw.WriteLine(" {0}", sb.ToString());
                }
                textBox2.Text = sw.ToString();
            }
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
                fs.Write(data, 0, data.Length);
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var name = button.Name;
            if (!name.StartsWith("btnTest")) return;

            ReadTest(int.Parse(name.Substring(7)));
        }

        private void ReadTest(int p)
        {
            switch (p)
            {
                case 1:
                    ReadResource("a.out");
                    break;
            }
        }
    }
}
