using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.IO;

namespace Parsers
{

    public class FontResolver : IFontResolver
    {

        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, (FontResolverInfo resolver, byte[] data)> fontFaceLookup = new System.Collections.Concurrent.ConcurrentDictionary<string, (FontResolverInfo facename, byte[] data)>();

        private readonly bool systemFontsFallback;


        public IList<string> Fonts { get; }

        public FontResolver() : this(true)
        {
            var assembly = typeof(FontResolver).Assembly;

            var list = new List<string>();
            this.Fonts = list.AsReadOnly();


            LoadFromManifest("CMU Serif", "Parsers.fonts.CMU_Serif.cmunrm.ttf", "Parsers.fonts.CMU_Serif.cmunbx.ttf", "Parsers.fonts.CMU_Serif.cmunbi.ttf", "Parsers.fonts.CMU_Serif.cmunti.ttf");

            LoadFromManifest("Code200365k", "Parsers.fonts.Code2003.Code200365k.ttf");

            LoadFromManifest("Montreal", "Parsers.fonts.Montreal.Montreal-Regular.ttf", bold: "Parsers.fonts.Montreal.Montreal-Bold.ttf");

            LoadFromManifest("Quivira", "Parsers.fonts.Quivira.Quivira.otf");


            void LoadFromManifest(string name, string regular, string? italicBold = null, string? bold = null, string? italic = null)
            {
                list.Add(name);
                using (var fontStream = assembly.GetManifestResourceStream(regular))
                    this.AddFont(
                        familyName: name,
                        boldStyle: BoldStyle.None,
                        italicStyle: ItalicStyle.None,
                        stream: fontStream);

                using (var fontStream = assembly.GetManifestResourceStream(italic ?? regular))
                    this.AddFont(
                        familyName: name,
                        boldStyle: BoldStyle.None,
                        italicStyle: italic != null ? ItalicStyle.Applyed : ItalicStyle.Simulate,
                        stream: fontStream);

                using (var fontStream = assembly.GetManifestResourceStream(bold ?? regular))
                    this.AddFont(
                        familyName: name,
                        boldStyle: bold != null ? BoldStyle.Applyed : BoldStyle.Simulate,
                        italicStyle: ItalicStyle.None,
                        stream: fontStream);

                using (var fontStream = assembly.GetManifestResourceStream(italicBold ?? regular))
                    this.AddFont(
                        familyName: name,
                        boldStyle: italicBold != null ? BoldStyle.Applyed : BoldStyle.Simulate,
                        italicStyle: italicBold != null ? ItalicStyle.Applyed : ItalicStyle.Simulate,
                        stream: fontStream);
            }

        }

        public FontResolver(bool systemFontsFallback)
        {
            this.systemFontsFallback = systemFontsFallback;
        }

        public void AddFont(string familyName, BoldStyle boldStyle, ItalicStyle italicStyle, Stream stream)
        {
            var faceName = familyName.ToLower()
                + (boldStyle != BoldStyle.None ? "|b" : "")
                + (italicStyle != ItalicStyle.None ? "|i" : "");

            var resolver = new FontResolverInfo(faceName, boldStyle == BoldStyle.Simulate, italicStyle == ItalicStyle.Simulate);
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                var data = memoryStream.ToArray();
                if (!this.fontFaceLookup.TryAdd(faceName, (resolver, data)))
                    throw new ArgumentException($"FontFace <{faceName}> already used");
            }
        }

        public byte[] GetFont(string faceName)
        {
            if (this.fontFaceLookup.TryGetValue(faceName, out var item))
                return item.data;
            return null;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string faceName = familyName.ToLower() +
                (isBold ? "|b" : "") +
                (isItalic ? "|i" : "");
            if (this.fontFaceLookup.TryGetValue(faceName, out var item))
                return item.resolver;
            if (this.systemFontsFallback)
                return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
            return null;
        }

        public enum BoldStyle
        {
            None,
            Applyed,
            Simulate,
        }

        public enum ItalicStyle
        {
            None,
            Applyed,
            Simulate
        }

    }

}
