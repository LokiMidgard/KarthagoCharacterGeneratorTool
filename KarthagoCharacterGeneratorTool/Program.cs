using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Helpers;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Parsers;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KarthagoCharacterGeneratorTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            await Task.WhenAll(args.Select(a => GenerateDocument(a)));
        }

        private static async Task GenerateDocument(string a)
        {

            var doc = Markdown.GetDefaultMarkdownDowcument();

            var txt = await System.IO.File.ReadAllTextAsync(a);

            doc.Parse(txt);

            // Create a MigraDoc document
            var document = CreateDocument(doc);

            //string ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
            MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToFile(document, "MigraDoc.mdddl");

            var renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;

            renderer.RenderDocument();

            // Save the document...
            var filename = System.IO.Path.ChangeExtension(a, ".pdf");
            renderer.PdfDocument.Save(filename);
        }

        public static Document CreateDocument(MarkdownDocument doc)
        {
            // Create a new MigraDoc document
            var document = new Document();
            document.Info.Title = "Charaktere Karten";
            document.Info.Subject = "Die Charakterkarten des spiels";
            document.Info.Author = "Arbeitstitel Karthago";
            document.Info.Keywords = "Karten, Charakter, Karthago";

            document.DefaultPageSetup.PageFormat = PageFormat.A6;

            PageSetup.GetPageSize(PageFormat.A6, out var width, out var height);
            document.DefaultPageSetup.PageWidth = width;
            document.DefaultPageSetup.PageHeight = height;

            document.DefaultPageSetup.LeftMargin = new Unit(0.6, UnitType.Centimeter);
            document.DefaultPageSetup.RightMargin = new Unit(0.6, UnitType.Centimeter);
            document.DefaultPageSetup.BottomMargin = new Unit(0.6, UnitType.Centimeter);
            document.DefaultPageSetup.TopMargin = new Unit(0.6, UnitType.Centimeter);

            document.DefineStyles();

            //Cover.DefineCover(document);
            //DefineTableOfContents(document);

            DefineContentSection(document);

            HandleBlocks(doc.Blocks, document);

            //DefineParagraphs(document);
            //DefineTables(document);
            //DefineCharts(document);

            return document;
        }



        private static void HandleBlocks(IEnumerable<MarkdownBlock> blocks, Document document, Paragraph paragraph = null)
        {
            bool lineBreak = false;

            foreach (var block in blocks)
            {
                if (lineBreak)
                {
                    document.LastSection.AddPageBreak();
                    lineBreak = false;
                }
                paragraph ??= document.LastSection.AddParagraph();
                switch (block)
                {
                    case ParagraphBlock p:
                        MakeParagraph(paragraph, p, document);
                        break;
                    case HeaderBlock header:
                        MakeHeader(paragraph, header, document);
                        break;
                    case ListBlock list:
                        MakeList(paragraph, list, document);
                        break;
                    case WorkerBlock w:
                        w.MakeWorkerTable(paragraph, document);
                        break;
                    case HorizontalRuleBlock hr:
                        lineBreak = true;
                        break;
                    default:
                        break;
                }
                paragraph = null;
            }
        }



        private static void MakeList(Paragraph paragraph, ListBlock list, Document document)
        {
            for (var i = 0; i < list.Items.Count; i++)
            {
                paragraph ??= document.LastSection.AddParagraph();

                var listStyle = list.Style == ListStyle.Bulleted
                    ? "UnorderedList"
                    : "OrderedList";

                //var section = (Section)parent;
                var isFirst = i == 0;
                var isLast = i == list.Items.Count - 1;

                // if this is the first item add the ListStart paragraph
                //if (isFirst)
                //{
                //    var p = section.AddParagraph();
                //    p.Style = "ListStart";
                //}

                var listItem = paragraph;
                listItem.Style = listStyle;

                var current = list.Items[i];
                HandleBlocks(current.Blocks, document, paragraph);


                // disable continuation if this is the first list item
                listItem.Format.ListInfo.ContinuePreviousList = !isFirst;

                // if the this is the last item add the ListEnd paragraph
                //if (isLast)
                //{
                //    var p = section.AddParagraph();
                //    p.Style = "ListEnd";
                //}
                paragraph = null;
            }




        }

        private static void MakeHeader(Paragraph paragraph, HeaderBlock header, Document document)
        {
            paragraph.Style = $"Heading{header.HeaderLevel}";
            paragraph.AddBookmark("Paragraphs");

            header.Inlines.FillInlines(paragraph);
        }
        private static void MakeParagraph(Paragraph paragraph, ParagraphBlock paragraphBlock, Document document)
        {
            paragraphBlock.Inlines.FillInlines(paragraph);
        }

        static void DefineContentSection(Document document)
        {
            var section = document.AddSection();
            section.PageSetup.OddAndEvenPagesHeaderFooter = false;
            section.PageSetup.StartingNumber = 1;

            //HeaderFooter header = section.Headers.Primary;
            //header.AddParagraph("\tOdd Page Header");

            //header = section.Headers.EvenPage;
            //header.AddParagraph("Even Page Header");

            // Create a paragraph with centered page number. See definition of style "Footer".
            var paragraph = new Paragraph();
            paragraph.AddDateField();
            paragraph.AddTab();
            paragraph.AddPageField();

            // Add paragraph to footer for odd pages.
            section.Footers.Primary.Add(paragraph);
            //// Add clone of paragraph to footer for odd pages. Cloning is necessary because an object must
            //// not belong to more than one other object. If you forget cloning an exception is thrown.
            //section.Footers.EvenPage.Add(paragraph.Clone());
        }
    }
}