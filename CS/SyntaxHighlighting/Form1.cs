using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraRichEdit;
using System.Collections.Generic;
using DevExpress.XtraRichEdit.API.Native;
using DevExpress.XtraRichEdit.Services.Implementation;

namespace SyntaxHighlighting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            richEditControl1.LoadDocument("test.sql", DocumentFormat.PlainText);

            ISyntaxHighlightService service = richEditControl1.GetService<ISyntaxHighlightService>();
            MySyntaxHighlightServiceWrapper wrapper = new MySyntaxHighlightServiceWrapper(richEditControl1);
            richEditControl1.RemoveService(typeof(ISyntaxHighlightService));
            richEditControl1.AddService(typeof(ISyntaxHighlightService), wrapper);
        }

        private void richEditControl1_DocumentLoaded(object sender, EventArgs e)
        {
            ClearSyntaxHighlightServiceCache();
        }

        private void richEditControl1_EmptyDocumentCreated(object sender, EventArgs e)
        {
            ClearSyntaxHighlightServiceCache();
        }
        void ClearSyntaxHighlightServiceCache()
        {
            ISyntaxHighlightService service = richEditControl1.GetService<ISyntaxHighlightService>();
            if (service == null)
                return;

            MySyntaxHighlightServiceWrapper wrapper = service as MySyntaxHighlightServiceWrapper;
            if (wrapper == null)
                return;

            wrapper.ResetCache();
        }

        class MySyntaxHighlightServiceWrapper : ISyntaxHighlightService
        {
            RichEditControl control;
            static string[] str;
            List<int> paragraphHashes;
            static MySyntaxHighlightServiceWrapper()
            {
                str = new string[] { "INSERT", "SELECT", "CREATE", "TABLE", "USE", "IDENTITY", "ON", "OFF", "NOT", "NULL", "WITH", "SET" };
                Array.Sort(str);
            }
            public MySyntaxHighlightServiceWrapper(RichEditControl control)
            {
                this.control = control;
                paragraphHashes = new List<int>();

            }
            public void ResetCache()
            {
                paragraphHashes.Clear();
            }
            #region ISyntaxHighlightService Members

            public void Execute()
            {
                Document doc = this.control.Document;
                int paragraphCount = doc.Paragraphs.Count;
                for (int i = 0; i < paragraphCount; i++)
                {
                    HighlightParagraph(i);

                } 
            }

            void HighlightParagraph(int paragraphIndex)
            {
                Document doc = this.control.Document;
                Paragraph paragraph = doc.Paragraphs[paragraphIndex];
                DocumentRange paragraphRange = paragraph.Range;
                int paragraphStart = paragraphRange.Start.ToInt();
                string text = doc.GetText(paragraphRange);
                int hash = text.GetHashCode();
                if (paragraphIndex < paragraphHashes.Count && paragraphHashes[paragraphIndex] == hash)
                    return;
                int length = text.Length;
                int prevWhiteSpaceIndex = -1;
                for (int i = 0; i <= length; i++)
                {
                    char ch;

                    if (i < length)
                        ch = text[i];
                    else
                        ch = ' ';

                    if (Char.IsWhiteSpace(ch) || Char.IsPunctuation(ch))
                    {
                        int wordLength = i - prevWhiteSpaceIndex - 1;
                        if (wordLength > 0)
                        {
                            int wordStart = prevWhiteSpaceIndex + 1;
                            string word = text.Substring(wordStart, wordLength);
                            int index = Array.BinarySearch(str, word);
                            DocumentRange range = doc.CreateRange(paragraphStart + wordStart, wordLength);
                            CharacterProperties cp = doc.BeginUpdateCharacters(range);
                            if (index >= 0)
                                cp.ForeColor = Color.Blue;
                            else
                                cp.ForeColor = Color.Black;
                            doc.EndUpdateCharacters(cp);
                        }
                        prevWhiteSpaceIndex = i;

                    }
                }
                for (int i = paragraphHashes.Count; i <= paragraphIndex; i++)
                    paragraphHashes.Add(String.Empty.GetHashCode());
                paragraphHashes[paragraphIndex] = hash;
            }
            #endregion
        }
    }
}