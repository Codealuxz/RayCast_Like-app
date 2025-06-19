using System.Windows.Documents;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace RayCast
{
    public static class MarkdownConverter
    {
        public static void ConvertToRichText(System.Windows.Controls.RichTextBox richTextBox, string markdownText)
        {
            
            richTextBox.Document.Blocks.Clear();

            
            string[] lines = markdownText.Split('\n');
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    richTextBox.Document.Blocks.Add(new Paragraph());
                    continue;
                }

                
                if (line.StartsWith("* ") || line.StartsWith("- "))
                {
                    var paragraph = new Paragraph();
                    paragraph.Margin = new System.Windows.Thickness(20, 0, 0, 0);
                    var run = new Run(line.Substring(2));
                    ProcessBold(run);
                    ProcessItalic(run);
                    paragraph.Inlines.Add(run);
                    richTextBox.Document.Blocks.Add(paragraph);
                    continue;
                }

                
                if (line.StartsWith("# "))
                {
                    var paragraph = new Paragraph();
                    var run = new Run(line.Substring(2));
                    run.FontSize = 18;
                    run.FontWeight = System.Windows.FontWeights.Bold;
                    ProcessBold(run);
                    ProcessItalic(run);
                    paragraph.Inlines.Add(run);
                    richTextBox.Document.Blocks.Add(paragraph);
                    continue;
                }
                else if (line.StartsWith("## "))
                {
                    var paragraph = new Paragraph();
                    var run = new Run(line.Substring(3));
                    run.FontSize = 16;
                    run.FontWeight = System.Windows.FontWeights.Bold;
                    ProcessBold(run);
                    ProcessItalic(run);
                    paragraph.Inlines.Add(run);
                    richTextBox.Document.Blocks.Add(paragraph);
                    continue;
                }

                
                var normalParagraph = new Paragraph();
                var normalRun = new Run(line);
                ProcessBold(normalRun);
                ProcessItalic(normalRun);
                normalParagraph.Inlines.Add(normalRun);
                richTextBox.Document.Blocks.Add(normalParagraph);
            }
        }

        private static void ProcessBold(Run run)
        {
            string text = run.Text;
            var matches = Regex.Matches(text, @"\*\*(.*?)\*\*");
            if (matches.Count > 0)
            {
                run.Text = text.Replace("**", "");
                run.FontWeight = System.Windows.FontWeights.Bold;
            }
        }

        private static void ProcessItalic(Run run)
        {
            string text = run.Text;
            var matches = Regex.Matches(text, @"\*(.*?)\*");
            if (matches.Count > 0)
            {
                run.Text = text.Replace("*", "");
                run.FontStyle = System.Windows.FontStyles.Italic;
            }
        }
    }
} 