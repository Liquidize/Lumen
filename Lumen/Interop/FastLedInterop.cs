using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lumen.Api.Graphics;
using ZLIB;

namespace Lumen.Interop
{
    public static class QueueExtensions
    {
        public static IEnumerable<T> DequeueChunk<T>(this ConcurrentQueue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                T result;
                if (false == queue.TryDequeue(out result))
                    throw new Exception("Unable to Dequeue the data!");
                yield return result;
            }
        }
    }

    public static class FastLedInterop
    {
        public static byte[] GetColorBytesAtOffset(LedColor[] pixels, uint offset, uint length, bool bReversed = false, bool bRedGreenSwap = false)
        {
            byte[] data = new byte[length * 3];
            for (int i = 0; i < length; i++)
            {
                if (bRedGreenSwap)
                {
                    data[i * 3] = bReversed ? pixels[offset + length - 1 - i].R : pixels[offset + i].G;
                    data[i * 3 + 1] = bReversed ? pixels[offset + length - 1 - i].G : pixels[offset + i].R;
                }
                else
                {
                    data[i * 3] = bReversed ? pixels[offset + length - 1 - i].R : pixels[offset + i].R;
                    data[i * 3 + 1] = bReversed ? pixels[offset + length - 1 - i].G : pixels[offset + i].G;
                }
                data[i * 3 + 2] = bReversed ? pixels[offset + length - 1 - i].B : pixels[offset + i].B;
            }
            return data;
        }


        public static byte[] ULONGToBytes(UInt64 input)
        {
            return new byte[8]
            {
                (byte)((input      ) & 0xff),
                (byte)((input >>  8) & 0xff),
                (byte)((input >> 16) & 0xff),
                (byte)((input >> 24) & 0xff),
                (byte)((input >> 32) & 0xff),
                (byte)((input >> 40) & 0xff),
                (byte)((input >> 48) & 0xff),
                (byte)((input >> 56) & 0xff),
            };
        }

        public static byte[] DWORDToBytes(UInt32 input)
        {
            return new byte[4]
            {
                (byte)((input      ) & 0xff),
                (byte)((input >>  8) & 0xff),
                (byte)((input >> 16) & 0xff),
                (byte)((input >> 24) & 0xff),
            };
        }

        public static byte[] WORDToBytes(UInt16 input)
        {
            return new byte[2]
            {
                (byte)((input      ) & 0xff),
                (byte)((input >>  8) & 0xff),
            };
        }

        public static byte[] CombineByteArrays(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new ZLIBStream(compressedStream, System.IO.Compression.CompressionLevel.Optimal))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }



    }
}
