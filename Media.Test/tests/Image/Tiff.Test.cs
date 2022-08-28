using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qkmaxware.Media.Image;
using Qkmaxware.Media.Image.Metadata;

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

    [TestMethod]
    public void TestMetadata() {
        var format = new TagImageFileFormat();
        var filename = "Test.tif";
        var image = format.Load(Path.Combine(testPathRoot, "raw", filename));

        foreach (var field in image.Metadata) {
            System.Console.WriteLine(field);
        }

        // Test reading metadata
        var width = image.Metadata.FindField<UInt16>(ExifTag.ImageWidth);
        var height = image.Metadata.FindField<UInt16>(ExifTag.ImageHeight);
        Assert.AreEqual(64, width.Value);
        Assert.AreEqual(64, height.Value);

        var layer = image.Metadata.FindField<string>(ExifTag.PageName);
        Assert.AreEqual("Background\0", layer.Value);

        // Test adding metadata
        image.Metadata.AddField(new Field<string>(ExifTag.Software, ".Net6"));
        format.Save(Path.Join(testPathRoot, "processed", filename + ".tagged.tif"), image);
    }

}

}
