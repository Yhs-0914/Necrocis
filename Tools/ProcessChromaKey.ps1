param(
    [Parameter(Mandatory = $true)]
    [string]$SourcePath,

    [Parameter(Mandatory = $true)]
    [string]$DestinationPath,

    [int]$CropPadding = 12
)

$drawingAssembly = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Drawing.dll'
Add-Type -Path $drawingAssembly

if (-not ("Necrocis.ChromaKeyProcessor" -as [type])) {
    Add-Type -ReferencedAssemblies $drawingAssembly -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Necrocis
{
    public static class ChromaKeyProcessor
    {
        public static void Process(string sourcePath, string destinationPath, int cropPadding)
        {
            using (var source = new Bitmap(sourcePath))
            {
                var key = SampleBorderMedian(source);
                using (var keyed = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb))
                {
                    var rect = new Rectangle(0, 0, source.Width, source.Height);
                    var sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    var keyedData = keyed.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    var sourceBytes = new byte[Math.Abs(sourceData.Stride) * source.Height];
                    var keyedBytes = new byte[Math.Abs(keyedData.Stride) * source.Height];
                    Marshal.Copy(sourceData.Scan0, sourceBytes, 0, sourceBytes.Length);

                    int minX = source.Width;
                    int minY = source.Height;
                    int maxX = -1;
                    int maxY = -1;

                    for (int y = 0; y < source.Height; y++)
                    {
                        for (int x = 0; x < source.Width; x++)
                        {
                            int index = y * sourceData.Stride + x * 4;
                            int b = sourceBytes[index];
                            int g = sourceBytes[index + 1];
                            int r = sourceBytes[index + 2];
                            double distance = Math.Sqrt(
                                (r - key.R) * (r - key.R) +
                                (g - key.G) * (g - key.G) +
                                (b - key.B) * (b - key.B));

                            int alpha;
                            if (distance <= 48.0)
                                alpha = 0;
                            else if (distance >= 168.0)
                                alpha = 255;
                            else
                                alpha = (int)Math.Round((distance - 48.0) / 120.0 * 255.0);

                            if (alpha > 0)
                            {
                                double edge = 1.0 - alpha / 255.0;
                                g = Clamp((int)Math.Round(g - Math.Max(0, g - Math.Max(r, b)) * edge));
                                minX = Math.Min(minX, x);
                                minY = Math.Min(minY, y);
                                maxX = Math.Max(maxX, x);
                                maxY = Math.Max(maxY, y);
                            }

                            keyedBytes[index] = (byte)b;
                            keyedBytes[index + 1] = (byte)g;
                            keyedBytes[index + 2] = (byte)r;
                            keyedBytes[index + 3] = (byte)alpha;
                        }
                    }

                    Marshal.Copy(keyedBytes, 0, keyedData.Scan0, keyedBytes.Length);
                    source.UnlockBits(sourceData);
                    keyed.UnlockBits(keyedData);

                    if (maxX < minX || maxY < minY)
                        throw new InvalidOperationException("No foreground pixels remained after chroma keying.");

                    minX = Math.Max(0, minX - cropPadding);
                    minY = Math.Max(0, minY - cropPadding);
                    maxX = Math.Min(source.Width - 1, maxX + cropPadding);
                    maxY = Math.Min(source.Height - 1, maxY + cropPadding);
                    var crop = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                    using (var output = keyed.Clone(crop, PixelFormat.Format32bppArgb))
                        output.Save(destinationPath, ImageFormat.Png);
                }
            }
        }

        private static Color SampleBorderMedian(Bitmap bitmap)
        {
            var reds = new List<int>();
            var greens = new List<int>();
            var blues = new List<int>();
            int step = Math.Max(1, Math.Min(bitmap.Width, bitmap.Height) / 128);

            for (int x = 0; x < bitmap.Width; x += step)
            {
                Add(bitmap.GetPixel(x, 0), reds, greens, blues);
                Add(bitmap.GetPixel(x, bitmap.Height - 1), reds, greens, blues);
            }
            for (int y = 0; y < bitmap.Height; y += step)
            {
                Add(bitmap.GetPixel(0, y), reds, greens, blues);
                Add(bitmap.GetPixel(bitmap.Width - 1, y), reds, greens, blues);
            }

            reds.Sort();
            greens.Sort();
            blues.Sort();
            int middle = reds.Count / 2;
            return Color.FromArgb(reds[middle], greens[middle], blues[middle]);
        }

        private static void Add(Color color, List<int> reds, List<int> greens, List<int> blues)
        {
            reds.Add(color.R);
            greens.Add(color.G);
            blues.Add(color.B);
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }
    }
}
'@
}

[Necrocis.ChromaKeyProcessor]::Process(
    (Resolve-Path -LiteralPath $SourcePath).Path,
    [System.IO.Path]::GetFullPath($DestinationPath),
    $CropPadding)
