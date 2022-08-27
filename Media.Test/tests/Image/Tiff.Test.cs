using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qkmaxware.Media.Image;

namespace Qkmaxware.Media.Test.Image {

[TestClass]
public class TiffTest {

    private static string testPathRoot = Path.GetFullPath(Path.Combine(System.Environment.CurrentDirectory, "..", "..", ".."));

    [TestMethod]
    public void TestSaveLoad() {
        var format = new TagImageFileFormat();
        var filename = "Test.tif";
        var image = format.Load(Path.Combine(testPathRoot, "raw", filename));

        Assert.AreEqual(SampleDepth.Bit8, image.SampleBitDepth);
        Assert.AreEqual(64, image.Width);
        Assert.AreEqual(64, image.Height);

        Directory.CreateDirectory(Path.Combine(testPathRoot,"processed"));
        format.Save(Path.Join(testPathRoot, "processed", filename), image);
    }

}

}
