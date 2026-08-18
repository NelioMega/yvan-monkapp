using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

// Dessine l'icone de YvanMonkapp : un tableau vert, un cadre bois, un pi a la craie.
// Les tailles <= 128 partent en DIB classique car System.Drawing.Icon (zone de
// notification) ne decode pas les entrees compressees en PNG.

int[] sizes = { 16, 24, 32, 48, 64, 128, 256 };

string outPath = args.Length > 0
    ? args[0]
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "YvanMonkapp", "Assets", "monka.ico"));

var entries = new List<(int Size, byte[] Bytes)>();
foreach (int s in sizes)
{
    using var bmp = DrawBoard(s);
    entries.Add((s, s >= 256 ? ToPng(bmp) : ToDib(bmp)));
}

using (var fs = File.Create(outPath))
using (var bw = new BinaryWriter(fs))
{
    bw.Write((ushort)0);
    bw.Write((ushort)1);
    bw.Write((ushort)entries.Count);

    int offset = 6 + 16 * entries.Count;
    foreach (var (size, bytes) in entries)
    {
        byte dim = size >= 256 ? (byte)0 : (byte)size;
        bw.Write(dim);
        bw.Write(dim);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((ushort)1);
        bw.Write((ushort)32);
        bw.Write((uint)bytes.Length);
        bw.Write((uint)offset);
        offset += bytes.Length;
    }

    foreach (var (_, bytes) in entries) bw.Write(bytes);
}

Console.WriteLine($"icone ecrite : {outPath} ({new FileInfo(outPath).Length} octets)");

static Bitmap DrawBoard(int s)
{
    var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb);
    using var g = Graphics.FromImage(bmp);
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

    int r = Math.Max(2, (int)(s * 0.16));
    var rect = new Rectangle(0, 0, s - 1, s - 1);
    using var path = new GraphicsPath();
    path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
    path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
    path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
    path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
    path.CloseFigure();

    using (var brush = new LinearGradientBrush(new Rectangle(0, 0, s, s),
               Color.FromArgb(255, 34, 62, 51), Color.FromArgb(255, 17, 33, 28), 60f))
    {
        g.FillPath(brush, path);
    }

    if (s >= 32)
    {
        using var pen = new Pen(Color.FromArgb(255, 201, 154, 79), Math.Max(1f, s / 28f));
        g.DrawPath(pen, path);
    }

    using var font = new Font("Segoe UI", s * 0.66f, FontStyle.Bold, GraphicsUnit.Pixel);
    using var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
    using var chalk = new SolidBrush(Color.FromArgb(255, 246, 246, 240));
    g.DrawString("\u03c0", font, chalk, new RectangleF(0, -s * 0.05f, s, s), fmt);

    return bmp;
}

static byte[] ToPng(Bitmap bmp)
{
    using var ms = new MemoryStream();
    bmp.Save(ms, ImageFormat.Png);
    return ms.ToArray();
}

static byte[] ToDib(Bitmap bmp)
{
    int w = bmp.Width, h = bmp.Height;
    using var ms = new MemoryStream();
    using var bw = new BinaryWriter(ms);

    // BITMAPINFOHEADER, hauteur doublee : image XOR puis masque AND
    bw.Write(40u);
    bw.Write(w);
    bw.Write(h * 2);
    bw.Write((ushort)1);
    bw.Write((ushort)32);
    bw.Write(0u);
    bw.Write((uint)(w * h * 4));
    bw.Write(0);
    bw.Write(0);
    bw.Write(0u);
    bw.Write(0u);

    var rows = new byte[h][];
    var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    try
    {
        for (int y = 0; y < h; y++)
        {
            rows[y] = new byte[w * 4];
            Marshal.Copy(data.Scan0 + data.Stride * y, rows[y], 0, w * 4);
        }
    }
    finally
    {
        bmp.UnlockBits(data);
    }

    for (int y = h - 1; y >= 0; y--) bw.Write(rows[y]);          // un DIB se stocke de bas en haut
    bw.Write(new byte[(w + 31) / 32 * 4 * h]);                   // masque AND vide : l'alpha suffit

    bw.Flush();
    return ms.ToArray();
}
