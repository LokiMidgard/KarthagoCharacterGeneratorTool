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

namespace CrisesCards
{

    public class CirsisGeneator
    {

        private static int count;

        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.WriteLine($"Will Work on {args.Length} Jobs in parrallel.");

            await Task.WhenAll(args.Select(a =>
            {
                var filename = System.IO.Path.ChangeExtension(a, ".pdf");

                return GenerateDocument(a, filename);
            }));
        }

        public static async Task<(string crisisType, int startIndex)[]> GenerateDocument(string input, string output)
        {
            var currentInstance = System.Threading.Interlocked.Increment(ref count);

            var doc = Markdown.GetDefaultMarkdownDowcument();

            var txt = await System.IO.File.ReadAllTextAsync(input);

            doc.Parse(txt);

            var lastChangeTime = System.IO.File.GetLastWriteTime(input);

            // Create a MigraDoc document
            var cards = CardData.Create(doc).ToArray();
            Console.WriteLine($"{currentInstance}: Found {cards.Length} cards.");
            var (document, metadata) = CreateDocument(cards, lastChangeTime, currentInstance);

            ////string ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
            //MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToFile(document, "MigraDoc.mdddl");

            //var renderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            //renderer.Document = document;

            //renderer.RenderDocument();

            // Save the document...
            Console.WriteLine($"{currentInstance}: Saving {output}...");
            document.Save(output);
            Console.WriteLine($"{currentInstance}: Finished!");
            return metadata;
        }



        public static (PdfDocument document, (string crisisType, int startIndex)[]) CreateDocument(IEnumerable<CardData> cards, DateTime fileChanged, int currentInstance)
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
            var total = cards.Sum(x => x.Metadata.Times);
            string currentCardType = null;
            var resultList = new List<(string crisisType, int startIndex)>();
            foreach (var card in cards)
            {
                if (card.Metadata.Type != null && currentCardType != null)
                {
                    // Create new Back
                    CrateBack(pageWdith, pageHeight, document, currentCardType);
                }

                if (card.Metadata.Type != null)
                {
                    resultList.Add((card.Metadata.Type, document.PageCount));
                }

                currentCardType = card.Metadata.Type ?? currentCardType;
                if (currentCardType is null)
                {
                    Console.WriteLine($"{currentInstance}: Did not found card metadata. SKIP");
                    continue;
                }
                var header = card.Content.FirstOrDefault(x => x is HeaderBlock);
                Console.WriteLine($"{currentInstance}: Working on <{header?.ToString() ?? "UNKNOWN TITLE"}> with {card.Metadata.Times} instances.");
                for (int i = 0; i < card.Metadata.Times; i++)
                {
                    Console.Write($"{i + 1}...");
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




                    var actionRect = new XRect(costMarginRight, new XUnit(5, XGraphicsUnit.Millimeter), pageHeight * 2, costSize.Height * 2);
                    var actionTextRect = actionRect;
                    actionTextRect.Height = costSize.Height;
                    actionTextRect.Offset(new XUnit(3, XGraphicsUnit.Millimeter), 0);

                    gfx.TranslateTransform(new XUnit(3, XGraphicsUnit.Millimeter), 0);
                    gfx.RotateAtTransform(90, actionRect.TopLeft);



                    gfx.DrawRoundedRectangle(XPens.MidnightBlue, XBrushes.DarkSlateBlue, actionRect, new XSize(10, 10));
                    gfx.DrawString(currentCardType, font, XBrushes.White,
                    actionTextRect, XStringFormats.CenterLeft);

                    gfx.RotateAtTransform(-90, actionRect.TopLeft);
                    gfx.TranslateTransform(new XUnit(-3, XGraphicsUnit.Millimeter), 0);

                    var circle = new XRect(new XUnit(-3, XGraphicsUnit.Millimeter), pageHeight - new XUnit(10, XGraphicsUnit.Millimeter), new XUnit(13, XGraphicsUnit.Millimeter), new XUnit(13, XGraphicsUnit.Millimeter));

                    gfx.DrawEllipse(XPens.MidnightBlue, XBrushes.White, circle);

                    gfx.DrawString($"{card.Metadata.Duration:n0}", font, XBrushes.Black,
                        circle, XStringFormats.Center);


                    var dateRec = new XRect(new XUnit(13, XGraphicsUnit.Millimeter), pageHeight - new XUnit(2.5, XGraphicsUnit.Millimeter), new XUnit(13, XGraphicsUnit.Millimeter), new XUnit(3, XGraphicsUnit.Millimeter));
                    var dateFont = new XFont("Verdana", 7, XFontStyle.Regular);
                    gfx.DrawString(fileChanged.ToString(), dateFont, XBrushes.Gray, dateRec.TopLeft);
                    gfx.DrawString($"{counter}/{total}", dateFont, XBrushes.Gray, new XRect(0, 0, pageWdith - new XUnit(3, XGraphicsUnit.Millimeter), pageHeight - new XUnit(2.5, XGraphicsUnit.Millimeter)), XStringFormats.BottomRight);

                    // Create a new MigraDoc document
                    var doc = new Document();
                    doc.Info.Title = "Krisen Karten";
                    doc.Info.Subject = "Die Krisenkarten des spiels";
                    doc.Info.Author = "Arbeitstitel Karthago";


                    doc.DefaultPageSetup.PageWidth = new Unit(pageWdith.Inch, UnitType.Inch);
                    doc.DefaultPageSetup.PageHeight = new Unit(pageHeight.Inch, UnitType.Inch);

                    doc.DefaultPageSetup.LeftMargin = new Unit(13, UnitType.Millimeter);
                    doc.DefaultPageSetup.RightMargin = new Unit(5, UnitType.Millimeter);
                    doc.DefaultPageSetup.BottomMargin = new Unit(10, UnitType.Millimeter);
                    doc.DefaultPageSetup.TopMargin = new Unit(6, UnitType.Millimeter);

                    doc.DefineStyles();

                    //Cover.DefineCover(document);
                    //DefineTableOfContents(document);

                    DefineContentSection(doc);
                    card.Content.HandleBlocks(doc);


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
                Console.WriteLine(" Finished card.");
            }
            CrateBack(pageWdith, pageHeight, document, currentCardType);


            //DefineParagraphs(document);
            //DefineTables(document);
            //DefineCharts(document);

            return (document, resultList.ToArray());
        }

