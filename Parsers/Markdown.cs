using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System.Collections.Generic;
using System.Linq;

namespace Parsers
{
    public static class Markdown
    {

        public static readonly FontResolver fontResolver;
        static Markdown()
        {
            fontResolver = new Parsers.FontResolver();
            GlobalFontSettings.FontResolver = fontResolver;
        }


        public static void FillInlines(this IEnumerable<MarkdownInline> inlines, Paragraph paragraph, TextFormat format = default)
        {
            foreach (var inline in inlines)
            {
                switch (inline)
                {
                    case TextRunInline text:
                        paragraph.AddFormattedText(text.Text, format);
                        break;
                    case EmojiInline text:
                        text.Make(paragraph, format);
                        break;
                    case BoldTextInline text:
                        text.Inlines.FillInlines(paragraph, format | TextFormat.Bold);
                        break;
                    case ItalicTextInline text:
                        text.Inlines.FillInlines(paragraph, format | TextFormat.Italic);
                        break;
                    case BoldItalicTextInline text:
                        text.Inlines.FillInlines(paragraph, format | TextFormat.Bold | TextFormat.Italic);
                        break;
                    default:
                        break;
                }
            }
        }
        public static void DefineStyles(this Document document)
        {
            // Get the predefined style Normal.
            var style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;
            style.ParagraphFormat.SpaceAfter = 3;

            // Create a new style called NoSpacing based on style Normal
            style = document.Styles.AddStyle("NoSpacing", "Normal");
            style.ParagraphFormat.SpaceAfter = 0;
            style.ParagraphFormat.SpaceBefore = 0;
            style.ParagraphFormat.LineSpacing = 0;

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

        public static void Make(this EmojiInline emoji, Paragraph paragraph, TextFormat format = default)
        {
            var substituteFont = GetSubstituteFont(emoji.Text);

            if (substituteFont != null)
                paragraph.AddFormattedText(emoji.Text, new Font(substituteFont.FontFamily.Name));
            else
                paragraph.AddFormattedText(emoji.Text, format);

        }

        public static void MakeList(this ListBlock list, Paragraph paragraph, Document document)
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

        public static void MakeHeader(this HeaderBlock header, Paragraph paragraph, Document document)
        {
            paragraph.Style = $"Heading{header.HeaderLevel}";
            paragraph.AddBookmark("Paragraphs");

            header.Inlines.FillInlines(paragraph);
        }
        public static void MakeParagraph(this ParagraphBlock paragraphBlock, Paragraph paragraph, Document document)
        {
            paragraphBlock.Inlines.FillInlines(paragraph);
        }

        public static void HandleBlocks(this IEnumerable<MarkdownBlock> blocks, Document document, Paragraph paragraph = null)
        {
            bool lineBreak = false;

            foreach (var block in blocks)
            {
                if (lineBreak)
                {
                    document.LastSection.AddPageBreak();
                    lineBreak = false;
                }

                switch (block)
                {
                    case ParagraphBlock p:
                        paragraph ??= document.LastSection.AddParagraph();
                        p.MakeParagraph(paragraph, document);
                        break;
                    case HeaderBlock header:
                        paragraph ??= document.LastSection.AddParagraph();
                        header.MakeHeader(paragraph, document);
                        break;
                    case ListBlock list:
                        paragraph ??= document.LastSection.AddParagraph();
                        list.MakeList(paragraph, document);
                        break;
                    case WorkerBlock w:
                        w.MakeWorkerTable(document, Unit.FromMillimeter(8));
                        break;
                    case HorizontalRuleBlock hr:
                        lineBreak = true;
                        break;
                    case TableBlock table:
                        table.Make(document); ;
                        break;
                    default:
                        break;
                }
                paragraph = null;
            }
        }

        public static void Make(this TableBlock block, Document document)
        {


            var table = new Table
            {
                Style = "table"
            };
            table.Format.Alignment = ParagraphAlignment.Center;
            table.Format.SpaceAfter = new Unit(1, UnitType.Centimeter);
            table.Format.WidowControl = true;

            //table.BottomPadding = new Unit(1, UnitType.Centimeter);

            table.Borders.Width = 0.75;

            for (var i = 0; i < block.ColumnDefinitions.Count; i++)
            {
                var column = table.AddColumn();

                column.Format.Alignment = block.ColumnDefinitions[i].Alignment switch
                {
                    ColumnAlignment.Center => ParagraphAlignment.Center,
                    ColumnAlignment.Right => ParagraphAlignment.Right,
                    _ => ParagraphAlignment.Left,
                };
            }



            for (var i = 0; i < block.Rows.Count; i++)
            {
                var row = table.AddRow();
                row.Height = 1;
                row.HeightRule = RowHeightRule.AtLeast;
                var originalRow = block.Rows[i].Cells;

                for (int j = 0; j < originalRow.Count; j++)
                {

                    var cell = row.Cells[j];

                    var p = cell.AddParagraph();
                    p.Style = "NoSpacing";
                    originalRow[j].Inlines.FillInlines(p);
                }



            }


            table.SetEdge(0, 0, block.ColumnDefinitions.Count, table.Rows.Count, Edge.Box, BorderStyle.Single, 1.5, Colors.Black);

            document.LastSection.Add(table);
        }

        public static XFont GetSubstituteFont(string emoji)
        {
            var xFont = fontResolver.Fonts.Select(x => new XFont(x, 9)).FirstOrDefault(x => x.IsCharSupported(emoji, 0));
            return xFont;
        }

        public static MarkdownDocument GetDefaultMarkdownDowcument()
        {
            return MarkdownDocument.CreateBuilder()
                            .AddBlockParser<HeaderBlock.HashParser>()
                            .AddBlockParser<ListBlock.Parser>()
                            .AddBlockParser<HorizontalRuleBlock.Parser>()
                            .AddBlockParser<CardMetadataBlock.Parser>()
                            .AddBlockParser<WorkerCostBlock.Parser>()
                            .AddBlockParser<WorkerTextBlock.Parser>()
                            .AddBlockParser<TableBlock.Parser>()

                            .AddInlineParser<ItalicTextInline.ParserAsterix>()
                            .AddInlineParser<ItalicTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldTextInline.ParserAsterix>()
                            .AddInlineParser<BoldItalicTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldItalicTextInline.ParserAsterix>()
                            .AddIcons()

                            .Build();
        }


        public static IDocumentBuilder AddIcons(this IDocumentBuilder documentBuilder)
        {
            return documentBuilder.AddInlineParser<EmojiInline.Parser>(x =>
             {
                 x.UseDefaultEmojis = false;
                 x.AddEmoji("f", 0x1f525);
                 x.AddEmoji("fw", 0x1f6f1);
                 //x.AddEmoji("fw", 0x26d1); // helmet with cross
                 x.AddEmoji("s", 0x2623);
                 x.AddEmoji("sw", 0x2695);
                 x.AddEmoji("x", 0x24e7);
                 x.AddEmoji("t", 0x231b);
                 x.AddEmoji("p", 0x1F465); // player
                 x.AddEmoji("m", 0x1F4FF); // Monk
                 x.AddEmoji("a", 0x1F52B); // Soldir
                 x.AddEmoji("b", 0x1F528); // Worker
                 x.AddEmoji("sience", 0x1F52C); // Sience
                                                //x.AddEmoji("siencetist", 0x1F97D); // Sience Googles. But not in font.
                 x.AddEmoji("siencetist", 0x1F393); // Sience Graduation head
                 x.AddEmoji("temple", 0x1F6D0); // Sience
                 x.AddEmoji("c", 0x1F0A0); // card

                 x.AddEmoji("d1", 0x2680); // Sience
                 x.AddEmoji("d2", 0x2681); // Sience
                 x.AddEmoji("d3", 0x2682); // Sience
                 x.AddEmoji("d4", 0x2683); // Sience
                 x.AddEmoji("d5", 0x2684); // Sience
                 x.AddEmoji("d6", 0x2685); // Sience
             });
        }
    }

}
