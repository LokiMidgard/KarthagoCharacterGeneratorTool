using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Helpers;
using Microsoft.Toolkit.Parsers.Markdown.Inlines;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using System;
using System.Collections.Generic;

namespace Parsers
{


    public class WorkerCostBlock : WorkerBlock
    {


        public IList<uint> WorkerCosts { get; set; }

        protected override int Count => this.WorkerCosts.Count;

        protected override void GetString(int index, Paragraph paragraph)
        {
            paragraph.AddText(this.WorkerCosts[index].ToString());
        }


        public new class Parser : Parser<WorkerCostBlock>
        {
            protected override BlockParseResult<WorkerCostBlock> ParseInternal(LineBlock markdown, int startLine, bool lineStartsNewParagraph, MarkdownDocument document)
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
                if (list.Count == 0)
                    return null;

                var result = new WorkerCostBlock()
                {
                    WorkerCosts = list.AsReadOnly()
                };
                return BlockParseResult.Create(result, startLine, 1);
            }
        }

    }

    public abstract class WorkerBlock : MarkdownBlock
    {

        protected abstract int Count { get; }

        protected abstract void GetString(int index, Paragraph paragraph);

        public void MakeWorkerTable(Paragraph paragraph, Document document, Unit? blockSize = null)
        {
            blockSize ??= Unit.FromCentimeter(1.5);

            var numberOfColumns = Math.Min(6, this.Count);


            var table = new Table
            {
                Style = "table"
            };
            table.Format.Alignment = ParagraphAlignment.Center;
            table.Format.SpaceAfter = new Unit(1, UnitType.Centimeter);
            table.Format.WidowControl = true;

            //table.BottomPadding = new Unit(1, UnitType.Centimeter);

            table.Borders.Width = 0.75;

            for (var i = 0; i < numberOfColumns; i++)
            {
                var column = table.AddColumn(blockSize.Value);
                column.Format.Alignment = ParagraphAlignment.Center;
            }

            Row row = null;


            for (var i = 0; i < this.Count; i++)
            {
                if (row is null)
                {
                    row = table.AddRow();
                    row.Height = blockSize.Value;
                    row.HeightRule = RowHeightRule.Exactly;
                }

                var cell = row.Cells[i % numberOfColumns];

                var p = cell.AddParagraph();
                this.GetString(i, p);




                if (i % numberOfColumns == numberOfColumns - 1)
                    row = null;

            }

            if (row != null)
                for (int i = 0; i < numberOfColumns - (this.Count % numberOfColumns); i++)
                {
                    var cell = row.Cells[numberOfColumns - 1 - i];
                    cell.Shading.Color = Colors.Black;
                }

            table.SetEdge(0, 0, numberOfColumns, table.Rows.Count, Edge.Box, BorderStyle.Single, 1.5, Colors.Black);

            document.LastSection.Add(table);
        }

    }
    public class WorkerTextBlock : WorkerBlock
    {
        protected override int Count => this.WorkerText.Count;

        protected override void GetString(int index, Paragraph paragraph)
        {
            this.WorkerText[index].FillInlines(paragraph);
        }

        public IList<IEnumerable<MarkdownInline>> WorkerText { get; set; }

        public new class Parser : Parser<WorkerTextBlock>
        {
            protected override BlockParseResult<WorkerTextBlock> ParseInternal(LineBlock markdown, int startLine, bool lineStartsNewParagraph, MarkdownDocument document)
            {
                var line = markdown[startLine].Trim();

                if (line.Length == 0 || line[0] != '>')
                    return null;

                line = line.Slice(2).TrimStart();

                var list = new List<IEnumerable<MarkdownInline>>();


                while (line.Length != 0)
                {
                    var current = line;
                    var splitter = current.IndexOf(":<");

                    if (splitter == -1)
                        return null;

                    var currentEnd = current.Slice(splitter + 1).FindClosingBrace() + splitter + 1;
                    if (currentEnd < splitter + 1)
                        return null;

                    current = current.Slice(0, currentEnd + 1);

                    var first = current.Slice(0, splitter);
                    var scccond = current.Slice(splitter + 1);
                    // remove the <>
                    scccond = scccond.Slice(1, scccond.Length - 2);


                    if (!uint.TryParse(first, out var count))
                        return null;

                    for (var i = 0; i < count; i++)
                    {
                        list.Add(document.ParseInlineChildren(scccond, true, true));
                    }

                    line = line.Slice(current.Length).Trim();
                }

                if (list.Count == 0)
                    return null;

                var result = new WorkerTextBlock()
                {
                    WorkerText = list.AsReadOnly()
                };
                return BlockParseResult.Create(result, startLine, 1);
            }
        }

    }

}
