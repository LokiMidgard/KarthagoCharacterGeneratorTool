using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using MigraDoc.DocumentObjectModel;
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


                            .AddInlineParser<ItalicTextInline.ParserAsterix>()
                            .AddInlineParser<ItalicTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldTextInline.ParserAsterix>()
                            .AddInlineParser<BoldItalicTextInline.ParserUnderscore>()
                            .AddInlineParser<BoldItalicTextInline.ParserAsterix>()
                            .AddInlineParser<EmojiInline.Parser>(x =>
                            {
                                x.UseDefaultEmojis = false;
                                x.AddEmoji("f", 0x1f525);
                                //x.AddEmoji("fw", 0x1f6f1); // dont knwo yet why this and fire have problems together
                                x.AddEmoji("fw", 0x26d1);
                                x.AddEmoji("s", 0x2623);
                                x.AddEmoji("sw", 0x2695);
                                x.AddEmoji("x", 0x24e7);
                                x.AddEmoji("t", 0x231b);
                            })

                            .Build();
        }
    }

}
