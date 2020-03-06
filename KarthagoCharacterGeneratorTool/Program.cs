using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Helpers;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
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

            var doc = MarkdownDocument.CreateBuilder()
                            .AddBlockParser<HeaderBlock.HashParser>()
                            .AddBlockParser<ListBlock.Parser>()
                            .AddBlockParser<HorizontalRuleBlock.Parser>()
                            .AddBlockParser<WorkerBlock.Parser>()

                            .AddInlineParser<ItalicTextInline.ParserAsterix>()
                            .AddInlineParser<ItalicTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldTextInline.ParserAsterix>()
                            .AddInlineParser<BoldItalicTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldItalicTextInline.ParserAsterix>()

                            .Build();

            var txt = await System.IO.File.ReadAllTextAsync(a);

            doc.Parse(txt);

            // Create a MigraDoc document
            var document = CreateDocument(doc);

            //string ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
            MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToFile(document, "MigraDoc.mdddl");

            var renderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
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
            document.Info.Title = "Hello, MigraDoc";
            document.Info.Subject = "Demonstrates an excerpt of the capabilities of MigraDoc.";
            document.Info.Author = "Stefan Lange";

            document.DefaultPageSetup.PageFormat = PageFormat.A6;

            PageSetup.GetPageSize(PageFormat.A6, out var width, out var height);
            document.DefaultPageSetup.PageWidth = width;
            document.DefaultPageSetup.PageHeight = height;

            document.DefaultPageSetup.LeftMargin = new Unit(0.6, UnitType.Centimeter);
            document.DefaultPageSetup.RightMargin = new Unit(0.6, UnitType.Centimeter);
            document.DefaultPageSetup.BottomMargin = new Unit(0.6, UnitType.Centimeter);
            document.DefaultPageSetup.TopMargin = new Unit(0.6, UnitType.Centimeter);

            DefineStyles(document);

            //Cover.DefineCover(document);
            //DefineTableOfContents(document);

            DefineContentSection(document);

            HandleBlocks(doc.Blocks, document);

            //DefineParagraphs(document);
            //DefineTables(document);
            //DefineCharts(document);

            return document;
        }


        public class WorkerBlock : MarkdownBlock
        {
            public IList<uint> WorkerCosts { get; set; }

            public new class Parser : Parser<WorkerBlock>
            {
                protected override BlockParseResult<WorkerBlock> ParseInternal(LineBlock markdown, int startLine, bool lineStartsNewParagraph, MarkdownDocument document)
                {
                    var line = markdown[startLine].Trim();

                    if (line.Length == 0 || line[0] != '>')
                        return null;

                    line = line.Slice(1).TrimStart();

                    var list = new List<uint>();

                    uint currentCost = 0;

                    while (line.Length != 0)
                    {


                        var end = line.IndexOfNexWhiteSpace();
                        if (end == -1)
                            end = line.Length;

                        var current = line.Slice(0, end).Trim();

                        var splitter = current.IndexOf(':');
                        if (splitter == -1)
                            return null;

                        var first = current.Slice(0, splitter);
                        var scccond = current.Slice(splitter + 1);

                        if (!uint.TryParse(first, out var count))
                            return null;
                        if (!uint.TryParse(scccond, out var cost))
                            return null;

                        for (var i = 0; i < count; i++)
                        {
                            currentCost += cost;
                            list.Add(currentCost);
                        }

                        line = line.Slice(end).Trim();
                    }

                    var result = new WorkerBlock()
                    {
                        WorkerCosts = list.AsReadOnly()
                    };
                    return BlockParseResult.Create(result, startLine, 1);
                }
            }

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
                        MakeTable(paragraph, w, document);
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

        private static void MakeTable(Paragraph paragraph, WorkerBlock w, Document document)
        {

            var numberOfColumns = Math.Min(6, w.WorkerCosts.Count);


            var table = new Table();
            table.Style = "table";
            table.Format.Alignment = ParagraphAlignment.Center;
            table.Format.SpaceAfter = new Unit(1, UnitType.Centimeter);
            table.Format.WidowControl = true;

            //table.BottomPadding = new Unit(1, UnitType.Centimeter);

            table.Borders.Width = 0.75;

            for (var i = 0; i < numberOfColumns; i++)
            {
                var column = table.AddColumn(Unit.FromCentimeter(1.5));
                column.Format.Alignment = ParagraphAlignment.Center;
            }

            Row row = null;


            for (var i = 0; i < w.WorkerCosts.Count; i++)
            {
                if (row is null)
                {
                    row = table.AddRow();
                    row.Height = Unit.FromCentimeter(1.5);
                }

                var cell = row.Cells[i % numberOfColumns];
                cell.AddParagraph(w.WorkerCosts[i].ToString());


                if (i % numberOfColumns == numberOfColumns - 1)
                    row = null;

            }

            if (row != null)
                for (int i = 0; i < numberOfColumns - (w.WorkerCosts.Count % numberOfColumns); i++)
                {
                    var cell = row.Cells[numberOfColumns - 1 - i];
                    cell.Shading.Color = Colors.Black;
                }

            table.SetEdge(0, 0, numberOfColumns, table.Rows.Count, Edge.Box, BorderStyle.Single, 1.5, Colors.Black);

            document.LastSection.Add(table);
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

            FillInlines(paragraph, header.Inlines);
        }
        private static void MakeParagraph(Paragraph paragraph, ParagraphBlock paragraphBlock, Document document)
        {
            FillInlines(paragraph, paragraphBlock.Inlines);
        }

        private static void FillInlines(Paragraph paragraph, IList<MarkdownInline> inlines, TextFormat format = default)
        {
            foreach (var inline in inlines)
            {
                switch (inline)
                {
                    case TextRunInline text:
                        paragraph.AddFormattedText(text.Text, format);
                        break;
                    case BoldTextInline text:
                        FillInlines(paragraph, text.Inlines, format | TextFormat.Bold);
                        break;
                    case ItalicTextInline text:
                        FillInlines(paragraph, text.Inlines, format | TextFormat.Italic);
                        break;
                    case BoldItalicTextInline text:
                        FillInlines(paragraph, text.Inlines, format | TextFormat.Bold | TextFormat.Italic);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Defines the styles used in the document.
        /// </summary>
        public static void DefineStyles(Document document)
        {
            // Get the predefined style Normal.
            var style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Times New Roman";

            // Heading1 to Heading9 are predefined styles with an outline level. An outline level
            // other than OutlineLevel.BodyText automatically creates the outline (or bookmarks) 
            // in PDF.

            style = document.Styles["Heading1"];
            style.Font.Name = "Tahoma";
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.Font.Color = Colors.DarkBlue;
            style.ParagraphFormat.PageBreakBefore = false;
            style.ParagraphFormat.SpaceAfter = 6;

            style = document.Styles["Heading2"];
            style.Font.Size = 12;
            style.Font.Bold = true;
            style.ParagraphFormat.PageBreakBefore = false;
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 6;

            style = document.Styles["Heading3"];
            style.Font.Size = 10;
            style.Font.Bold = true;
            style.Font.Italic = true;
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 3;

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called TextBox based on style Normal
            style = document.Styles.AddStyle("TextBox", "Normal");
            style.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
            style.ParagraphFormat.Borders.Width = 2.5;
            style.ParagraphFormat.Borders.Distance = "3pt";
            style.ParagraphFormat.Shading.Color = Colors.SkyBlue;

            // Create a new style called TOC based on style Normal
            style = document.Styles.AddStyle("TOC", "Normal");
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);
            style.ParagraphFormat.Font.Color = Colors.Blue;

            // Create a new style called TOC based on style Normal
            style = document.Styles.AddStyle("table", "Normal");
            style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            // Bullit
            var unorderedlist = document.AddStyle("UnorderedList", "Normal");
            var listInfo = new ListInfo();
            listInfo.ListType = ListType.BulletList1;
            unorderedlist.ParagraphFormat.ListInfo = listInfo;
            unorderedlist.ParagraphFormat.LeftIndent = "1cm";
            unorderedlist.ParagraphFormat.FirstLineIndent = "-0.5cm";
            unorderedlist.ParagraphFormat.SpaceAfter = 0;

            var orderedlist = document.AddStyle("OrderedList", "UnorderedList");
            orderedlist.ParagraphFormat.ListInfo.ListType = ListType.NumberList1;

            // for list spacing (since MigraDoc doesn't provide a list object that we can target)
            var listStart = document.AddStyle("ListStart", "Normal");
            listStart.ParagraphFormat.SpaceAfter = 0;
            listStart.ParagraphFormat.LineSpacing = 0.5;
            var listEnd = document.AddStyle("ListEnd", "ListStart");
            listEnd.ParagraphFormat.LineSpacing = 1;

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