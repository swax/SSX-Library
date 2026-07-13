using SSX_Library.Internal;
using SSX_Library.Internal.Utilities;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SSXLibrary.FileHandlers.LevelFiles.OnTourPS2
{
    public class SSBHandler
    {
        /*
            
        All IDS
        0 - Materials?
        1 - Patches
        2 - WorldMDR
        3 - Instance
        4 - Particle Model
        5 - Particles Instance
        6 - Lights
        7 - Halo
        8 - Splines
        9 - Shape
        10 - Old Shape Lightmaps
        11 - Vis Curtains
        12 - Collision?
        13 - Sound Triggers?
        14 - AIP
        15 - World Painter?
        16 - Scripts?
        17 - CameraTriggers?
        18 - NIS Table
        19 - Missions?
        20 - AudioBank
        21 - Radar?
        22 - Avalanche Animation
         */
        //public void LoadAndExtractSSB(string path, string extractPath)
        //{
        //    using (Stream stream = File.Open(path, FileMode.Open))
        //    {
        //        MemoryStream memoryStream = new MemoryStream();
        //        List<int> ints = new List<int>();
        //        int a = 0;
        //        int CEND = 0;
        //        int CBXS = 0;
        //        while (true)
        //        {
        //            if (stream.Position >= stream.Length - 1)
        //            {
        //                break;
        //            }
        //            string MagicWords = StreamUtil.ReadString(stream, 4);

        //            int Size = StreamUtil.ReadUInt32(stream);
        //            byte[] Data = new byte[Size - 8];
        //            byte[] DecompressedData = new byte[1];
        //            Data = StreamUtil.ReadBytes(stream, Size - 8);

        //            DecompressedData = RefpackHandler.Decompress(Data);
        //            StreamUtil.WriteBytes(memoryStream, DecompressedData);

        //            if (MagicWords.ToUpper() == "CBXS")
        //            {
        //                CBXS += 1;
        //            }

        //            if (MagicWords.ToUpper() == "CEND")
        //            {
        //                CEND += 1;
        //                int FilePos = 0;
        //                memoryStream.Position = 0;
        //                Directory.CreateDirectory(extractPath + "//" + a);
        //                while (memoryStream.Position < memoryStream.Length)
        //                {
        //                    int ID = StreamUtil.ReadUInt8(memoryStream);
        //                    int ChunkSize = StreamUtil.ReadInt24(memoryStream);
        //                    int TrackID = StreamUtil.ReadInt8(memoryStream);
        //                    int RID = StreamUtil.ReadInt24(memoryStream);

        //                    byte[] NewData = StreamUtil.ReadBytes(memoryStream, ChunkSize);

        //                    if (ID == 2)
        //                    {
        //                        WorldMDR worldMDR = new WorldMDR();
        //                        worldMDR.LoadData(NewData);
        //                    }

        //                    //var file = File.Create(extractPath + "//" + a + "//" + FilePos + "." + ID + "bin");
        //                    FilePos++;
        //                }
        //                memoryStream.Dispose();
        //                memoryStream = new MemoryStream();
        //                a++;
        //            }
        //        }
        //    }
        //}
        public void LoadAndExtractSSBFromSBD(string path, string extractPath)
        {
            SDBHandler sdbHandler = new SDBHandler();
            //sdbHandler.LoadSBD(path.Replace(".ssb", ".sdb"));

            PHMHandler phmHandler = new PHMHandler();
            //phmHandler.LoadPHM(path.Replace(".ssb", ".phm"));

            PSMHandler psmHandler = new PSMHandler();
            psmHandler.LoadPSM(path.Replace(".ssb", ".psm"));

            using (Stream stream = File.Open(path, FileMode.Open))
            {
                //PatchesJsonHandler patchesJsonHandler = new PatchesJsonHandler();
                //Bin0JsonHandler bin0JsonHandler = new Bin0JsonHandler();
                //InstanceJsonHandler bin3JsonHandler = new InstanceJsonHandler();
                //Bin5JsonHandler bin5JsonHandler = new Bin5JsonHandler();
                //Bin6JsonHandler bin6JsonHandler = new Bin6JsonHandler();
                //SplineJsonHandler splineJsonHandler = new SplineJsonHandler();
                //VisCurtainJsonHandler visCurtainJsonHandler = new VisCurtainJsonHandler();
                //MDRJsonHandler mdrJsonHandler = new MDRJsonHandler();

                MemoryStream DataMemoryStream = new MemoryStream();
                List<int> ints = new List<int>();
                int a = 0;
                int splitCount = 1;
                int FilePos = 0;
                int ChunkID = -1;
                int ChunkMax = -1;
                Directory.CreateDirectory(extractPath + "//Textures");
                Directory.CreateDirectory(extractPath + "//Lightmaps");
                Directory.CreateDirectory(extractPath + "//Levels");
                while (true)
                {
                    if (stream.Position >= stream.Length - 1)
                    {
                        break;
                    }
                    string MagicWords = StreamUtil.ReadString(stream, 4);

                    int Size = StreamUtil.ReadUInt32(stream);
                    int ReadSize = 8;
                    if (ChunkID == -1)
                    {
                        ChunkMax = StreamUtil.ReadUInt32(stream);
                        ReadSize = 12;
                    }
                    ChunkID++;

                    byte[] Buffer = StreamUtil.ReadBytes(stream, Size - ReadSize);
                    StreamUtil.WriteBytes(DataMemoryStream, Buffer);

                    if(ChunkID==ChunkMax)
                    {
                        DataMemoryStream.Position = 0;
                        byte[] Data = new byte[DataMemoryStream.Length];
                        byte[] DecompressedData = new byte[1];
                        Data = StreamUtil.ReadBytes(DataMemoryStream, (int)DataMemoryStream.Length);
                        DecompressedData = Refpack.Decompress(Data);
                        MemoryStream memoryStream = new MemoryStream();
                        StreamUtil.WriteBytes(memoryStream, DecompressedData);
                        var file = File.Create(extractPath + "0.bin");
                        memoryStream.Position = 0;
                        memoryStream.CopyTo(file);
                        memoryStream.Dispose();
                        memoryStream = new MemoryStream();
                        file.Close();
                    }
                }
            }
        }

        //public void PackSSB(string Folder, string BuildPath)
        //{
        //    MemoryStream memoryStream = new MemoryStream();
        //    string[] AllFiles = Directory.GetFiles(Folder, "*.BSX");
        //    for (int i = 0; i < AllFiles.Length; i++)
        //    {
        //        using (Stream stream = File.Open(Folder +"//"+ i.ToString()+".BSX", FileMode.Open))
        //        {
        //            byte[] bytes = new byte[1];
        //            while (true)
        //            {
        //                byte[] output = new byte[32768];
        //                bool End = false;
        //                int ReadLength = 40000;
        //                if (ReadLength+stream.Position>stream.Length)
        //                {
        //                    ReadLength = (int)(stream.Length - stream.Position);
        //                    End = true;
        //                }
        //                long StartPos = stream.Position;
        //                bool Start = true;
        //                while(output.Length> 32768-8)
        //                {
        //                    if (!Start)
        //                    {
        //                        stream.Position = StartPos;
        //                        ReadLength -= 32768 / 4;
        //                        End = false;
        //                    }
        //                    bytes = StreamUtil.ReadBytes(stream, ReadLength);
        //                    RefpackHandler.Compress(bytes, out output, CompressionLevel.Max);
        //                    Start = false;
        //                }
                        
                        
        //                if(!End)
        //                {
        //                    StreamUtil.WriteString(memoryStream,"CBSX");
        //                }
        //                else
        //                {
        //                    StreamUtil.WriteString(memoryStream, "CEND");
        //                }

        //                StreamUtil.WriteInt32(memoryStream, 32768);

        //                StreamUtil.WriteBytes(memoryStream, output);

        //                StreamUtil.AlignBy(memoryStream, 32768);

        //                if(End)
        //                {
        //                    break;
        //                }

        //            }
        //        }
        //    }
        //    if (File.Exists(BuildPath))
        //    {
        //        File.Delete(BuildPath);
        //    }
        //    var file = File.Create(BuildPath);
        //    memoryStream.Position = 0;
        //    memoryStream.CopyTo(file);
        //    memoryStream.Dispose();
        //    file.Close();
        //    GC.Collect();
        //}
    }
}
