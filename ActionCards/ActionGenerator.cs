﻿using Microsoft.Toolkit.Parsers.Markdown;
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

namespace ActionCards
{
    public static class ActionGenerator
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            await Task.WhenAll(args.Select(a =>
            {
                var filename = System.IO.Path.ChangeExtension(a, ".pdf");
                return GenerateDocument(a, filename);
            }));
        }

        public static async Task GenerateDocument(string input, string output)
        {
            var doc = Markdown.GetDefaultMarkdownDowcument();
            var txt = await System.IO.File.ReadAllTextAsync(input);

            doc.Parse(txt);

            var lastChangeTime = System.IO.File.GetLastWriteTime(input);

            // Create a MigraDoc document
            var document = CreateDocument(CardData.Create(doc).ToArray(), lastChangeTime);

            // Save the document...
            document.Save(output);
        }

        private static PdfDocument CreateDocument(IEnumerable<CardData> cards, DateTime fileChanged)
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

            }
            // card back
            {
                PdfPage page = document.AddPage();

                page.Width = new XUnit(pageWdith.Millimeter, XGraphicsUnit.Millimeter);
                page.Height = new XUnit(pageHeight.Millimeter, XGraphicsUnit.Millimeter);


                XGraphics gfx = XGraphics.FromPdfPage(page);
                // HACK²
                gfx.MUH = PdfFontEncoding.Unicode;
                //gfx.MFEH = PdfFontEmbedding.Default;

                XFont font = new XFont("Verdana", 36, XFontStyle.Regular);
                var costSize = new XSize(new XUnit(23, XGraphicsUnit.Millimeter), font.Height);
                var costMarginRight = new XUnit(5, XGraphicsUnit.Millimeter);
                var titleRect = new XRect(0, 0, pageWdith, pageHeight);
                gfx.DrawString($"Aktion", font, XBrushes.DarkRed,
                  titleRect, XStringFormats.Center);
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
