using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SSX_Library.Internal;
using SSX_Library.Internal.Utilities;

namespace SSXLibrary.FileHandlers.LevelFiles.SSX3PS2.SSBData
{
    public class WorldSSH
    {
        public byte MatrixFormat;
        public int Size;
        public int Width;
        public int Height;
        public int Xaxis;
        public int Yaxis;
        public int LXPos;
        public bool flag1;
        public bool flag2;
        public bool flag3;
        public bool flag4;
        public int TYPos;
        public int Mipmaps; //Unit4

        bool AlphaFix;
        bool MetalBin;

        public byte[] Matrix;
        public SSHColourTable sshTable;
        public Image<Rgba32> bitmap;
        public Image<Rgba32> metalBitmap;

        public void Load(byte[] byteArray)
        {
            MemoryStream stream = new MemoryStream();
            StreamUtil.WriteBytes(stream, byteArray);
            stream.Position = 0;

            MatrixFormat = StreamUtil.ReadUInt8(stream);

            Size = StreamUtil.ReadUInt24(stream);

            Width = StreamUtil.ReadInt16(stream);

            Height = StreamUtil.ReadInt16(stream);

            Xaxis = StreamUtil.ReadInt16(stream);

            Yaxis = StreamUtil.ReadInt16(stream);

            //Add Other Flags Later
            LXPos = StreamUtil.ReadInt12(stream);

            TYPos = StreamUtil.ReadInt12(stream);

            int RealSize;

            if (Size != 0)
            {
                RealSize = Size - 0x80;
            }
            else
            {
                RealSize = Width * Height;
                if (MatrixFormat == 5)
                {
                    RealSize = RealSize * 4;
                }
            }

            stream.Position = 0x80;

            //Read Matrix
            var tempByte = new byte[RealSize];
            stream.Read(tempByte, 0, tempByte.Length);
            Matrix = tempByte;

            if (MatrixFormat == 1)
            {
                Matrix = ByteUtil.Unswizzle4bpp(Matrix, Width, Height);
            }

            if (MatrixFormat == 2)
            {
                Matrix = ByteUtil.Unswizzle8(Matrix, Width, Height);
            }

            //Split Image Into Proper Bytes
            if (MatrixFormat == 1)
            {
                tempByte = new byte[RealSize * 2];
                int posPoint = 0;
                for (int a = 0; a < Matrix.Length; a++)
                {
                    tempByte[posPoint] = (byte)ByteUtil.ByteToBitConvert(Matrix[a], 0, 3);
                    posPoint++;
                    tempByte[posPoint] = (byte)ByteUtil.ByteToBitConvert(Matrix[a], 4, 7);
                    posPoint++;
                }
                Matrix = tempByte;
            }

            //INDEXED COLOUR
            if (MatrixFormat == 2 || MatrixFormat == 1)
            {
                stream.Position = Size + 0x1;

                sshTable = new SSHColourTable();

                sshTable.Size = StreamUtil.ReadUInt24(stream);

                sshTable.Width = StreamUtil.ReadInt16(stream);

                sshTable.Height = StreamUtil.ReadInt16(stream);

                sshTable.Total = StreamUtil.ReadInt16(stream);

                sshTable.Format = StreamUtil.ReadUInt32(stream);

                sshTable.colorTable = new List<Rgba32>();

                int tempSize = sshTable.Size - 0x80;

                if (sshTable.Size == 0)
                {
                    tempSize = sshTable.Total;
                }

                stream.Position = Size + 0x80;

                var Matrix = StreamUtil.ReadBytes(stream, tempSize * 4);

                if (MatrixFormat == 2)
                {
                    Matrix = ByteUtil.UnswizzlePalette(Matrix, sshTable.Total);
                }

                for (int i = 0; i < tempSize; i++)
                {
                    int R = Matrix[i * 4];
                    int G = Matrix[i * 4+1];
                    int B = Matrix[i * 4+2];
                    int A = Matrix[i * 4+3];
                    sshTable.colorTable.Add(new Rgba32(A, R, G, B));
                }

                //for (int a = 0; a < tempSize; a++)
                //{
                //    sshTable.colorTable.Add(StreamUtil.ReadColour(stream));
                //}

                int Max = 0;
                //Alpha Fix
                for (int a = 0; a < sshTable.colorTable.Count; a++)
                {
                    if (Max < sshTable.colorTable[a].A)
                    {
                        Max = sshTable.colorTable[a].A;
                    }
                }
                if (Max <= 128)
                {
                    AlphaFix = true;

                    for (int a = 0; a < sshTable.colorTable.Count; a++)
                    {
                        var TempColour = sshTable.colorTable[a];
                        int A = TempColour.A * 2 - 1;
                        if (A > 255)
                        {
                            A = 255;
                        }
                        else if (A < 0)
                        {
                            A = 0;
                        }
                        TempColour = new Rgba32(A, TempColour.R, TempColour.G, TempColour.B);
                        sshTable.colorTable[a] = TempColour;
                    }
                }
            }

            //Find End Of Image
            long endPos = stream.Length;

            List<Color> MetalColours = new List<Color>();
            //Colour Correction
            int tempRead = stream.ReadByte();
            if (tempRead == 105)
            {
                //MetalBin = true;
                for (int c = 0; c < sshTable.colorTable.Count; c++)
                {
                    Rgba32 tempColor = sshTable.colorTable[c];
                    MetalColours.Add(new Argb32(255, tempColor.A, tempColor.A, tempColor.A));
                    int A = 255;
                    int R = tempColor.R;
                    int G = tempColor.G;
                    int B = tempColor.B;
                    sshTable.colorTable[c] = new Rgba32(R, G, B, A);
                }
            }
            else
            {
                stream.Position -= 1;
            }

            //Create Bitmap Image
            bitmap = new Image<Rgba32>(Width, Height);
            //bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            //metalBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            int post = 0;
            if (MatrixFormat == 1)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int colorPos = Matrix[post];
                        bitmap[x, y] = sshTable.colorTable[colorPos];

                        if (MetalBin)
                        {
                            //metalBitmap.SetPixel(x, y, MetalColours[colorPos]);
                        }
                        post++;
                    }
                }
            }
            else
            if (MatrixFormat == 2)
            {
                for (int y = 0; y < Height; y++)
                {
                        for (int x = 0; x < Width; x++)
                        {
                            int colorPos = Matrix[post];
                            //if (sshTable.Format != 0)
                            //{
                            //    colorPos = ByteUtil.ByteBitSwitch(colorPos);
                            //}

                            if (MetalBin)
                            {
                                //metalBitmap.SetPixel(x, y, MetalColours[colorPos]);
                            }

                            bitmap[x, y] = sshTable.colorTable[colorPos];
                        post++;
                    }
                }
            }
            else
            if (MatrixFormat == 5)
            {
                SSHColourTable colourTable = new SSHColourTable();
                colourTable.colorTable = new List<Rgba32>();
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int R = Matrix[post];
                        post++;
                        int G = Matrix[post];
                        post++;
                        int B = Matrix[post];
                        post++;
                        int A = Matrix[post];
                        post++;
                        bitmap[x, y] = new Rgba32(A, R, G, B);
                        if (!colourTable.colorTable.Contains(new Rgba32(A, R, G, B)))
                        {
                            colourTable.colorTable.Add(new Rgba32(A, R, G, B));
                        }
                    }
                }
                sshTable = colourTable;
            }
            else
            {
                //MessageBox.Show("Error reading File" + MatrixFormat.ToString());
            }

        }
        public void SaveImage(string path)
        {
            bitmap.SaveAsPng(path);
        }
        public struct SSHColourTable
        {
            public int Size;
            public int Width;
            public int Height;
            public int Total;
            public int Format;
            public List<Rgba32> colorTable;
        }
    }
}
