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
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SienceCards
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
            document.Info.Title = "Forschungs Karten";
            document.Info.Subject = "Die Forschungskarten des spiels";
            document.Info.Author = "Arbeitstitel Karthago";
            document.Info.Keywords = "Karten, Forschung, Karthago";


            //var maxOccurenceOfCard = cards.Max(x => x.Metadata.Times);
            int counter = 0;
            var total = cards.Count();
            foreach (var card in cards)
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
                var durationRect = costRect;
                durationRect.Height *= 2.1;

                gfx.DrawRoundedRectangle(XPens.RoyalBlue, XBrushes.LightBlue, actionRect, new XSize(10, 10));
                gfx.DrawRoundedRectangle(XPens.Orange, XBrushes.LightYellow, durationRect, new XSize(10, 10));
                gfx.DrawRoundedRectangle(XPens.Purple, XBrushes.MediumPurple, costRect, new XSize(10, 10));


                var costTextRect = costRect;
                costTextRect.Width -= new XUnit(1, XGraphicsUnit.Millimeter);
                gfx.DrawString($"{card.Metadata.Cost:n0} ¤", font, XBrushes.Black,
                  costTextRect, XStringFormats.CenterRight);

                var subfont = Markdown.GetSubstituteFont("⌛");
                subfont = new XFont(subfont.Name, font.Size);

                var durationTextRect = durationRect;
                durationTextRect.Width -= new XUnit(1, XGraphicsUnit.Millimeter);

                gfx.DrawString($"{card.Metadata.Duration:n0} ⌛", subfont, XBrushes.Black,
                    durationTextRect, XStringFormats.BottomRight);


                var actionTextRect = actionRect;
                actionTextRect.Offset(new XUnit(3, XGraphicsUnit.Millimeter), 0);

                gfx.DrawString($"Forschung", font, XBrushes.Black,
                  actionTextRect, XStringFormats.CenterLeft);




                var dateRec = new XRect(new XUnit(3, XGraphicsUnit.Millimeter), pageHeight - new XUnit(2.5, XGraphicsUnit.Millimeter), new XUnit(3, XGraphicsUnit.Millimeter), new XUnit(3, XGraphicsUnit.Millimeter));
                var dateFont = new XFont("Verdana", 7, XFontStyle.Regular);
                gfx.DrawString(fileChanged.ToString(), dateFont, XBrushes.Gray, dateRec.TopLeft);
                gfx.DrawString($"{counter}/{total}", dateFont, XBrushes.Gray, new XRect(0, 0, pageWdith - new XUnit(3, XGraphicsUnit.Millimeter), pageHeight - new XUnit(2.5, XGraphicsUnit.Millimeter)), XStringFormats.BottomRight);

                // Create a new MigraDoc document
                var doc = new Document();
                doc.Info.Title = "Forschungs Karten";
                doc.Info.Subject = "Die Forschungskarten des spiels";
                doc.Info.Author = "Arbeitstitel Karthago";


                doc.DefaultPageSetup.PageWidth = new Unit(pageWdith.Inch, UnitType.Inch);
                doc.DefaultPageSetup.PageHeight = new Unit(pageHeight.Inch, UnitType.Inch);

                doc.DefaultPageSetup.LeftMargin = new Unit(5, UnitType.Millimeter);
                doc.DefaultPageSetup.RightMargin = new Unit(5, UnitType.Millimeter);
                doc.DefaultPageSetup.BottomMargin = new Unit(10, UnitType.Millimeter);
                doc.DefaultPageSetup.TopMargin = new Unit(16, UnitType.Millimeter);

                doc.DefineStyles();

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
                        w.MakeWorkerTable(paragraph, document, Unit.FromMillimeter(8));
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

            header.Inlines.FillInlines(paragraph);
        }
        private static void MakeParagraph(Paragraph paragraph, ParagraphBlock paragraphBlock, Document document)
        {
            paragraphBlock.Inlines.FillInlines(paragraph);
        }

        /// <summary>
        /// Defines the styles used in the document.
        /// </summary>


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
