namespace SSX_Library.Internal.Utilities.StreamExtensions;

/// <summary>
/// Stream extensions for moving a stream's position without reading or writing.
/// </summary>
internal static class Seeker
{
    /// <summary>
    /// Advanced the stream position to the next multiple of 16.
    /// With how often align by 16 is used, a quick function for it comes handy.
    /// </summary>
    /// <param name="alignment">How many bytes to align by</param>
    public static void AlignBy16(this Stream stream)
    {
        stream.AlignBy(16);
    }

    /// <summary>
    /// Advances the stream position to the next multiple of the specified alignment.
    /// Along with including a possible start offset if the start of the alignment
    /// shouldn't be based on beginning of the stream.
    /// </summary>
    /// <param name="alignment">How many bytes to align by</param>
    public static void AlignBy(this Stream stream, int alignment, long startOffset = 0)
    {
        long streamOffset = stream.Position - startOffset;
        int offset = alignment - ((int)streamOffset % alignment);
        if (offset != alignment)
        {
            stream.Position += offset;
        }
    }
}