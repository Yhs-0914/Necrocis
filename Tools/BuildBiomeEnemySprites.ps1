param([string]$SourceDirectory = 'Exports/EnemyConcepts/2026-09-16-animation-sources')
$ErrorActionPreference = 'Stop'
$drawing = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Drawing.dll'
Add-Type -Path $drawing
Add-Type -ReferencedAssemblies $drawing -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
public static class BiomeSpriteSlicer {
    // Import preparation only: key the generated green background, slice the
    // 4x4 grid and uniformly resample. Animation poses come from the source art.
    public static void Slice(string source, string destination) {
        Directory.CreateDirectory(destination);
        string[] actions = { "Idle", "Move", "Attack", "Death" };
        using (var sheet = new Bitmap(source)) {
            if (sheet.Width != sheet.Height) throw new Exception("Expected square 4x4 sheet: " + source);
            var rects = new Rectangle[16];
            int[] rows = Cuts(sheet, true, 0, sheet.Width);
            int largest = 1;
            for (int row = 0; row < 4; row++) {
                int[] cols = Cuts(sheet, false, rows[row], rows[row+1]);
                for (int col = 0; col < 4; col++) {
                    int left=cols[col+1], right=cols[col], top=rows[row+1], bottom=rows[row];
                    for(int y=rows[row]; y<rows[row+1]; y++) for(int x=cols[col]; x<cols[col+1]; x++) {
                        if (IsKey(sheet.GetPixel(x,y))) continue;
                        left=Math.Min(left,x); right=Math.Max(right,x); top=Math.Min(top,y); bottom=Math.Max(bottom,y);
                    }
                    if(right<left || bottom<top) throw new Exception("Empty cell: " + source);
                    rects[row*4+col]=Rectangle.FromLTRB(left,top,right+1,bottom+1);
                    largest=Math.Max(largest,Math.Max(right-left+1,bottom-top+1));
                }
            }
            // One scale for all sixteen poses; do not enlarge the collapsed death frames.
            double scale = 112.0/largest;
            for (int row = 0; row < 4; row++) for (int col = 0; col < 4; col++) {
                using (var frame = new Bitmap(128, 128, PixelFormat.Format32bppArgb)) {
                    Rectangle bounds=rects[row*4+col];
                    int width=Math.Max(1,(int)Math.Round(bounds.Width*scale));
                    int height=Math.Max(1,(int)Math.Round(bounds.Height*scale));
                    int offsetX=(128-width)/2, offsetY=120-height;
                    int opaque = 0;
                    for (int y = 0; y < height; y++) for (int x = 0; x < width; x++) {
                        int sx = Math.Min(bounds.Right-1, bounds.Left+(int)((x+0.5)/scale));
                        int sy = Math.Min(bounds.Bottom-1, bounds.Top+(int)((y+0.5)/scale));
                        Color c = sheet.GetPixel(sx, sy);
                        if (IsKey(c)) c = Color.Transparent;
                        else if (c.A > 0) opaque++;
                        frame.SetPixel(offsetX+x, offsetY+y, c);
                    }
                    if (opaque < 35) throw new Exception("Empty animation frame: " + source + " " + row + ":" + col);
                    frame.Save(Path.Combine(destination, actions[row] + "_" + col + ".png"), ImageFormat.Png);
                }
            }
        }
    }
    private static bool IsKey(Color c) { return c.A == 0 || (c.G > 90 && c.G-Math.Max(c.R,c.B) > 35); }
    // Generated sheets can drift a few pixels off their nominal grid. Locate
    // the empty separator nearest each grid line so appendages aren't sliced.
    private static int[] Cuts(Bitmap sheet, bool horizontal, int start, int end) {
        int length=horizontal?sheet.Height:sheet.Width;
        int[] cuts={0,0,0,0,length};
        for(int i=1;i<4;i++) {
            int nominal=(int)Math.Round(length*i/4.0), best=nominal, score=int.MaxValue;
            int radius=length/18;
            for(int p=nominal-radius;p<=nominal+radius;p++) {
                int occupied=0;
                for(int q=start;q<end;q++) if(!IsKey(sheet.GetPixel(horizontal?q:p,horizontal?p:q))) occupied++;
                int value=occupied*length+Math.Abs(p-nominal);
                if(value<score) { score=value; best=p; }
            }
            cuts[i]=best;
        }
        return cuts;
    }
}
'@
$root = Split-Path -Parent $PSScriptRoot
$sourceRoot = Join-Path $root $SourceDirectory
$assetRoot = Join-Path $root 'Assets/_Project/Art/Images/Enemies/BiomeExclusive'
$utf8 = New-Object System.Text.UTF8Encoding($false)
foreach ($sheet in Get-ChildItem -LiteralPath $sourceRoot -Filter '*.png') {
    $destination = Join-Path $assetRoot $sheet.BaseName
    [BiomeSpriteSlicer]::Slice($sheet.FullName, $destination)
    foreach ($frame in Get-ChildItem -LiteralPath $destination -Filter '*.png') {
        $metaPath = $frame.FullName + '.meta'
        $guid = if (Test-Path -LiteralPath $metaPath) { [regex]::Match([IO.File]::ReadAllText($metaPath), 'guid: ([a-f0-9]+)').Groups[1].Value } else { [guid]::NewGuid().ToString('N') }
        $meta = @"
fileFormatVersion: 2
guid: $guid
TextureImporter:
  serializedVersion: 13
  internalIDToNameTable: []
  externalObjects: {}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  maxTextureSize: 128
  textureCompression: 0
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 9
  spritePivot: {x: 0.5, y: 0.06}
  spritePixelsToUnits: 80
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteGenerateFallbackPhysicsShape: 0
  textureType: 8
  textureShape: 1
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 128
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 100
    overridden: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    nameFileIdTable: {}
  userData: Necrocis biome enemy animation
  assetBundleName:
  assetBundleVariant:
"@
        [IO.File]::WriteAllText($metaPath, $meta + "`n", $utf8)
    }
    Write-Output ($sheet.BaseName + ': 16 frames')
}
# Give every new folder a stable Unity identity before Unity imports it.
foreach ($folder in Get-ChildItem -LiteralPath (Join-Path $root 'Assets/_Project/Art/Images/Enemies') -Directory -Recurse) {
    $metaPath = $folder.FullName + '.meta'
    if (!(Test-Path -LiteralPath $metaPath)) {
        $guid = [guid]::NewGuid().ToString('N')
        [IO.File]::WriteAllText($metaPath, "fileFormatVersion: 2`nguid: $guid`nfolderAsset: yes`nDefaultImporter:`n  externalObjects: {}`n", $utf8)
    }
}
