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

namespace ActionCards
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
                            .AddBlockParser<CardMetadataBlock.Parser>()

                            .AddInlineParser<ItalicTextInline.ParserAsterix>()
                            .AddInlineParser<ItalicTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldTextInline.ParserAsterix>()
                            .AddInlineParser<BoldItalicTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldItalicTextInline.ParserAsterix>()

                            .Build();

            var txt = await System.IO.File.ReadAllTextAsync(a);

            doc.Parse(txt);

            var lastChangeTime = System.IO.File.GetLastWriteTime(a);

            // Create a MigraDoc document
            var document = CreateDocument(CardData.Create(doc).ToArray(), lastChangeTime);

            ////string ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
            //MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToFile(document, "MigraDoc.mdddl");

            //var renderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            //renderer.Document = document;

            //renderer.RenderDocument();

            // Save the document...
            var filename = System.IO.Path.ChangeExtension(a, ".pdf");
            document.Save(filename);
        }

        public static PdfDocument CreateDocument(IEnumerable<CardData> cards, DateTime fileChanged)
        {
            var pageWdith = XUnit.FromInch(2.5);
            var pageHeight = XUnit.FromInch(3.5);


            PdfDocument document = new PdfDocument();
            document.Info.Title = "Aktions Karten";
            document.Info.Subject = "Die Aktionskarten des spiels";
            document.Info.Author = "Arbeitstitel Karthago";
            document.Info.Keywords = "Karten, Aktion, Karthago";
            

            var maxOccurenceOfCard = cards.Max(x => x.Metadata.Times);
            int counter = 0;
            var total = cards.Sum(x => x.Metadata.Times);
            foreach (var card in cards)
            {

                for (int t = 0; t < card.Metadata.Times; t++)
                {
                    counter++;

                    PdfPage page = document.AddPage();

                    page.Width = new XUnit(pageWdith.Millimeter, XGraphicsUnit.Millimeter);
                    page.Height = new XUnit(pageHeight.Millimeter, XGraphicsUnit.Millimeter);


                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    // HACK²
                    gfx.MUH = PdfFontEncoding.Unicode;
                    //gfx.MFEH = PdfFontEmbedding.Default;

                    XFont font = new XFont("Verdana", 13, XFontStyle.Regular);



                    var costSize = new XSize(new XUnit(23, XGraphicsUnit.Millimeter), font.Height);

                    var costMarginRight = new XUnit(5, XGraphicsUnit.Millimeter);



                    var costRect = new XRect(pageWdith - costSize.Width - costMarginRight, new XUnit(5, XGraphicsUnit.Millimeter), costSize.Width, costSize.Height);

                    var actionRect = new XRect(costMarginRight, new XUnit(5, XGraphicsUnit.Millimeter), pageWdith - costMarginRight - costMarginRight, costSize.Height);

                    gfx.DrawRoundedRectangle(XPens.Red, XBrushes.IndianRed, actionRect, new XSize(10, 10));
                    gfx.DrawRoundedRectangle(XPens.Purple, XBrushes.MediumPurple, costRect, new XSize(10, 10));


                    gfx.DrawString($"{card.Metadata.Cost:n0} ¤", font, XBrushes.Black,
                      costRect, XStringFormats.CenterRight);

                    var actionTextRect = actionRect;
                    actionTextRect.Offset(new XUnit(3, XGraphicsUnit.Millimeter), 0);

                    gfx.DrawString($"Aktion", font, XBrushes.Black,
                      actionTextRect, XStringFormats.CenterLeft);

                    for (int i = 0; i < maxOccurenceOfCard; i++)
                    {
                        var rect = new XRect(new XUnit(3, XGraphicsUnit.Millimeter) + new XUnit(6, XGraphicsUnit.Millimeter) * i, pageHeight - new XUnit(8.5, XGraphicsUnit.Millimeter), new XUnit(3, XGraphicsUnit.Millimeter), new XUnit(3, XGraphicsUnit.Millimeter));

                        if (i + 1 <= card.Metadata.Times)
                            gfx.DrawEllipse(XBrushes.Green, rect);
                        else
                            gfx.DrawEllipse(XPens.Green, rect);
                    }


                    var dateRec = new XRect(new XUnit(3, XGraphicsUnit.Millimeter), pageHeight - new XUnit(2.5, XGraphicsUnit.Millimeter), new XUnit(3, XGraphicsUnit.Millimeter), new XUnit(3, XGraphicsUnit.Millimeter));
                    var dateFont = new XFont("Verdana", 7, XFontStyle.Regular);
                    gfx.DrawString(fileChanged.ToString(), dateFont, XBrushes.Gray, dateRec.TopLeft);
                    gfx.DrawString($"{counter}/{total}", dateFont, XBrushes.Gray, new XRect(0, 0, pageWdith - new XUnit(3, XGraphicsUnit.Millimeter), pageHeight - new XUnit(2.5, XGraphicsUnit.Millimeter)), XStringFormats.BottomRight);

                    // Create a new MigraDoc document
                    var doc = new Document();
                    doc.Info.Title = "Aktions Karten";
                    doc.Info.Subject = "Die Aktionskarten des spiels";
                    doc.Info.Author = "Arbeitstitel Karthago";


                    doc.DefaultPageSetup.PageWidth = new Unit(pageWdith.Inch, UnitType.Inch);
                    doc.DefaultPageSetup.PageHeight = new Unit(pageHeight.Inch, UnitType.Inch);

                    doc.DefaultPageSetup.LeftMargin = new Unit(5, UnitType.Millimeter);
                    doc.DefaultPageSetup.RightMargin = new Unit(5, UnitType.Millimeter);
                    doc.DefaultPageSetup.BottomMargin = new Unit(10, UnitType.Millimeter);
                    doc.DefaultPageSetup.TopMargin = new Unit(15, UnitType.Millimeter);

                    DefineStyles(doc);

                    //Cover.DefineCover(document);
                    //DefineTableOfContents(document);

                    DefineContentSection(doc);
                    HandleBlocks(card.Content, doc);


                    // Create a renderer and prepare (=layout) the document
                    MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(doc);
                    docRenderer.PrepareDocument();

                    //XRect rect = new XRect(new XPoint(Unit.FromCentimeter(1).Value, Unit.FromCentimeter(3).Value), new XSize((pageWdith.Value - Unit.FromCentimeter(2).Value), (pageHeight.Value - Unit.FromCentimeter(4).Value)));

                    // Use BeginContainer / EndContainer for simplicity only. You can naturaly use you own transformations.
                    //XGraphicsContainer container = gfx.BeginContainer(rect, A4Rect, XGraphicsUnit.Point);

                    // Draw page border for better visual representation
                    //gfx.DrawRectangle(XPens.LightGray, A4Rect);

                    // Render the page. Note that page numbers start with 1.
                    docRenderer.RenderPage(gfx, 1);

                    // Note: The outline and the hyperlinks (table of content) does not work in the produced PDF document.

                    // Pop the previous graphical state
                    //gfx.EndContainer(container);
                }
            }


            //DefineParagraphs(document);
            //DefineTables(document);
            //DefineCharts(document);

            return document;
        }

        public class CardData
        {
            private CardData(CardMetadataBlock metadata, IList<MarkdownBlock> content)
            {
                this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
                this.Content = content ?? throw new ArgumentNullException(nameof(content));
            }


            public CardMetadataBlock Metadata { get; }
            public IList<MarkdownBlock> Content { get; }



            public static IEnumerable<CardData> Create(MarkdownDocument doc)
            {

                var list = new List<MarkdownBlock>();
                foreach (var item in doc.Blocks)
                {
                    if (item is HorizontalRuleBlock)
                    {
                        try
                        {
                            var metadata = list.OfType<CardMetadataBlock>();
                            var metadataCount = metadata.Count();
                            if (metadataCount != 1)
                            {
                                var txt = string.Join("\n", list);
                                Console.Error.WriteLine(txt);

                                yield return new CardData(new CardMetadataBlock() { Cost = 9999, Times = 1 }
                                , new MarkdownBlock[]
                                {
                                    new HeaderBlock()
                                    {
                                         HeaderLevel =1,
                                        Inlines = new MarkdownInline[]
                                        {
                                            new TextRunInline()
                                            {
                                                Text = $"Parsing Error"
                                            }
                                        }
                                    },
                                    new ParagraphBlock()
                                    {
                                        Inlines = new MarkdownInline[]
                                        {
                                            new TextRunInline()
                                            {
                                                Text = $"There must be exactly 1 {nameof(CardMetadataBlock)} but were {metadataCount}"
                                            }
                                        }
                                    },
                                    new ParagraphBlock()
                                    {
                                        Inlines = new MarkdownInline[]
                                        {
                                            new TextRunInline()
                                            {
                                                Text = txt
                                            }
                                        }
                                    }
                                });


                                continue;
                            }

                            yield return new CardData(metadata.First(), list.Where(x => !(x is CardMetadataBlock)).ToArray());
                        }
                        finally
                        {
                            list.Clear();
                        }
                    }
                    else
                    {
                        list.Add(item);
                    }
                }
                {
                    // for the last item wo also want to handle it.
                    var metadata = list.OfType<CardMetadataBlock>();
                    if (metadata.Count() != 1)
                    {
                        Console.WriteLine(string.Join("\n", list));
                        yield break;
                    }

                    yield return new CardData(metadata.First(), list.Where(x => !(x is CardMetadataBlock)).ToArray());
                }
            }
        }

        public class CardMetadataBlock : MarkdownBlock
        {

            public int Times { get; set; }
            public int Cost { get; set; }

            public new class Parser : Parser<CardMetadataBlock>
            {
                protected override BlockParseResult<CardMetadataBlock> ParseInternal(LineBlock markdown, int startLine, bool lineStartsNewParagraph, MarkdownDocument document)
                {
                    var line = markdown[startLine].Trim();

                    if (line.Length == 0 || line[0] != '>')
                        return null;

                    line = line.Slice(1).TrimStart();

                    var list = new List<uint>();

                    uint currentCost = 0;

                    int? times = null;
                    int? cost = null;

                    while (line.Length != 0)
                    {


                        var end = line.IndexOfNexWhiteSpace();
                        if (end == -1)
                            end = line.Length;

                        var current = line.Slice(0, end).Trim();


                        var type = current[^1];

                        if (type != 'x' && type != '$')
                            return null;

                        if (type == 'x')
                        {
                            if (times.HasValue)
                                return null;
                            if (!int.TryParse(current.Slice(0, current.Length - 1), out var value))
                                return null;

                            times = value;
                        }
                        else if (type == '$')
                        {
                            if (cost.HasValue)
                                return null;
                            if (!int.TryParse(current.Slice(0, current.Length - 1), System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("de"), out var value))
                                return null;

                            cost = value;
                        }


                        line = line.Slice(end).Trim();
                    }

                    if (cost is null || times is null)
                        return null;

                    var result = new CardMetadataBlock()
                    {
                        Cost = cost.Value,
                        Times = times.Value,
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
                        //case CardMetadataBlock w:
                        //    MakeTable(paragraph, w, document);
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

        //private static void MakeTable(Paragraph paragraph, CardMetadataBlock w, Document document)
        //{

        //    var numberOfColumns = Math.Min(6, w.WorkerCosts.Count);


        //    var table = new Table();
        //    table.Style = "table";
        //    table.Format.Alignment = ParagraphAlignment.Center;
        //    table.Format.SpaceAfter = new Unit(1, UnitType.Centimeter);
        //    table.Format.WidowControl = true;

        //    //table.BottomPadding = new Unit(1, UnitType.Centimeter);

        //    table.Borders.Width = 0.75;

        //    for (var i = 0; i < numberOfColumns; i++)
        //    {
        //        var column = table.AddColumn(Unit.FromCentimeter(1.5));
        //        column.Format.Alignment = ParagraphAlignment.Center;
        //    }

        //    Row row = null;


        //    for (var i = 0; i < w.WorkerCosts.Count; i++)
        //    {
        //        if (row is null)
        //        {
        //            row = table.AddRow();
        //            row.Height = Unit.FromCentimeter(1.5);
        //        }

        //        var cell = row.Cells[i % numberOfColumns];
        //        cell.AddParagraph(w.WorkerCosts[i].ToString());


        //        if (i % numberOfColumns == numberOfColumns - 1)
        //            row = null;

        //    }

        //    if (row != null)
        //        for (int i = 0; i < numberOfColumns - (w.WorkerCosts.Count % numberOfColumns); i++)
        //        {
        //            var cell = row.Cells[numberOfColumns - 1 - i];
        //            cell.Shading.Color = Colors.Black;
        //        }

        //    table.SetEdge(0, 0, numberOfColumns, table.Rows.Count, Edge.Box, BorderStyle.Single, 1.5, Colors.Black);

        //    document.LastSection.Add(table);
        //}

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
            //section.PageSetup.OddAndEvenPagesHeaderFooter = false;
            //section.PageSetup.StartingNumber = 1;

            //HeaderFooter header = section.Headers.Primary;
            //header.AddParagraph("\tOdd Page Header");

            //header = section.Headers.EvenPage;
            //header.AddParagraph("Even Page Header");

            // Create a paragraph with centered page number. See definition of style "Footer".
            //var paragraph = new Paragraph();
            //paragraph.AddDateField();
            //paragraph.AddTab();
            //paragraph.AddPageField();

            //// Add paragraph to footer for odd pages.
            //section.Footers.Primary.Add(paragraph);
            //// Add clone of paragraph to footer for odd pages. Cloning is necessary because an object must
            //// not belong to more than one other object. If you forget cloning an exception is thrown.
            //section.Footers.EvenPage.Add(paragraph.Clone());
        }
    }
}
