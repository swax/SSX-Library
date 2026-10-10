using System.Text;
using SSXLibrary.FileHandlers.LevelFiles.SSX3PS2;

namespace SSX_Library.Tests;

public sealed class SsbResourceReaderTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "ssb-reader-" + Guid.NewGuid().ToString("N"));
    private string SsbPath => Path.Combine(directory, "bam.ssb");

    public SsbResourceReaderTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, recursive: true);

    private void WriteSdb(params SDBHandler.StreamingChunkInfo[] chunks)
    {
        var sdb = new SDBHandler();
        sdb.streamingChunkInfos.AddRange(chunks);
        sdb.Save(Path.ChangeExtension(SsbPath, ".sdb"));
    }

    // An eight-byte RefPack literal: AIP resource header, track 8, RID 0x123456, no payload.
    private static byte[] Block(string marker = "CEND") =>
    [
        .. Encoding.ASCII.GetBytes(marker), 24, 0, 0, 0,
        0x10, 0xFB, 0, 0, 8,
        0xE1, 14, 0, 0, 0, 8, 0x56, 0x34, 0x12,
        0xFC,
        0, // Padding after the RefPack terminator belongs to the block.
    ];

    [Fact]
    public void ReadsRelocatedChunksInSdbOrderWithoutNameFiles()
    {
        using (var ssb = File.Create(SsbPath))
        {
            ssb.Position = 256;
            ssb.Write(Block());
            ssb.Position = 1024;
            ssb.Write(Block());
        }
        // AIP is excluded from unpackedSize. The resource count still includes it.
        WriteSdb(new() { numResources = 1, chunkOffset = 4 }, new() { numResources = 1, chunkOffset = 1 });

        var resources = SSBHandler.ReadResources(SsbPath).ToList();

        Assert.Equal(2, resources.Count);
        Assert.Equal([0, 1], resources.Select(r => r.StreamingChunkId));
        Assert.All(resources, r =>
        {
            Assert.Equal((14, 8, 0x123456), (r.Type, r.Track, r.Rid));
            Assert.Empty(r.Data);
        });
    }

    [Theory]
    [InlineData("invalid marker")]
    [InlineData("invalid size")]
    [InlineData("truncated block")]
    [InlineData("missing CEND")]
    [InlineData("truncated resource")]
    [InlineData("missing resource")]
    [InlineData("invalid offset")]
    public void RejectsBrokenChunkFraming(string failure)
    {
        byte[] bytes = Block(failure == "missing CEND" ? "CBXS" : "CEND");
        switch (failure)
        {
            case "invalid marker": bytes[0] = (byte)'X'; break;
            case "invalid size": bytes[4] = 7; break;
            case "truncated block": bytes = bytes[..^1]; break;
            case "truncated resource": bytes[15] = 1; break; // Payload size is one, but no payload follows.
        }
        File.WriteAllBytes(SsbPath, bytes);
        WriteSdb(new SDBHandler.StreamingChunkInfo
        {
            numResources = failure == "missing resource" ? 2 : 1,
            chunkOffset = failure == "invalid offset" ? 1 : 0,
        });

        Assert.Throws<InvalidDataException>(() => SSBHandler.ReadResources(SsbPath).ToList());
    }
}
