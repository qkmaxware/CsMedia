using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Qkmaxware.Media.Image;
using Qkmaxware.Media.Video;

namespace Qkmaxware.Media.Test {

[TestClass]
public class Yuv4MpegSerializerTest {
    [TestMethod]
    public void TestSerialization() {
        // Create animation
        List<IColourSampler> frames = new List<IColourSampler>();  
        var length = 48; 
        for (var i = 0; i <= length; i++) {
            var percent = ((i)/(double)length);
            var image = new Color[480,640];
            for (var k = 0; k < image.GetLength(0); k++) {
                for (var L = 0; L < image.GetLength(1); L++) {
                    image[k,L] = Color.Black;
                }
            }
            // 3 pixel wide line
            for (var j = 0; j < (percent * 640); j++) {
                // Move across the screen (height is first dimension, width the second)
                image[480/2,        j] = Color.Red;
                image[480/2 - 1,    j] = Color.Red;
                image[480/2 + 1,    j] = Color.Red;
            }
            frames.Add(image.GetSampler());
        }

        // Serialize
        var serializer = new Yuv4Mpeg2Serializer();
        Directory.CreateDirectory("data");
        using (var writer = new BinaryWriter(File.Open(Path.Combine("data", "movie.y4m"), FileMode.Create))) {
            serializer.Serialize(writer, frames, framesPerSecond: 24);
        }
    }   
}

}