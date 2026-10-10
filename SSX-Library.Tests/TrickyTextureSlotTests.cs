using SSXLibrary;
using SSXLibrary.JsonFiles.Tricky;
using SSX_Library.FileHandlers.LevelFiles.Tricky.PS2;

namespace SSX_Library.Tests;

public sealed class TrickyTextureSlotTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BuildPreservesTextureAssignmentsAndWritesTheBankSize(bool seeded)
    {
        string root = Path.Combine(Path.GetTempPath(), $"tricky-slots-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var patches = new PatchesJsonHandler();
            foreach (string texture in new[] { "0002.png", "0000.png" })
            {
                var points = new float[16, 3];
                for (int i = 0; i < 16; i++) { points[i, 0] = i % 4; points[i, 1] = i / 4; }
                patches.Patches.Add(new PatchesJsonHandler.PatchJson
                {
                    PatchName = texture, TexturePath = texture, Points = points,
                    UVPoints = new float[4, 2], LightMapPoint = new float[4],
                });
            }
            patches.CreateJson(Path.Combine(root, "Patches.json"));
            var seed = new List<string> { "0000.png", "0001.png", "0002.png" };
            var builder = new TrickyLevelInterface
            {
                PBDGenerate = true, SSHGenerate = false, LSSHGenerate = false,
                MAPGenerate = false, LTGGenerate = false, ADLGenerate = false, SSFGenerate = false,
                SkyPBDGenerate = false, SkySSHGenerate = false, AIPGenerate = false, SOPGenerate = false,
                SeedImageFiles = seeded ? seed : null,
            };
            string path = Path.Combine(root, "level.pbd");
            builder.BuildTrickyLevelFiles(root, path);
            var actual = new PBDHandler();
            actual.LoadPBD(path);

            Assert.Equal(seeded ? 3 : 2, actual.NumTextures);
            Assert.Equal(seeded ? 2 : 0, actual.Patches[0].TextureAssigment);
            Assert.Equal(seeded ? 0 : 1, actual.Patches[1].TextureAssigment);
            Assert.Equal(new[] { "0000.png", "0001.png", "0002.png" }, seed);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
