using Microsoft.Toolkit.Parsers.Markdown;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using Parsers;
using System;
using System.Threading.Tasks;

namespace AssetGenerator
{
    internal class RuleGenerator
    {
        
        public static async Task GenerateDocument(string input, string output)
        {
            var doc = new  MarkdownDocument().GetBuilder().RemoveInlineParser<Microsoft.Toolkit.Parsers.Markdown.Inlines.EmojiInline.Parser>().AddIcons().Build();

            var txt = await System.IO.File.ReadAllTextAsync(input);

            doc.Parse(txt);

            var lastChangeTime = System.IO.File.GetLastWriteTime(input);

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

            document.DefaultPageSetup.PageFormat = PageFormat.A5;

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
        /// <summary>
        /// Defines the styles used in the document.
        /// </summary>
        static void DefineContentSection(Document document)
        {
            var section = document.AddSection();

        }
    }
}