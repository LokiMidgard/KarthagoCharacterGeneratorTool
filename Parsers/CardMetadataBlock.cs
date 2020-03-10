using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Parsers.Markdown.Blocks;
using Microsoft.Toolkit.Parsers.Markdown.Helpers;
using System;
using System.Collections.Generic;

namespace Parsers
{


    public class CardMetadataBlock : MarkdownBlock
    {

        public int Times { get; set; }
        public int Cost { get; set; }
        public int Duration { get; set; }
        public string Type { get; set; }

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
                int? duration = null;
                string? cardType = null;

                while (line.Length != 0)
                {
                    int end;
                    if (line[0] == '(')
                    {
                        end = line.FindClosingBrace() + 1;
                        if (end == 0)
                            return null;
                    }
                    else
                    {
                        end = line.IndexOfNexWhiteSpace();
                        if (end == -1)
                            end = line.Length;
                    }

                    var current = line.Slice(0, end).Trim();


                    var type = current[^1];

                    if (type != 'x' && type != '$' && type != 't' && type != ')')
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
                    else if (type == 't')
                    {
                        if (duration.HasValue)
                            return null;
                        if (!int.TryParse(current.Slice(0, current.Length - 1), System.Globalization.NumberStyles.Any, new System.Globalization.CultureInfo("de"), out var value))
                            return null;

                        duration = value;
                    }
                    else if (type == ')')
                    {
                        if (cardType != null)
                            return null;
                        cardType = current.Slice(1, current.Length - 2).ToString();
                    }


                    line = line.Slice(end).Trim();
                }

                if (cost is null && times is null && duration is null && cardType is null)
                    return null;

                var result = new CardMetadataBlock()
                {
                    Cost = cost ?? 0,
                    Times = times ?? 1,
                    Duration = duration ?? 1,
                    Type = cardType,
                };
                return BlockParseResult.Create(result, startLine, 1);
            }
        }

    }

}
