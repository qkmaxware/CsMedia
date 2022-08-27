using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qkmaxware.Media.Image;

namespace Qkmaxware.Media.Test.Image {

[TestClass]
public class TiffTest {

    [TestMethod]
    public void TestSaveLoad() {
        var format = new TagImageFileFormat();
        var filename = Path.Combine("raw", "Test.tif");
        var image = format.Load(filename);

        Assert.AreEqual(SampleDepth.Bit8, image.SampleBitDepth);
        Assert.AreEqual(64, image.Width);
        Assert.AreEqual(64, image.Height);

        Directory.CreateDirectory("processed");
        format.Save(Path.Join("processed", filename), image);
    }

}

}
