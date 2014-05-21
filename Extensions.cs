using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace SharpVisualBinaryDiff
{
    static class Extensions
    {
        // http://msdn.microsoft.com/en-us/library/ms754041(v=vs.110).aspx
        static public String GetText(this System.Windows.Controls.RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                rtb.Document.ContentStart,  // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentEnd // TextPointer to the end of content in the RichTextBox.
            );

            // The Text property on a TextRange object returns a string 
            // representing the plain text content of the TextRange. 
            return textRange.Text;
        }
    }
}
