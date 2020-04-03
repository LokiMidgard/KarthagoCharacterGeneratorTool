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
    public class CharacterGenerator
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

            // Create a MigraDoc document
            var document = CreateDocument(doc);

            //string ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
            MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToFile(document, "MigraDoc.mdddl");

            var renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;

            renderer.RenderDocument();

            // Save the document...
            renderer.PdfDocument.Save(output);
        }

        private static Document CreateDocument(MarkdownDocument doc)
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

            doc.Blocks.HandleBlocks(document);

            //DefineParagraphs(document);
            //DefineTables(document);
            //DefineCharts(document);

            return document;
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