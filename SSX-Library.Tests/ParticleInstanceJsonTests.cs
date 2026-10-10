using Newtonsoft.Json;
using SSXLibrary.JsonFiles.Tricky;

namespace SSX_Library.Tests;

public sealed class ParticleInstanceJsonTests
{
    [Fact]
    public void ParticleModelIndex_UsesCanonicalJsonName_AndRoundTrips()
    {
        var current = new ParticleInstanceJsonHandler
        {
            Particles =
            {
                new ParticleInstanceJsonHandler.ParticleJson
                {
                    ParticleName = "Fog_Test",
                    ParticleModelIndex = 7,
                },
            },
        };

        string json = JsonConvert.SerializeObject(current);
        Assert.Contains("\"ParticleModelIndex\":7", json);
        Assert.DoesNotContain("\"UnknownInt1\":", json);
        var roundTrip = JsonConvert.DeserializeObject<ParticleInstanceJsonHandler>(json);
        Assert.NotNull(roundTrip);
        Assert.Equal(7, roundTrip.Particles[0].ParticleModelIndex);
    }
}
