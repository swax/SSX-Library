using System.Numerics;
using SSXLibrary.FileHandlers.LevelFiles.SSX3PS2.SSBData;
using SSXLibrary.JsonFiles.SSX3;

namespace SSX_Library.Tests;

public sealed class Ssx3JsonExportTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "ssx3-json-" + Guid.NewGuid().ToString("N"));

    public Ssx3JsonExportTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, recursive: true);

    [Theory]
    [InlineData(1f)]
    [InlineData(0.4641f)]
    [InlineData(-1.311f)] // a mirrored prop
    public void InstanceJsonScaleIsTheInstanceScale(float scale)
    {
        var matrix = Matrix4x4.CreateFromYawPitchRoll(0.3f, -1.1f, 2f) * Matrix4x4.CreateTranslation(-6986, -93180, 87348);
        var instance = new WorldInstance { matrix4X4 = matrix, Scale = scale };

        var json = instance.ToJSON();

        var placed = Matrix4x4.CreateScale(json.Scale[0], json.Scale[1], json.Scale[2])
            * Matrix4x4.CreateFromQuaternion(new Quaternion(json.Rotation[0], json.Rotation[1], json.Rotation[2], json.Rotation[3]))
            * Matrix4x4.CreateTranslation(json.Position[0], json.Position[1], json.Position[2]);
        var corner = new Vector3(2252, 1498, 3475);
        var expected = Vector3.Transform(corner, Matrix4x4.CreateScale(scale) * matrix);
        Assert.InRange((Vector3.Transform(corner, placed) - expected).Length(), 0, 0.5f);
    }

    [Fact]
    public void ModelObjectWithoutAMatrixExportsAnIdentityTransform()
    {
        var matrix = Matrix4x4.CreateRotationZ(1f) * Matrix4x4.CreateTranslation(10, 20, 30);
        var mdr = new WorldMDR
        {
            ModelObjects =
            [
                new WorldMDR.ModelObject { ParentID = -1, MatrixOffset = -1 },
                new WorldMDR.ModelObject { ParentID = 0, MatrixOffset = 256, matrix4X4 = matrix },
            ],
        };

        var json = mdr.SaveModel(directory);

        var none = json.ModelObjects[0];
        Assert.False(none.IncludeMatrix);
        Assert.Equal([0f, 0f, 0f], none.Position);
        Assert.Equal([0f, 0f, 0f, 1f], none.Rotation);
        Assert.Equal([1f, 1f, 1f], none.Scale);

        var placed = json.ModelObjects[1];
        Assert.True(placed.IncludeMatrix);
        Assert.Equal([10f, 20f, 30f], placed.Position);
        Assert.Equal(1f, placed.Scale[0], 4);
    }

    [Fact]
    public void AipJsonKeepsEveryStartRecordFieldAndLoadsBack()
    {
        var aip = new WorldAIP();
        aip.u1Structs.Add(new WorldAIP.U1Struct
        {
            U0 = 1, U1 = 2, U2 = new Vector3(3, 4, 5), U3 = new Vector3(6, 7, 8), U4 = 9, U5 = 10,
        });
        string path = Path.Combine(directory, "AIP.json");

        aip.ToJson(path);
        var json = AIPJsonHandler.Load(path);

        var start = Assert.Single(json.u1Structs);
        Assert.Equal((1, 2, 9, 10), (start.U0, start.U1, start.U4, start.U5));
        Assert.Equal([3f, 4f, 5f], start.U2);
        Assert.Equal([6f, 7f, 8f], start.U3);
    }
}
