using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SSX_Library.Internal;
using SSX_Library.Internal.Utilities;

namespace SSXLibrary.FileHandlers.LevelFiles.SSX3PS2.SSBData
{
    public class WorldOldSSH
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
        public Image<Rgba32> Image;
        public Image<Rgba32> Metalimage;

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
                RealSize = Size - 16;
            }
            else
            {
                RealSize = Width * Height;
                if (MatrixFormat == 5)
                {
                    RealSize = RealSize * 4;
                }
            }

            //Read Matrix
            var tempByte = new byte[RealSize];
            stream.Read(tempByte, 0, tempByte.Length);
            Matrix = tempByte;

            //Decompress
            if (MatrixFormat == 130)
            {
                Matrix = Refpack.Decompress(Matrix);
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

            sshTable.colorsTable = new List<Rgba32>();

            //INDEXED COLOUR
            if (MatrixFormat == 2 || MatrixFormat == 1 || MatrixFormat == 130)
            {
                int Spos = (int)stream.Position;
                bool find = false;
                while (!find)
                {
                    if (stream.ReadByte() == 0x21)
                    {
                        Spos = (int)stream.Position;
                        find = true;
                    }
                }
                SSHColourTable sshTable = new SSHColourTable();

                sshTable.Size = StreamUtil.ReadUInt24(stream);

                sshTable.Width = StreamUtil.ReadInt16(stream);

                sshTable.Height = StreamUtil.ReadInt16(stream);

                sshTable.Total = StreamUtil.ReadInt16(stream);

                sshTable.Format = StreamUtil.ReadUInt32(stream);

                sshTable.colorsTable = new List<Rgba32>();

                int tempSize = sshTable.Size / 4 - 4;
                if (sshTable.Size == 0)
                {
                    tempSize = sshTable.Total;
                }

                stream.Position = Spos + 15;

                for (int a = 0; a < tempSize; a++)
                {
                    sshTable.colorsTable.Add(new Rgba32(stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte()));
                }

                int Max = 0;
                //Alpha Fix
                for (int a = 0; a < sshTable.colorsTable.Count; a++)
                {
                    if (Max < sshTable.colorsTable[a].A)
                    {
                        Max = sshTable.colorsTable[a].A;
                    }
                }
                if (Max <= 128)
                {
                    AlphaFix = true;

                    for (int a = 0; a < sshTable.colorsTable.Count; a++)
                    {
                        var TempColour = sshTable.colorsTable[a];
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
                        sshTable.colorsTable[a] = TempColour;
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
                for (int c = 0; c < sshTable.colorsTable.Count; c++)
                {
                    Rgba32 tempColor = sshTable.colorsTable[c];
                    MetalColours.Add(new Rgba32(255, tempColor.A, tempColor.A, tempColor.A));
                    int A = 255;
                    int R = tempColor.R;
                    int G = tempColor.G;
                    int B = tempColor.B;
                    sshTable.colorsTable[c] = new Rgba32(A, R, G, B);
                }
            }
            else
            {
                stream.Position -= 1;
            }

            //Create Bitmap Image
            Image = new Image<Rgba32>(Width, Height);
            //metalBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            int post = 0;
            if (MatrixFormat == 1)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int colorPos = Matrix[post];
                        Image[x, y] = sshTable.colorsTable[colorPos];

                        if (MetalBin)
                        {
                            //metalBitmap.SetPixel(x, y, MetalColours[colorPos]);
                        }
                        post++;
                    }
                }
            }
            else
                if (MatrixFormat == 2 || MatrixFormat == 130)
                {
                    //if (LXPos == 2)
                    //{
                    //    Matrix = ByteUtil.ByteArraySwap(Matrix, tempImageHeader);
                    //}
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            int colorPos = Matrix[post];
                            if (sshTable.Format != 0)
                            {
                                colorPos = ByteUtil.ByteBitSwitch(colorPos);
                            }

                            if (MetalBin)
                            {
                                //metalBitmap.SetPixel(x, y, MetalColours[colorPos]);
                            }

                            Image[x, y] = sshTable.colorsTable[colorPos];
                            post++;
                        }
                    }
                }
                else
                    if (MatrixFormat == 5)
                    {
                        SSHColourTable colourTable = new SSHColourTable();
                        colourTable.colorsTable = new List<Rgba32>();
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
                                //bitmap.SetPixel(x, y, Color.FromArgb(A, R, G, B));
                                if (!colourTable.colorsTable.Contains(new Rgba32(A, R, G, B)))
                                {
                                    colourTable.colorsTable.Add(new Rgba32(A, R, G, B));
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
            Image.SaveAsPng(path);
        }
        public struct SSHColourTable
        {
            public int Size;
            public int Width;
            public int Height;
            public int Total;
            public int Format;
            public List<Rgba32> colorsTable;
        }
    }
}
