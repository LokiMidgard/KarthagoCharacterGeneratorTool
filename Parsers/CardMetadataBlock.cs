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

                while (line.Length != 0)
                {


                    var end = line.IndexOfNexWhiteSpace();
                    if (end == -1)
                        end = line.Length;

                    var current = line.Slice(0, end).Trim();


                    var type = current[^1];

                    if (type != 'x' && type != '$' && type != 't')
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


                    line = line.Slice(end).Trim();
                }

                if (times is null)
                    times = 1;
                if (duration is null)
                    duration = 1;

                if (cost is null || times is null)
                    return null;

                var result = new CardMetadataBlock()
                {
                    Cost = cost.Value,
                    Times = times.Value,
                    Duration = duration.Value,
                };
                return BlockParseResult.Create(result, startLine, 1);
            }
        }

    }

}
