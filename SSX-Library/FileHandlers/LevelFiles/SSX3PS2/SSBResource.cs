namespace SSXLibrary.FileHandlers.LevelFiles.SSX3PS2;

/// <summary>A resource's type, owning track, track-local ID, SDB streaming chunk index, and raw payload.</summary>
public readonly record struct SSBResource(int Type, int Track, int Rid, int StreamingChunkId, byte[] Data);
