using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using iBoardBot.Helpers;

namespace iBoardBot {
    internal class Program {
        private static void Main(string[] args) {
            if (!args.Any()) {
                Console.WriteLine("ApiKey should be supplied.");
                return;
            }

            var apiKey = args.First();
            var baseUrl = new Uri("http://ibbapp.jjrobots.com/");
            var board = new BoardClient(baseUrl, apiKey);
            var fontFamily = LoadFontFamilyFromFile("Modenine.ttf");
            var lastImage = RenderText(fontFamily, "");

            while (true) {
                var newImage = RenderText(fontFamily, DateTime.Now.ToString("HH:mm"));
                var differences = GetDifferences(lastImage, newImage).ToArray();

                if (differences.Length > 0) {
                    var minX = differences.Min(point => point.X);
                    var maxX = differences.Max(point => point.X);
                    var minY = differences.Min(point => point.Y);
                    var maxY = differences.Max(point => point.Y);

                    board.Clear(minX/2, minY/2, maxX/2, maxY/2);

                    var region = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                    var redrawImage = GetRegionFromBitmap(newImage, region);

                    board.Draw(redrawImage);
                }

                Thread.Sleep(1000);
            }
        }

        private static Bitmap RenderText(FontFamily fontCollection, string text) {
            var bitmap = new Bitmap(716, 240);
            using (var graphics = Graphics.FromImage(bitmap)) {
                using (var font = new Font(fontCollection, 120, FontStyle.Regular, GraphicsUnit.Pixel)) {
                    var rectangle = new Rectangle(0, 0, 716, 240);
                    var stringFormat = new StringFormat {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };

                    graphics.Clear(Color.White);
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                    graphics.DrawString(text, font, Brushes.Black, rectangle, stringFormat);
                }
            }

            return bitmap;
        }


        public static Bitmap GetRegionFromBitmap(Bitmap srcBitmap, Rectangle srcRegion) {
            var newBitmap = new Bitmap(srcBitmap.Width, srcBitmap.Height);
            using (var graphics = Graphics.FromImage(newBitmap)) {
                graphics.Clear(Color.White);
                graphics.DrawImage(srcBitmap, srcRegion, srcRegion, GraphicsUnit.Pixel);
            }
            return newBitmap;
        }

        private static IEnumerable<Point> GetDifferences(Bitmap bmp1, Bitmap bmp2) {
            for (var y = 0; y < bmp1.Height; y++) {
                for (var x = 0; x < bmp1.Width; x++) {
                    var c1 = bmp1.GetPixel(x, y);
                    var c2 = bmp2.GetPixel(x, y);

                    if (c1 != c2) {
                        yield return new Point {X = x, Y = y};
                    }
                }
            }
        }

        private static FontFamily LoadFontFamilyFromFile(string fileName) {
            var fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(fileName);
            return fontCollection.Families[0];
        }
    }
}