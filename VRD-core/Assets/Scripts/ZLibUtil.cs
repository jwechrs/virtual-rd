using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
namespace ZLibUtil{
    public class ZLibDecompressStream {
        private MemoryStream compressedByteStream;
        private DeflateStream deflateStream;
        private MemoryStream decompressedByteStream;
        private int compressedByteArrayHead = 0;
        public void initStream(){
            //compressedByteStream = new byte[0];
            compressedByteStream = new MemoryStream();
            deflateStream = new DeflateStream(
                                              compressedByteStream,
                                              CompressionMode.Decompress);
            decompressedByteStream = new MemoryStream();
        }
        public byte[] decompress(byte[] compressedBytes){
            compressedByteStream.Position = compressedByteArrayHead;
            compressedByteStream.Write(compressedBytes, 0, compressedBytes.Length);
            compressedByteStream.SetLength(compressedBytes.Length);
            compressedByteStream.Position = compressedByteArrayHead;
            //Debug.Log("compres")
            compressedByteArrayHead += compressedBytes.Length;
            //compressedByteStream.Flush();
            //compressedByteStream = new MemoryStream(compressedBytes);
            Debug.Log("compressedbytestream length~" + compressedByteStream.Length);
            deflateStream = new DeflateStream(
                                              compressedByteStream,
                                              CompressionMode.Decompress);
            //MemoryStream decompressedStream = new MemoryStream();
            // using (MemoryStream decompressedStream = new MemoryStream()){
            //     deflateStream.CopyTo(decompressedStream);
            //     return decompressedStream.ToArray();
            // }
            //deflateStream.CopyTo(decompressedByteStream);
            //deflateStream.
            //deflateStream.Close();
            //Debug.Log("decompress length=" + deflateStream.Length);
            //return decompressedByteStream.ToArray();
            byte[] decompressedByte = new byte[10];
            int count = deflateStream.Read(decompressedByte, 0, 8);
            Debug.Log("count=" + count.ToString());
            return decompressedByte;
        }
        public void close(){
            compressedByteStream.Dispose();
            deflateStream.Dispose();
        }
    }
}
