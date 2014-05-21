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
            String binaryString = LoadFile(e);
            TextBoxLeft.Document.Blocks.Clear();
            TextBoxLeft.AppendText(binaryString);
            Compare();
        }

        private void FileDropRight(object sender, DragEventArgs e)
        {
            String binaryString = LoadFile(e);
            TextBoxRight.Document.Blocks.Clear();
            TextBoxRight.AppendText(binaryString);
            Compare();
        }

        enum TextRangeType
        {
            Same,        // textRange for when left and right are the same
            Different,   // textRange for when left and right are different
            Undefined,   // initial value
        };


        private TextRangeType InitializeRangeType(char leftChar, char rightChar)
        {
            if( leftChar == rightChar )
                return TextRangeType.Same;

            return TextRangeType.Different;
        }

        private TextRangeType SwitchRangeType(TextRangeType rangeType)
        {
            return (rangeType==TextRangeType.Same)? TextRangeType.Different : TextRangeType.Same;
        }

        private bool CanAddToRange(char leftChar, char rightChar, TextRangeType rangeType)
        {
            if (rangeType == TextRangeType.Undefined)
                return false;

            if (leftChar == ' ' || leftChar == '-' || leftChar == '\r' || leftChar == '\n')
                return true;

            if ((rangeType == TextRangeType.Same) && (leftChar == rightChar))
                return true;

            if ((rangeType == TextRangeType.Different) && (leftChar != rightChar))
                return true;

            return false;
        }



        private void FlushToDocument(String text, RichTextBox rtb, TextRangeType rangeType)
        {
            if (String.IsNullOrEmpty(text))
                return;

            TextRange textRange = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd);
            textRange.Text = text;
            if (rangeType == TextRangeType.Different)
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
                textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            }
            else
            {
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Regular);
            }
        }


        private void Compare()
        {
            String leftText = TextBoxLeft.GetText();
            String rightText = TextBoxRight.GetText();

            if (!String.IsNullOrEmpty(leftText) && !String.IsNullOrEmpty(rightText))
            {
                TextBoxLeft.Document.Blocks.Clear();
                TextBoxRight.Document.Blocks.Clear();

                int minSize = Math.Min(leftText.Length, rightText.Length);
                int maxSize = Math.Max(leftText.Length, rightText.Length);
                

                TextRangeType rangeType = TextRangeType.Undefined;
                String rangeStringLeft = String.Empty;
                String rangeStringRight = String.Empty;

                for (int i = 0; i < minSize; ++i )
                {
                    char leftChar = leftText.ElementAt(i);
                    char rightChar = rightText.ElementAt(i);

                    if (CanAddToRange(leftChar, rightChar, rangeType))
                    {
                        rangeStringLeft  += leftChar;
                        rangeStringRight += rightChar;
                    }
                    else
                    {
                        // Flush text to document (in a new TextRange)
                        FlushToDocument(rangeStringLeft, TextBoxLeft, rangeType);
                        FlushToDocument(rangeStringRight, TextBoxRight, rangeType);

                        if (rangeType == TextRangeType.Undefined)
                            rangeType = InitializeRangeType(leftChar, rightChar);
                        else
                            rangeType = SwitchRangeType(rangeType);

                        rangeStringLeft = String.Empty + leftChar;
                        rangeStringRight = String.Empty + rightChar;
                    }
                }

                // Flush final range
                FlushToDocument(rangeStringLeft, TextBoxLeft, rangeType);
                FlushToDocument(rangeStringRight, TextBoxRight, rangeType);

                // Add a TextRange for characters that are in the long string, but not in the short one
                if( leftText.Length != rightText.Length )
                {
                    String longString = (leftText.Length > rightText.Length) ? leftText : rightText;
                    RichTextBox rtb = (leftText.Length > rightText.Length) ? TextBoxLeft : TextBoxRight;
                    String diffString = longString.Substring(minSize);
                    FlushToDocument(diffString, rtb, TextRangeType.Different);
                }
            }

        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // TODO
            //Compare();
        }
    }
}
