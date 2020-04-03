using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace AssetGenerator
{
    static class Program
    {
        private const double Resolution = 4;

        static async Task Main(string[] args)
        {
            if (args.Length == 1)
                Environment.CurrentDirectory = args[0];

            var output = new DirectoryInfo("output");
            output.Create();

            await CreateCrisis(output);
            await CreateActions(output);
            await CreateSience(output);
            await CreateRule(output);
        }

        private static async Task<bool> CreateRule(DirectoryInfo output)
        {
            var source = new FileInfo("rules.md");
            if (!source.Exists)
                return false;

            await RuleGenerator.GenerateDocument(source.FullName, Path.Combine(output.FullName, "rules.pdf"));

            return true;
        }
        private static async Task CreateCharacters(DirectoryInfo output)
        {
            var actionsInput = new DirectoryInfo("chracters");
            if (actionsInput.Exists)
            {
                var actionsOutput = output.CreateSubdirectory("chracters");
                var allCrisisFile = new FileInfo(Path.Combine(actionsOutput.FullName, "Index"));
                using (var crisisStream = allCrisisFile.Open(FileMode.Create))
                using (var crisisWriter = new StreamWriter(crisisStream))

                {

                    foreach (var item in actionsInput.GetFiles("*.md"))
                    {
                        var tmp = Path.GetTempFileName();
                        try
                        {
                            await global::KarthagoCharacterGeneratorTool.CharacterGenerator.GenerateDocument(item.FullName, tmp);

                            using var library = DocLib.Instance;
                            int pageCount;
                            using (var docReader = library.GetDocReader(tmp, 1, 1))
                                pageCount = docReader.GetPageCount();

                            var baseNname = Path.GetFileNameWithoutExtension(item.Name);
                            var numberOfCards = pageCount;

                            await crisisWriter.WriteLineAsync(baseNname);
                            await crisisWriter.WriteLineAsync(numberOfCards.ToString());
                            for (int j = 0; j < pageCount - 1; j++)
                            {
                                var name = $"{baseNname}_{j}";
                                using var pageReader = library.GetPageReader(tmp, j, Resolution);

                                var width = pageReader.GetPageWidth();
                                var height = pageReader.GetPageHeight();

                                var outputPng = new Bitmap(width, height);
                                var g = Graphics.FromImage(outputPng);
                                using (var currentImage = GetModifiedImage(pageReader))
                                    g.DrawImageUnscaled(currentImage, new Point(0, 0));
                                g.Dispose();
                                outputPng.Save(Path.Combine(actionsOutput.FullName, $"{name}.png"));
                                outputPng.Dispose();
                            }
                        }
                        finally
                        {
                            File.Delete(tmp);
                        }
                    }

                }
            }
        }

        private static async Task CreateSience(DirectoryInfo output)
        {
            var actionsInput = new DirectoryInfo("sience");
            if (actionsInput.Exists)
            {
                var actionsOutput = output.CreateSubdirectory("sience");
                var allCrisisFile = new FileInfo(Path.Combine(actionsOutput.FullName, "Index"));
                using (var crisisStream = allCrisisFile.Open(FileMode.Create))
                using (var crisisWriter = new StreamWriter(crisisStream))

                {

                    foreach (var item in actionsInput.GetFiles("*.md"))
                    {
                        var tmp = Path.GetTempFileName();
                        try
                        {
                            await global::SienceCards.SieneceGenerator.GenerateDocument(item.FullName, tmp);

                            using var library = DocLib.Instance;
                            int pageCount;
                            using (var docReader = library.GetDocReader(tmp, 1, 1))
                                pageCount = docReader.GetPageCount();
                            Bitmap outputPng = null;
                            Graphics g = null;
                            {
                                int width = -1;
                                int totalWidth = -1;
                                int height = -1;
                                var name = Path.GetFileNameWithoutExtension(item.Name);
                                var numberOfCards = (pageCount - 1);
                                await crisisWriter.WriteLineAsync(name);
                                await crisisWriter.WriteLineAsync(numberOfCards.ToString());
                                for (int j = 0; j < pageCount - 1; j++)
                                {
                                    using var pageReader = library.GetPageReader(tmp, j, Resolution);
                                    if (outputPng is null)
                                    {
                                        width = pageReader.GetPageWidth();
                                        totalWidth = width * Math.Min(numberOfCards, 10);
                                        height = pageReader.GetPageHeight();
                                        var totalHeight = (int)Math.Ceiling((numberOfCards / 10.0)) * height;
                                        outputPng = new Bitmap(totalWidth, totalHeight);
                                        g = Graphics.FromImage(outputPng);
                                    }
                                    using (var currentImage = GetModifiedImage(pageReader))
                                        g.DrawImageUnscaled(currentImage, new Point(width * (j % 10), height * (j / 10)));
                                }
                                g.Dispose();
                                outputPng.Save(Path.Combine(actionsOutput.FullName, $"{name}_front.png"));
                                outputPng.Dispose();
                                using (var pageReader = library.GetPageReader(tmp, pageCount - 1, Resolution))
                                using (var currentImage = GetModifiedImage(pageReader))
                                    currentImage.Save(Path.Combine(actionsOutput.FullName, $"{name}_back.png"));
                            }
                        }
                        finally
                        {
                            File.Delete(tmp);
                        }
                    }

                }
            }
        }
        private static async Task CreateActions(DirectoryInfo output)
        {
            var actionsInput = new DirectoryInfo("actions");
            if (actionsInput.Exists)
            {
                var actionsOutput = output.CreateSubdirectory("actions");
                var allCrisisFile = new FileInfo(Path.Combine(actionsOutput.FullName, "Index"));
                using (var crisisStream = allCrisisFile.Open(FileMode.Create))
                using (var crisisWriter = new StreamWriter(crisisStream))

                {

                    foreach (var item in actionsInput.GetFiles("*.md"))
                    {
                        var tmp = Path.GetTempFileName();
                        try
                        {
                            await global::ActionCards.ActionGenerator.GenerateDocument(item.FullName, tmp);

                            using var library = DocLib.Instance;
                            int pageCount;
                            using (var docReader = library.GetDocReader(tmp, 1, 1))
                                pageCount = docReader.GetPageCount();
                            Bitmap outputPng = null;
                            Graphics g = null;
                            {
                                int width = -1;
                                int totalWidth = -1;
                                int height = -1;
                                var name = Path.GetFileNameWithoutExtension(item.Name);
                                var numberOfCards = (pageCount - 1);
                                await crisisWriter.WriteLineAsync(name);
                                await crisisWriter.WriteLineAsync(numberOfCards.ToString());
                                for (int j = 0; j < pageCount - 1; j++)
                                {
                                    using var pageReader = library.GetPageReader(tmp, j, Resolution);
                                    if (outputPng is null)
                                    {
                                        width = pageReader.GetPageWidth();
                                        totalWidth = width * Math.Min(numberOfCards, 10);
                                        height = pageReader.GetPageHeight();
                                        var totalHeight = (int)Math.Ceiling((numberOfCards / 10.0)) * height;
                                        outputPng = new Bitmap(totalWidth, totalHeight);
                                        g = Graphics.FromImage(outputPng);
                                    }
                                    using (var currentImage = GetModifiedImage(pageReader))
                                        g.DrawImageUnscaled(currentImage, new Point(width * (j % 10), height * (j / 10)));
                                }
                                g.Dispose();
                                outputPng.Save(Path.Combine(actionsOutput.FullName, $"{name}_front.png"));
                                outputPng.Dispose();
                                using (var pageReader = library.GetPageReader(tmp, pageCount - 1, Resolution))
                                using (var currentImage = GetModifiedImage(pageReader))
                                    currentImage.Save(Path.Combine(actionsOutput.FullName, $"{name}_back.png"));
                            }
                        }
                        finally
                        {
                            File.Delete(tmp);
                        }
                    }

                }
            }
        }



        private static async Task CreateCrisis(DirectoryInfo output)
        {
            var crisisDirectory = new DirectoryInfo("crisis");
            if (crisisDirectory.Exists)
            {
                var crisisOutput = output.CreateSubdirectory("crisis");
                var allCrisisFile = new FileInfo(Path.Combine(crisisOutput.FullName, "Index"));
                using (var crisisStream = allCrisisFile.Open(FileMode.Create))
                using (var crisisWriter = new StreamWriter(crisisStream))

                {

                    foreach (var item in crisisDirectory.GetFiles("*.md"))
                    {
                        var tmp = Path.GetTempFileName();
                        try
                        {
                            var dataset = await global::CrisesCards.CirsisGeneator.GenerateDocument(item.FullName, tmp);

                            using (var library = DocLib.Instance)
                            {
                                int pageCount;
                                using (var docReader = library.GetDocReader(tmp, 1, 1))
                                    pageCount = docReader.GetPageCount();
                                for (int j = 0; j < dataset.Length; j++)
                                {
                                    var start = dataset[j].startIndex;
                                    var crisis = dataset[j].crisisType;
                                    int end;
                                    if (j < dataset.Length - 1)
                                        end = dataset[j + 1].startIndex - 1;
                                    else
                                        end = pageCount - 1;

                                    await crisisWriter.WriteLineAsync(crisis);
                                    var numberOfCards = (end - start);
                                    await crisisWriter.WriteLineAsync(numberOfCards.ToString());

                                    Bitmap outputPng = null;
                                    Graphics g = null;
                                    {
                                        int width = -1;
                                        int totalWidth = -1;
                                        int height = -1;

                                        for (int i = start, index = 0; i < end; i++, index++)
                                        {

                                            using (var pageReader = library.GetPageReader(tmp, i, Resolution))
                                            {
                                                if (outputPng is null)
                                                {
                                                    width = pageReader.GetPageWidth();
                                                    totalWidth = width * Math.Min(numberOfCards, 10);
                                                    height = pageReader.GetPageHeight();
                                                    var totalHeight = (int)Math.Ceiling((numberOfCards / 10.0)) * height;
                                                    outputPng = new Bitmap(totalWidth, totalHeight);
                                                    g = Graphics.FromImage(outputPng);
                                                }
                                                using (var currentImage = GetModifiedImage(pageReader))
                                                    g.DrawImageUnscaled(currentImage, new Point(width * (index % 10), height * (index / 10)));

                                            }
                                        }
                                    }
                                    g.Dispose();
                                    outputPng.Save(Path.Combine(crisisOutput.FullName, $"{crisis}_front.png"));
                                    outputPng.Dispose();
                                    using (var pageReader = library.GetPageReader(tmp, end, Resolution))
                                    {
                                        using (var currentImage = GetModifiedImage(pageReader))
                                            currentImage.Save(Path.Combine(crisisOutput.FullName, $"{crisis}_back.png"));
                                    }
                                }
                            }
                        }
                        finally
                        {
                            File.Delete(tmp);
                        }
                    }

                }
            }
        }

        private static IPageReader GetPageReader(this DocLib library, string tmp, int pageIndex, double resolution)
        {
            int widht, height;

            using (var docReader = library.GetDocReader(tmp, 1024, 1024))
            using (var page = docReader.GetPageReader(pageIndex))
            {
                var type = page.GetType();
                var member = type.GetField("_scaling", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var scaling = (double)member.GetValue(page);

                ;
                var originalHeight = page.GetPageHeight() / scaling;
                var originalWidth = page.GetPageWidth() / scaling;

                widht = (int)(originalWidth * resolution);
                height = (int)(originalHeight * resolution);
            }
            var doc2 = library.GetDocReader(tmp, widht, height);
            var pageReader = doc2.GetPageReader(pageIndex);
            return new PageReader(pageReader, doc2);
        }

        private class PageReader : IPageReader
        {
            private readonly IPageReader pageReader;
            private readonly IDocReader doc2;

            public PageReader(IPageReader pageReader, IDocReader doc2)
            {
                this.pageReader = pageReader;
                this.doc2 = doc2;
            }

            public int PageIndex => this.pageReader.PageIndex;

            public void Dispose()
            {
                this.pageReader.Dispose();
                this.doc2.Dispose();
            }

            public IEnumerable<Character> GetCharacters()
            {
                return this.pageReader.GetCharacters();
            }

            public byte[] GetImage()
            {
                return this.pageReader.GetImage();
            }

            public int GetPageHeight()
            {
                return this.pageReader.GetPageHeight();
            }

            public int GetPageWidth()
            {
                return this.pageReader.GetPageWidth();
            }

            public string GetText()
            {
                return this.pageReader.GetText();
            }
        }

        private static Image GetModifiedImage(IPageReader pageReader)
        {
            var rawBytes = pageReader.GetImage();

            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();

            var characters = pageReader.GetCharacters();

            using (var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                var background = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                bmp.AddBytes(rawBytes);

                using (var g = Graphics.FromImage(background))
                {
                    g.Clear(Color.White);
                    g.DrawImageUnscaled(bmp, Point.Empty);
                }


                //bmp.DrawRectangles(characters);

                return background;
            }
        }

        private static void AddBytes(this Bitmap bmp, byte[] rawBytes)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            var pNative = bmpData.Scan0;

            Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
            bmp.UnlockBits(bmpData);
        }

        private static void DrawRectangles(this Bitmap bmp, IEnumerable<Character> characters)
        {
            var pen = new Pen(Color.Red);

            using (var graphics = Graphics.FromImage(bmp))
            {
                foreach (var c in characters)
                {
                    var rect = new Rectangle(c.Box.Left, c.Box.Top, c.Box.Right - c.Box.Left, c.Box.Bottom - c.Box.Top);
                    graphics.DrawRectangle(pen, rect);
                }
            }
        }

    }
}
