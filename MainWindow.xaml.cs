using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharpVisualBinaryDiff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MyCatch(System.Exception ex)
        {
            var st = new StackTrace(ex, true);      // stack trace for the exception with source file information
            var frame = st.GetFrame(0);             // top stack frame
            String sourceMsg = String.Format("{0}({1})", frame.GetFileName(), frame.GetFileLineNumber());
            Console.WriteLine(sourceMsg);
            MessageBox.Show(ex.Message + Environment.NewLine + sourceMsg);
            Debugger.Break();
        }

        private String LoadFile(DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                    return "Not a file!";

                String[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 1)
                    return "Too many files!";

                String fileName = files[0];

                if (!File.Exists(fileName))
                    return "Not a file!";

                byte[] bytes;
                try
                {
                    bytes = File.ReadAllBytes(fileName);
                }
                catch (Exception ex)
                {
                    return "File.ReadAllBytes() exception:" + Environment.NewLine + ex.Message;
                }

                String binaryString = String.Empty;
                int newLineCount = 0;
                for (int i = 0; i < (bytes.Length + 3)/ 4; ++i )
                {
                    Int32 start = 4 * i;
                    Int32 length = Math.Min(4, bytes.Length - start);
                    binaryString += BitConverter.ToString(bytes, start, length);
                    newLineCount = (newLineCount + 1) % 4;
                    if( newLineCount==0)
                        binaryString += Environment.NewLine;
                    else
                        binaryString += " ";
                }

                return binaryString;
            }
            catch (System.Exception ex)
            {
                MyCatch(ex);
                return "Exception";
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void FileDropLeft(object sender, DragEventArgs e)
        {
            TextBoxLeft.Text = LoadFile(e);
            Compare();
        }

        private void FileDropRight(object sender, DragEventArgs e)
        {
            TextBoxRight.Text = LoadFile(e);
            Compare();
        }

        private void Compare()
        {
            if (!String.IsNullOrEmpty(TextBoxLeft.Text) && !String.IsNullOrEmpty(TextBoxRight.Text))
            {
                // TODO

            }

        }

    }
}