        private static void CrateBack(XUnit pageWdith, XUnit pageHeight, PdfDocument document, string currentCardType)
        {
            PdfPage page = document.AddPage();

            page.Width = new XUnit(pageWdith.Millimeter, XGraphicsUnit.Millimeter);
            page.Height = new XUnit(pageHeight.Millimeter, XGraphicsUnit.Millimeter);


            XGraphics gfx = XGraphics.FromPdfPage(page);
            // HACK²
            gfx.MUH = PdfFontEncoding.Unicode;
            //gfx.MFEH = PdfFontEmbedding.Default;

            gfx.ScaleAtTransform(-1, 1, page.Width / 2, page.Height / 2);

            XFont font = new XFont("Verdana", 13, XFontStyle.Regular);



            var costSize = new XSize(new XUnit(23, XGraphicsUnit.Millimeter), font.Height);

            var costMarginRight = new XUnit(5, XGraphicsUnit.Millimeter);




            var actionRect = new XRect(costMarginRight, new XUnit(5, XGraphicsUnit.Millimeter), pageHeight * 2, costSize.Height * 2);
            var actionTextRect = actionRect;
            actionTextRect.Height = costSize.Height;
            actionTextRect.Offset(new XUnit(3, XGraphicsUnit.Millimeter), 0);

            gfx.TranslateTransform(new XUnit(3, XGraphicsUnit.Millimeter), 0);
            gfx.RotateAtTransform(90, actionRect.TopLeft);

            gfx.DrawRoundedRectangle(XPens.MidnightBlue, XBrushes.DarkSlateBlue, actionRect, new XSize(10, 10));

            gfx.ScaleAtTransform(1, -1, actionTextRect.Center);

            gfx.DrawString(currentCardType, font, XBrushes.White,
            actionTextRect, XStringFormats.TopLeft);
            gfx.ScaleAtTransform(1, -1, actionTextRect.Center);


            gfx.RotateAtTransform(-90, actionRect.TopLeft);
            gfx.TranslateTransform(new XUnit(-3, XGraphicsUnit.Millimeter), 0);

            var circle = new XRect(new XUnit(-3, XGraphicsUnit.Millimeter), pageHeight - new XUnit(10, XGraphicsUnit.Millimeter), new XUnit(13, XGraphicsUnit.Millimeter), new XUnit(13, XGraphicsUnit.Millimeter));

            gfx.DrawEllipse(XPens.MidnightBlue, XBrushes.White, circle);


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
