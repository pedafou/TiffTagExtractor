using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using TiffTagExtractor.Commons;

namespace TiffTagExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string tiffFilePath = string.Empty;

            Console.WriteLine("TIFF TAG Extractor v.1.0.0");
            Console.WriteLine("==========================");

            if (args.Length < 1)
            {
                Console.WriteLine("Insert Path of TIFF File");
                tiffFilePath = Console.ReadLine();
            }
            else
            {
                tiffFilePath = args[0];
            }

            if (!File.Exists(tiffFilePath))
            {
                Console.WriteLine("Input File is not Exist");
                return;
            }

            ReadTiffTag(tiffFilePath);

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static void ReadTiffTag(string fileName, bool genResultFile = false)
        {
            Console.WriteLine("\nInput File : {0}", fileName);
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                var tagReader = new TagReader(fs);
                bool isBigTiff = false;
                bool byteOrder = false; //isBigEndian : Need Reverse                
                bool isError = false;

                var byteOrder_1 = tagReader.ReadByte(false);
                var byteOrder_2 = tagReader.ReadByte(false);
                var versionNo_1 = tagReader.ReadByte(false);
                var versionNo_2 = tagReader.ReadByte(false);

                #region ByteOrder / Tiff or BigTiff
                if (byteOrder_1 == 'I' || byteOrder_2 == 'I')
                {
                    byteOrder = false;

                    if (versionNo_1 == 42)
                        isBigTiff = false;
                    else if (versionNo_1 == 43)
                        isBigTiff = true;
                    else
                        isError = true;
                }
                else if (byteOrder_1 == 'M' || byteOrder_2 == 'M')
                {
                    byteOrder = true;

                    if (versionNo_2 == 42)
                        isBigTiff = false;
                    else if (versionNo_2 == 43)
                        isBigTiff = true;
                    else
                        isError = true;
                }
                else
                    isError = true;

                if (isError)
                {
                    Console.Write("[Error] Input file is not supported ");
                    return;
                }

                Console.WriteLine("Type : {0} / {1}", isBigTiff ? "BigTiff" : "Tiff", byteOrder ? "Big Endian" : "Little Endian");
                #endregion

                #region Define Tag Info (Position / Count / Size)

                ulong startOffset = 0;
                ulong tagCount = 0;

                var tagSize = isBigTiff ? 20 : 12;
                var tagOffset = isBigTiff ? 8 : 2;
                var numberSize = isBigTiff ? 8 : 4;
                var dataSize = isBigTiff ? 8 : 4;

                if (isBigTiff) // BigTiff
                {
                    tagReader.Seek(8);
                    startOffset = tagReader.ReadLong64(byteOrder);
                    tagReader.Seek(startOffset);
                    tagCount = tagReader.ReadLong64(byteOrder);
                }
                else // Tiff
                {
                    tagReader.Seek(4);
                    startOffset = tagReader.ReadLong(byteOrder);
                    tagReader.Seek(startOffset);
                    tagCount = tagReader.ReadShort(byteOrder);
                }
                #endregion

                #region
                for (int tagIndex = 0; Convert.ToUInt64(tagIndex) < tagCount; tagIndex++)
                {
                    tagReader.Seek(startOffset + Convert.ToUInt64(tagOffset + tagIndex * tagSize));

                    var tagID = tagReader.ReadShort(byteOrder);
                    var tagType = Enum.IsDefined(typeof(TagID), (TagID)tagID) ? (TagID)tagID : TagID.Unknown;
                    var dataType = tagReader.ReadShort(byteOrder);
                    var dataBytes = (ulong)TagReader.GetBytes(dataType);
                    ulong count = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                    if ((dataBytes * count) > Convert.ToUInt64(dataSize))
                    {
                        var address = isBigTiff ? tagReader.ReadLong64(byteOrder) : tagReader.ReadLong(byteOrder);
                        tagReader.Seek(address);
                    }

                    var isGeoTag = (tagType != TagID.GeoAsciiParamsTag && tagType != TagID.GeoDoubleParamsTag && tagType != TagID.GeoKeyDirectoryTag && tagType != TagID.ModelTransformationTag);
                    int readCount = isGeoTag && (dataType != 2) && (count > 10) ? 10 : (int)count;
                    var value = tagReader.Read(dataType, readCount, byteOrder);
                    Console.Write(" * {0,5} - {1,-30} : ", tagID, tagType);
                    switch (tagType)
                    {
                        case TagID.ModelTransformationTag:
                        case TagID.GeoKeyDirectoryTag:
                            Console.WriteLine();
                            var geoKeyDirectoryTagValue = value.Cast<object>().ToArray();
                            for (int i = 0; i < (int)count; i += 4)
                                Console.WriteLine("\t{0}", string.Join("\t", geoKeyDirectoryTagValue.Skip(i).Take(4)));
                            break;
                        default:
                            if (dataType == 2)
                                Console.Write(string.Join("", value.Cast<object>()));
                            else
                                Console.Write(string.Join(", ", value.Cast<object>()));

                            if (readCount < (int)count)
                                Console.WriteLine("...({0} items)", count);
                            else
                                Console.WriteLine();



                            break;
                    }
                }
                #endregion
            }
        }
    }
}
