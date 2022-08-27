using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qkmaxware.Media.Image;

namespace Qkmaxware.Media.Test.Image {

[TestClass]
public class PpmTest {

    private static string testPathRoot = Path.GetFullPath(Path.Combine(System.Environment.CurrentDirectory, "..", "..", ".."));

    [TestMethod]
    public void TestAsciiSaveLoad() {
        System.Console.WriteLine(testPathRoot);
        var format = new PortablePixelMapFormat();
        var filename = "Test.ascii.ppm";
        var image = format.Load(Path.Combine(testPathRoot, "raw", filename));

        Assert.AreEqual(SampleDepth.Bit8, image.SampleBitDepth);
        Assert.AreEqual(4, image.Width);
        Assert.AreEqual(4, image.Height);

        Directory.CreateDirectory(Path.Combine(testPathRoot, "processed"));
        using (var writer = new StreamWriter(Path.Join(testPathRoot, "processed", filename))) {
            format.SaveTo(writer, image);
        }
    }

    [TestMethod]
    public void TestBinarySaveLoad() {
        var format = new PortablePixelMapFormat();
        var filename = "Test.binary.ppm";
        var image = format.Load(Path.Combine(testPathRoot, "raw", filename));

        Assert.AreEqual(SampleDepth.Bit8, image.SampleBitDepth);
        Assert.AreEqual(4, image.Width);
        Assert.AreEqual(4, image.Height);

        Directory.CreateDirectory(Path.Combine(testPathRoot, "processed"));
        format.Save(Path.Join(testPathRoot, "processed", filename), image);
    }

}

}
