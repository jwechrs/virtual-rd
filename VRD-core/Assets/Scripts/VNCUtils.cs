using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
namespace NetworkUtil{
    public class BufferedNetworkStream{
        private byte[] bufferedBytes;
        private int bufferedBytesLength = 0;
        public NetworkStream networkStream;
        private int bufferedBytesHead = 0;
        private int bufferedBytesTail = 0;
        public void initBuffedBytes(int length){
            bufferedBytes = new byte[length];
            bufferedBytesLength = length;
        }
        public void setNetworkStream(NetworkStream stream){
            networkStream = stream;
        }
        private void doubleBufferSize(){
            byte[] newBufferedBytes = new byte[bufferedBytesLength * 2];
            Array.Copy(bufferedBytes, 0, newBufferedBytes, 0, bufferedBytesLength);
            this.bufferedBytes = newBufferedBytes;
            bufferedBytesLength *= 2;
        }
        public void readTillEnd(){
            // NetworkStreamの最後まで読み込む
            // リクエストに応じて一部を読み出す
            // 長さを超える場合はx2にする
            if (bufferedBytesHead != 0){
                throw new Exception("head must be 0");
            }
            int length;
            int readNetworkHead = 0;
            int bufferRemainingSize = bufferedBytesLength - readNetworkHead;
            Debug.Log("head=" + readNetworkHead.ToString() + ", remeaining=" + bufferRemainingSize.ToString());
            while ((length = networkStream.Read(bufferedBytes, readNetworkHead, bufferRemainingSize)) != 0){
                Debug.Log("length=" + length.ToString());
                readNetworkHead += length;
                if (readNetworkHead == bufferedBytesLength)
                {
                    this.doubleBufferSize();
                }
                bufferRemainingSize = bufferedBytesLength - readNetworkHead;
                Debug.Log("head=" + readNetworkHead.ToString() + ", remeaining=" + bufferRemainingSize.ToString());
            }
            Debug.Log("network flushed. Read " + readNetworkHead.ToString() + "bytes.");
            bufferedBytesTail = readNetworkHead;
        }
        private void logParams(){
            // Debug.Log("bufferedBytesH=" + bufferedBytesHead.ToString());
            // Debug.Log("bufferedBytesT=" + bufferedBytesTail.ToString());
            //Debug.Log("bufferedBytesL=" + bufferedBytesLength.ToString());
        }
        private void headZeroPosition(){
            if (bufferedBytesHead != bufferedBytesLength){
                throw new Exception("element remaining at the end of the buffer");
            }
            if (bufferedBytesTail != bufferedBytesLength){
                throw new Exception("Invalid tail position");
            }
            // bufferedBytes[]は全て読みきったときに実行
            // headを0に戻す
            bufferedBytesHead = 0;
            bufferedBytesTail = 0;
        }
        public void ReadStreamToByteArray(int length, byte[] destArray, int destArrayHead){
            //bufferedBytesHead += length;
            //Debug.Log("head=" + bufferedBytesHead);// max 6366
            //return VNCUtils.VNCController.streamReadTillEnd(networkStream, length);
            byte[] outputBytes = new byte[length];
            if (bufferedBytesHead + length <= bufferedBytesLength){
                if (bufferedBytesHead + length <= bufferedBytesTail){
                    //Debug.Log("buffered content ready");
                    logParams();
                    //Array.Copy(
                    // System.Buffer.BlockCopy(
                    //     bufferedBytes, bufferedBytesHead, outputBytes, 0, length
                    // );
                    System.Buffer.BlockCopy
                        (bufferedBytes, bufferedBytesHead, destArray, destArrayHead, length);
                    bufferedBytesHead += length;
                    //return outputBytes;
                    return;
                } else {
                    int remainingLength = bufferedBytesLength - bufferedBytesTail;
                    // if (remainingLength >= 10){
                    //     remainingLength = 10;
                    // }
                    //Debug.Log("requesting length=" + remainingLength.ToString());
                    //Debug.Log("Network stream read start=" + System.DateTime.Now.Millisecond);
                    Debug.Log("time-startDecoding" + System.DateTime.Now.Millisecond);
                    int readLength = networkStream.Read(
                        bufferedBytes, bufferedBytesTail, remainingLength
                    );
                    //Debug.Log("buffered content fetching");
                    //logParams();
                    Debug.Log("Network stream read finish=" + System.DateTime.Now.Millisecond);
                    //Debug.Log("fetched number of bytes=" + readLength.ToString());
                    bufferedBytesTail += readLength;
                    //return ReadStream(length);
                    //return;
                    ReadStreamToByteArray(length, destArray, destArrayHead);
                }
            } else {
                //throw new Exception("length + head exceeds buffer array length");
                Debug.Log("length + head exceeds buffer array length");
                int firstHalfLength = bufferedBytesLength - bufferedBytesHead;
                int lastHalfLength = length - firstHalfLength;
                //byte[] firstHalf = ReadStream(firstHalfLength);
                ReadStreamToByteArray(firstHalfLength, destArray, destArrayHead);
                //destArrayHead += firstHalfLength;
                this.headZeroPosition();
                //byte[] lastHalf = ReadStream(lastHalfLength);
                ReadStreamToByteArray(lastHalfLength, destArray, destArrayHead + firstHalfLength);
                //byte[] totalBytes = new byte[length];
                // if (firstHalf.Length + lastHalf.Length != length){
                //     throw new Exception("could not fetch specified length!");
                // }
                //Array.Copy(firstHalf, 0, totalBytes, 0, firstHalf.Length);
                //Array.Copy(lastHalf, 0, totalBytes, firstHalf.Length, lastHalf.Length);
                //return totalBytes;
                //System.
            }
        }
        public int CompatRead(byte[] outputBytes, int outputBytesHead, int numberOfBytes){
            // For compatibility with NetworkStream#Read(bytes, head, length);
            if (bufferedBytesHead < bufferedBytesTail){
                // bufferにデータが残っている
                int remainingLength = bufferedBytesTail - bufferedBytesHead;
                int requestedLength = numberOfBytes;
                if (requestedLength >= remainingLength){
                    requestedLength = remainingLength;
                }
                //System.Buffer.BlockCopy(bufferedBytes, bufferedBytesHead, outputBytes, outputBytesHead,bufferedBytesTail-bufferedBytesHead);
                System.Buffer.BlockCopy(bufferedBytes, bufferedBytesHead, outputBytes, outputBytesHead, requestedLength);
                bufferedBytesHead += requestedLength;
                return requestedLength;
            } else {
                // bufferに残っているデータはない
                // int bufferRemainingLength = bufferedBytesLength - bufferedBytesHead;
                // if (numberOfBytes <= bufferRemainingLength){
                // }
                return networkStream.Read(outputBytes, outputBytesHead, numberOfBytes);
            }
        }

        public byte[] ReadStream(int length){
            //bufferedBytesHead += length;
            //Debug.Log("head=" + bufferedBytesHead);// max 6366
            //return VNCUtils.VNCController.streamReadTillEnd(networkStream, length);
            byte[] outputBytes = new byte[length];
            if (bufferedBytesHead + length <= bufferedBytesLength){
                if (bufferedBytesHead + length <= bufferedBytesTail){
                    //Debug.Log("buffered content ready");
                    logParams();
                    //Array.Copy(
                    System.Buffer.BlockCopy(
                        bufferedBytes, bufferedBytesHead, outputBytes, 0, length
                    );
                    bufferedBytesHead += length;
                    return outputBytes;
                } else {
                    int remainingLength = bufferedBytesLength - bufferedBytesTail;
                    // if (remainingLength >= 10){
                    //     remainingLength = 10;
                    // }
                    //Debug.Log("requesting length=" + remainingLength.ToString());
                    //Debug.Log("Network stream read start=" + System.DateTime.Now.Millisecond);
                    int readLength = networkStream.Read(
                        bufferedBytes, bufferedBytesTail, remainingLength
                    );
                    //Debug.Log("buffered content fetching");
                    //logParams();
                    //Debug.Log("Network stream read finish=" + System.DateTime.Now.Millisecond);
                    //Debug.Log("fetched number of bytes=" + readLength.ToString());
                    bufferedBytesTail += readLength;
                    return ReadStream(length);
                }
            } else {
                //throw new Exception("length + head exceeds buffer array length");
                Debug.Log("length + head exceeds buffer array length");
                int firstHalfLength = bufferedBytesLength - bufferedBytesHead;
                int lastHalfLength = length - firstHalfLength;
                byte[] firstHalf = ReadStream(firstHalfLength);
                this.headZeroPosition();
                byte[] lastHalf = ReadStream(lastHalfLength);
                byte[] totalBytes = new byte[length];
                if (firstHalf.Length + lastHalf.Length != length){
                    throw new Exception("could not fetch specified length!");
                }
                Array.Copy(firstHalf, 0, totalBytes, 0, firstHalf.Length);
                Array.Copy(lastHalf, 0, totalBytes, firstHalf.Length, lastHalf.Length);
                return totalBytes;
            }
        }
    }
    public class BytesToType{
        public static int fourBytesToInt(byte[] bytes){
            return bytes[0]<<24 | bytes[1] << 16 | bytes[2]<<8|bytes[3];
        }
    }
}


namespace PixelEncodeDecoder{
    public class ZRLEDecoder{
        private ZLibUtil.ZLibDecompressStream zlibDecompressStream;
        public void initZlibStream(){
        }
        public void decodeToColors(
            NetworkUtil.BufferedNetworkStream bufferedNetworkStream,
            Color[] imageColors,
            int rectangleX, int rectangleY,
            int rectangleWidth, int rectangleHeight, int bytesPerPixel,
            int imageWidth, int imageHeight){
            int length = NetworkUtil.BytesToType.fourBytesToInt(bufferedNetworkStream.ReadStream(4));
            byte[] zlibDecompressedBytes = zlibDecompressStream.decompress(bufferedNetworkStream.ReadStream(length));
            byte subencoding = zlibDecompressedBytes[0];
            int subencodingInt = (int)(subencoding & 0x7F);
            Debug.Log("subencoding=" + subencodingInt.ToString());
        }
    }
    namespace HextileSubDecoder {
        // decode sub-rectangle (16x16)
        public class SubrectsColouredDecoder {
            private int rectangleX;
            private int rectangleY;
            private byte[] subRectangleBytes;
            private int bytesPerPixel;
            private int startX;
            private int startY;
            private int imageWidth;
            private int numberOfSubrectangles;
            public void prepare(byte[] subRectangleBytes, int rectangleX, int rectangleY,
                                int startX, int startY,
                                int imageWidth,
                                int numberOfSubrectangles,
                                int bytesPerPixel){
                this.startX = startX;
                this.startY = startY;
                this.rectangleX = rectangleX;
                this.rectangleY = rectangleY;
                this.subRectangleBytes = subRectangleBytes;
                this.bytesPerPixel = bytesPerPixel;
                this.imageWidth = imageWidth;
                this.numberOfSubrectangles = numberOfSubrectangles;
            }
            public void decodeBytes(Color32[] imageColors){
                int subRectangleColorByteIndex = 0;
                int subRectangleColorByteIncrement = 2 + bytesPerPixel;
                int xy;
                int wh;
                int h;
                int w;
                int x;
                int y;
                Color32 rectanglePixel = new Color32(0, 0, 0, 255);
                for(int subRectangleIndex=0; subRectangleIndex<numberOfSubrectangles; subRectangleIndex++){
                    // rectanglePixel = VNCUtils.VNCController.byteArrayToColor32(
                    //     subRectangleBytes, bytesPerPixel,
                    //     //subRectangleIndex * (bytesPerPixel+2)
                    //     subRectangleColorByteIndex
                    // );
                    rectanglePixel.b = subRectangleBytes[subRectangleColorByteIndex];
                    rectanglePixel.g = subRectangleBytes[subRectangleColorByteIndex+1];
                    rectanglePixel.r = subRectangleBytes[subRectangleColorByteIndex+2];
                    //int xy = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                    //int xy  = subRectangleBytes[subRectangleIndex * (bytesPerPixel +2) + bytesPerPixel];
                    subRectangleColorByteIndex += bytesPerPixel;
                    xy  = subRectangleBytes[subRectangleColorByteIndex ++];
                    x = (xy >> 4) + startX;
                    y = (xy & 0x0F) + startY;
                    //int wh = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                    //int wh = subRectangleBytes[subRectangleIndex * (bytesPerPixel +2) + bytesPerPixel + 1];
                    wh = subRectangleBytes[subRectangleColorByteIndex ++];
                    w = (wh >> 4) + 1;
                    h = (wh & 0x0F) + 1;
                    HextileDecoder.fillRectangle(
                        imageColors, rectanglePixel,
                        x + rectangleX,
                        y + rectangleY,
                        w, h,
                        imageWidth);
                    //subRectangleColorByteIndex += subRectangleColorByteIncrement;
                }
            }
        }
    }
    public class HextileDecoder{
        private byte[] segmentedBytes;
        private int rectangleX;
        private int rectangleY;
        private int rectangleHeight;
        private int rectangleWidth;
        private int imageWidth;
        private int imageHeight;
        private int bytesPerPixel;
        public static void setColor(
            Color32[] imageColors, Color32 pixelColor,
            int x, int y, int imageWidth){
            //return;
            int colorArrayIndex = imageWidth * y + (imageWidth-x-1);
            imageColors[colorArrayIndex] = pixelColor;
        }
        public static void fillRectangle(
            //Color[] imageColors, Color fillColor,
            Color32[] imageColors, Color32 fillColor,
            int x, int y, int w, int h, int imageWidth){
            int maxX = x + w;
            int maxY = y + h;
            int colorArrayIndex = 0;
            int colorArrayIndexYBase = y * imageWidth;
            for(int pixelY=y; pixelY<maxY; pixelY++){
                colorArrayIndex = colorArrayIndexYBase + imageWidth-(x+1);
                for(int pixelX=x; pixelX<maxX; pixelX ++){
                    //setColor(imageColors, fillColor, pixelX, pixelY, imageWidth);
                    imageColors[colorArrayIndex--] = fillColor;
                }
                colorArrayIndexYBase += imageWidth;
                //colorIndex += imageWidth;
            }
        }
        public void prepare(NetworkUtil.BufferedNetworkStream bufferedNetworkStream,
                            //Color[] imageColors,
                            //Color32[] imageColors,
                            int rectangleX, int rectangleY,
                            int rectangleWidth, int rectangleHeight, int bytesPerPixel,
                            int imageWidth, int imageHeight){
            this.segmentedBytes = segmentBytes
                (bufferedNetworkStream, rectangleX, rectangleY,
                 rectangleWidth, rectangleHeight, bytesPerPixel,
                 imageWidth, imageHeight);
            this.rectangleX = rectangleX;
            this.rectangleY = rectangleY;
            this.rectangleWidth = rectangleWidth;
            this.rectangleHeight = rectangleHeight;
            this.bytesPerPixel = bytesPerPixel;
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
        }
        public void decodeBytes(Color32[] imageColors){
            decodeBytes(this.segmentedBytes,
                        imageColors,
                        this.rectangleX, this.rectangleY,
                        this.rectangleWidth, this.rectangleHeight,
                        this.bytesPerPixel,
                        this.imageWidth,
                        this.imageHeight);
        }
        public void decodeBytesAsync(Color32[] imageColors){
            Thread t = new Thread(new ThreadStart(() =>{
                this.decodeBytes(imageColors);
            }));
            t.Start();
        }

            //Color32 backgroundColor = new )
        public static byte[] segmentBytes(
            //NetworkStream networkStream,
            NetworkUtil.BufferedNetworkStream bufferedNetworkStream,
            //Color[] imageColors,
            //Color32[] imageColors,
            int rectangleX, int rectangleY,
            int rectangleWidth, int rectangleHeight, int bytesPerPixel,
            int imageWidth, int imageHeight){
            //Color32 backgroundColor = new Color32(0, 0, 0, 255);
            //Color32 foregroundColor = new Color32(0, 0, 0, 255);
            //Color32 pixelColor;
            int x;
            int y;
            //Debug.Log("time-startDecoding" + System.DateTime.Now.Millisecond);
            //Debug.Log("bytesperPixel=" + bytesPerPixel.ToString());
            //int requestBytes = rectangleHeight * rectangleWidth * bytesPerPixel;
            int defaultTileByteCount = bytesPerPixel << 8; // 16 * 16 * bytesPerPixel
            bool nonDefaultByteCount = false;
            int rawPixelBytesCount = defaultTileByteCount;
            int bufferBytesSize = 1920 * 1080 * 4;
            byte[] rectangleBytes = new byte[bufferBytesSize];
            int rectangleBytesHead = 0;

            for(int startY=0; startY < rectangleHeight; startY += 16){
                for(int startX=0; startX < rectangleWidth; startX += 16){
                    nonDefaultByteCount = false;
                    int numberOfSubrectangles = 0;
                    //byte[] subencoding = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1);
                    byte[] subencoding = bufferedNetworkStream.ReadStream(1);
                    int subencodingInt = (int)subencoding[0];
                    rectangleBytes[rectangleBytesHead++] = subencoding[0];
                    //Debug.Log("subEncoding=" + subencodingInt.ToString());
                    if (subencodingInt >= 32){
                        Debug.Log("Invalid subencoding=" + subencodingInt.ToString());
                        //Debug.Log("subencoding=" + subencodingInt.ToString());
                        throw new Exception("Invalid subencoding");
                    }
                    int tileW = 16;
                    if (startX + 16 >=rectangleWidth){
                        tileW = rectangleWidth-startX;
                        nonDefaultByteCount = true;
                    }
                    int tileH = 16;
                    if (startY + 16 >= rectangleHeight){
                        tileH = rectangleHeight-startY;
                        nonDefaultByteCount = true;
                    }
                    if ((subencodingInt & 0x01) != 0){
                        //Debug.Log("raw encoding tile W=" + tileW.ToString() + ", tileH=" + tileH.ToString());
                        // fillRectangle(
                        //     imageColors, backgroundColor, 
                        //     startX, startY, tileW, tileH, 
                        //     imageWidth
                        // );
                        if (nonDefaultByteCount){
                            rawPixelBytesCount = tileW * tileH * bytesPerPixel;
                        } else {
                            rawPixelBytesCount = defaultTileByteCount;
                        }
                        //byte[] rawPixelBytes = VNCUtils.VNCController.streamReadTillEnd(networkStream, rawPixelBytesCount);
                        // byte[] rawPixelBytes = bufferedNetworkStream.ReadStream(rawPixelBytesCount);
                        // Array.Copy(rawPixelBytes, 0, rectangleBytes, rectangleBytesHead, rawPixelBytesCount);
                        bufferedNetworkStream.ReadStreamToByteArray
                            (rawPixelBytesCount, rectangleBytes, rectangleBytesHead);
                        rectangleBytesHead += rawPixelBytesCount;
                        // x = 0;
                        // y = 0;
                        // for(int rawByteIndex=0; rawByteIndex < rawPixelBytesCount; rawByteIndex += bytesPerPixel){
                        //     pixelColor = VNCUtils.VNCController.byteArrayToColor32(
                        //         rawPixelBytes, bytesPerPixel, rawByteIndex
                        //     );
                        //     setColor(
                        //         imageColors, pixelColor, x + startX + rectangleX, 
                        //         y + startY + rectangleY, imageWidth);
                        //     x ++;
                        //     if (x >= tileW){
                        //         y ++;
                        //         x = 0;
                        //     }
                        // }
                        // continue;
                    }
                    if ((subencodingInt & 0x02) != 0){ // Background Specified
                        //Debug.Log("background specified");
                        //byte[] backgroundColorByte = VNCUtils.VNCController.streamReadTillEnd(networkStream, bytesPerPixel);
                        //byte[] backgroundColorByte = bufferedNetworkStream.ReadStream(bytesPerPixel);
                        //backgroundColor = VNCUtils.VNCController.byteArrayToColor32(backgroundColorByte, bytesPerPixel);
                        //Array.Copy(backgroundColorByte, 0, rectangleBytes, rectangleBytesHead, bytesPerPixel);
                        bufferedNetworkStream.ReadStreamToByteArray(bytesPerPixel, rectangleBytes, rectangleBytesHead);
                        rectangleBytesHead += bytesPerPixel;
                    }
                    if ((subencodingInt & 0x04) != 0){
                        //Debug.Log("foreground specified");
                        // foregroundColor = VNCUtils.VNCController.byteArrayToColor32(
                        //     //VNCUtils.VNCController.streamReadTillEnd(networkStream, bytesPerPixel), bytesPerPixel
                        //     bufferedNetworkStream.ReadStream(bytesPerPixel), bytesPerPixel
                        // );

                        //Array.Copy(bufferedNetworkStream.ReadStream(bytesPerPixel),
                        //           0, rectangleBytes, rectangleBytesHead, bytesPerPixel);
                        bufferedNetworkStream.ReadStreamToByteArray(bytesPerPixel, rectangleBytes, rectangleBytesHead);
                        rectangleBytesHead += bytesPerPixel;
                    }
                    // fillRectangle(
                    //     imageColors, backgroundColor, 
                    //     startX + rectangleX,
                    //     startY + rectangleY,
                    //     tileW, tileH,
                    //     imageWidth
                    // );
                    if ((subencodingInt & 0x08) != 0){
                        //Debug.Log("number of sub-rectangles");
                        //numberOfSubrectangles = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                        numberOfSubrectangles = bufferedNetworkStream.ReadStream(1)[0];
                        rectangleBytes[rectangleBytesHead++] = (byte)numberOfSubrectangles;
                        //Debug.Log("number of sub-rectangles=" + numberOfSubrectangles.ToString());
                    }
                    if (numberOfSubrectangles >= 1){
                        if ((subencodingInt & 16) != 0){
                            //Debug.Log("colour");
                            //byte[] subRectangleBytes = VNCUtils.VNCController.streamReadTillEnd(networkStream, (bytesPerPixel + 2) * numberOfSubrectangles
                            //);
                            //byte[] subRectangleBytes = bufferedNetworkStream.ReadStream((2+bytesPerPixel) * numberOfSubrectangles);
                            int subRectangleColorByteIndex = 0;
                              int subRectangleColorByteIncrement = 2 + bytesPerPixel;
                              int xy;
                              int wh;
                              int h;
                              int w;
                              //Color32 rectanglePixel = new Color32(0, 0, 0, 255);
                              //Array.Copy(subRectangleBytes, 0, rectangleBytes, rectangleBytesHead, (2+bytesPerPixel) * numberOfSubrectangles);
                              int numberOfBytes = (2 + bytesPerPixel) * numberOfSubrectangles;
                              bufferedNetworkStream.ReadStreamToByteArray
                                  (numberOfBytes, rectangleBytes, rectangleBytesHead);
                              //rectangleBytesHead += (2+bytesPerPixel) * numberOfSubrectangles;
                              rectangleBytesHead += numberOfBytes;

                              // for(int subRectangleIndex=0; subRectangleIndex<numberOfSubrectangles; subRectangleIndex++){
                              //     // rectanglePixel = VNCUtils.VNCController.byteArrayToColor32(
                              //     //     subRectangleBytes, bytesPerPixel,
                              //     //     //subRectangleIndex * (bytesPerPixel+2)
                              //     //     subRectangleColorByteIndex
                              //     // );
                              //     rectanglePixel.b = subRectangleBytes[subRectangleColorByteIndex];
                              //     rectanglePixel.g = subRectangleBytes[subRectangleColorByteIndex+1];
                              //     rectanglePixel.r = subRectangleBytes[subRectangleColorByteIndex+2];
                              //     //int xy = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                              //     //int xy  = subRectangleBytes[subRectangleIndex * (bytesPerPixel +2) + bytesPerPixel];
                              //     subRectangleColorByteIndex += bytesPerPixel;
                              //     xy  = subRectangleBytes[subRectangleColorByteIndex ++];
                              //     x = (xy >> 4) + startX;
                              //     y = (xy & 0x0F) + startY;
                              //     //int wh = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                              //     //int wh = subRectangleBytes[subRectangleIndex * (bytesPerPixel +2) + bytesPerPixel + 1];
                              //     wh = subRectangleBytes[subRectangleColorByteIndex ++];
                              //     w = (wh >> 4) + 1;
                              //     h = (wh & 0x0F) + 1;
                              //     fillRectangle(
                              //         imageColors, rectanglePixel,
                              //         x + rectangleX,
                              //         y + rectangleY,
                              //         w, h,
                              //         imageWidth);
                              //     //subRectangleColorByteIndex += subRectangleColorByteIncrement;
                              // }

                            // HextileSubDecoder.SubrectsColouredDecoder decoder = new HextileSubDecoder.SubrectsColouredDecoder();
                            // decoder.prepare(subRectangleBytes,
                            //                 rectangleX, rectangleY,
                            //                 startX, startY,
                            //                 imageWidth, numberOfSubrectangles,
                            //                 bytesPerPixel);
                            // Thread t = new Thread(new ThreadStart(() => {
                            //     decoder.decode(imageColors);
                            //                              }));
                            // t.Start();
                        } else {
                            // byte[] xywhBytes = VNCUtils.VNCController.streamReadTillEnd(
                            //     networkStream, 2 * numberOfSubrectangles
                            // );
                            //byte[] xywhBytes = bufferedNetworkStream.ReadStream(2 * numberOfSubrectangles);
                            //int subRectangleByteIndex = 0;
                            //Array.Copy(xywhBytes, 0, rectangleBytes, rectangleBytesHead, 2 * numberOfSubrectangles);
                            bufferedNetworkStream.ReadStreamToByteArray(2 * numberOfSubrectangles, rectangleBytes, rectangleBytesHead);
                            rectangleBytesHead += 2 *numberOfSubrectangles;
                            // for(int subRectangleIndex=0; subRectangleIndex<numberOfSubrectangles; subRectangleIndex++){
                            //     //byte[] xywhBytes = VNCUtils.VNCController.streamReadTillEnd(networkStream, 2);
                            //     int xy = xywhBytes[subRectangleByteIndex];
                            //     //int xy = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                            //     x = (xy >> 4) + startX;
                            //     y = (xy & 0x0F) + startY;
                            //     int wh = xywhBytes[subRectangleByteIndex+1];
                            //     //int wh = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                            //     int w = (wh >> 4) + 1;
                            //     int h = (wh & 0x0F) + 1;
                            //     fillRectangle(
                            //         imageColors, foregroundColor,
                            //         x +rectangleX,
                            //         y + rectangleY,
                            //         w, h, imageWidth
                            //     );
                            //     subRectangleByteIndex += 2;
                            // }
                        }
                    }
                }
                //startX += 16;
            }
            //Debug.Log("time-decode finish:" + System.DateTime.Now.Millisecond);
            return rectangleBytes;
        }
         public static void decodeBytes(
            //NetworkStream networkStream,
            //NetworkUtil.BufferedNetworkStream bufferedNetworkStream,
                                        byte[] rfbBytes,
            //Color[] imageColors,
            Color32[] imageColors,
            int rectangleX, int rectangleY,
            int rectangleWidth, int rectangleHeight, int bytesPerPixel,
            int imageWidth, int imageHeight){
            Color32 backgroundColor = new Color32(0, 0, 0, 255);
            Color32 foregroundColor = new Color32(0, 0, 0, 255);
            Color32 pixelColor;
            int x;
            int y;
            int rfbBytesHead = 0;
            //Debug.Log("time-startDecoding" + System.DateTime.Now.Millisecond);
            //Debug.Log("bytesperPixel=" + bytesPerPixel.ToString());
            //int requestBytes = rectangleHeight * rectangleWidth * bytesPerPixel;
            int defaultTileByteCount = bytesPerPixel << 8; // 16 * 16 * bytesPerPixel
            bool nonDefaultByteCount = false;
            int rawPixelBytesCount = defaultTileByteCount;

            for(int startY=0; startY < rectangleHeight; startY += 16){
                for(int startX=0; startX < rectangleWidth; startX += 16){
                    nonDefaultByteCount = false;
                    int numberOfSubrectangles = 0;
                    //byte[] subencoding = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1);
                    //byte[] subencoding = rfbBytes[rfbBytesHead];//bufferedNetworkStream.ReadStream(1);
                    int subencodingInt = rfbBytes[rfbBytesHead++];//(int)subencoding[0];
                    //Debug.Log("subEncoding=" + subencodingInt.ToString());
                    if (subencodingInt >= 32){
                        Debug.Log("Invalid subencoding=" + subencodingInt.ToString());
                        //Debug.Log("subencoding=" + subencodingInt.ToString());
                        throw new Exception("Invalid subencoding");
                        return;
                    }
                    int tileW = 16;
                    if (startX + 16 >=rectangleWidth){
                        tileW = rectangleWidth-startX;
                        nonDefaultByteCount = true;
                    }
                    int tileH = 16;
                    if (startY + 16 >= rectangleHeight){
                        tileH = rectangleHeight-startY;
                        nonDefaultByteCount = true;
                    }
                    if ((subencodingInt & 0x01) != 0){
                        //Debug.Log("raw encoding tile W=" + tileW.ToString() + ", tileH=" + tileH.ToString());
                        // fillRectangle(
                        //     imageColors, backgroundColor, 
                        //     startX, startY, tileW, tileH, 
                        //     imageWidth
                        // );
                        if (nonDefaultByteCount){
                            rawPixelBytesCount = tileW * tileH * bytesPerPixel;
                        } else {
                            rawPixelBytesCount = defaultTileByteCount;
                        }
                        //byte[] rawPixelBytes = VNCUtils.VNCController.streamReadTillEnd(networkStream, rawPixelBytesCount);
                        //byte[] rawPixelBytes = bufferedNetworkStream.ReadStream(rawPixelBytesCount);
                        x = 0;
                        y = 0;
                        for(int rawByteIndex=0; rawByteIndex < rawPixelBytesCount; rawByteIndex += bytesPerPixel){
                            pixelColor = VNCUtils.VNCController.byteArrayToColor32(
                                //rawPixelBytes,
                                                                                   rfbBytes,
                                                                                   bytesPerPixel, rawByteIndex + rfbBytesHead
                            );
                            setColor(
                                imageColors, pixelColor, x + startX + rectangleX, 
                                y + startY + rectangleY, imageWidth);
                            x ++;
                            if (x >= tileW){
                                y ++;
                                x = 0;
                            }
                        }
                        rfbBytesHead += rawPixelBytesCount;
                        continue;
                    }
                    if ((subencodingInt & 0x02) != 0){ // Background Specified
                        //Debug.Log("background specified");
                        //byte[] backgroundColorByte = VNCUtils.VNCController.streamReadTillEnd(networkStream, bytesPerPixel);
                        //byte[] backgroundColorByte = bufferedNetworkStream.ReadStream(bytesPerPixel);
                        //backgroundColor = VNCUtils.VNCController.byteArrayToColor32(backgroundColorByte, bytesPerPixel);
                        backgroundColor = VNCUtils.VNCController.byteArrayToColor32
                            (rfbBytes, bytesPerPixel, rfbBytesHead);
                        rfbBytesHead += bytesPerPixel;
                    }
                    if ((subencodingInt & 0x04) != 0){
                        //Debug.Log("foreground specified");
                        foregroundColor = VNCUtils.VNCController.byteArrayToColor32(
                            //VNCUtils.VNCController.streamReadTillEnd(networkStream, bytesPerPixel), bytesPerPixel
                            //bufferedNetworkStream.ReadStream(bytesPerPixel), bytesPerPixel
                                                                                    rfbBytes, bytesPerPixel, rfbBytesHead
                        );
                        rfbBytesHead += bytesPerPixel;
                    }
                    fillRectangle(
                        imageColors, backgroundColor, 
                        startX + rectangleX,
                        startY + rectangleY,
                        tileW, tileH,
                        imageWidth
                    );
                    if ((subencodingInt & 0x08) != 0){
                        //Debug.Log("number of sub-rectangles");
                        //numberOfSubrectangles = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                        //numberOfSubrectangles = bufferedNetworkStream.ReadStream(1)[0];
                        numberOfSubrectangles = rfbBytes[rfbBytesHead++];
                        //Debug.Log("number of sub-rectangles=" + numberOfSubrectangles.ToString());
                    }
                    if (numberOfSubrectangles >= 1){
                        if ((subencodingInt & 16) != 0){
                            //Debug.Log("colour");
                            //byte[] subRectangleBytes = VNCUtils.VNCController.streamReadTillEnd(networkStream, (bytesPerPixel + 2) * numberOfSubrectangles
                            //);
                            //byte[] subRectangleBytes = bufferedNetworkStream.ReadStream((2+bytesPerPixel) * numberOfSubrectangles);
                            int subRectangleColorByteIndex = rfbBytesHead;
                              int subRectangleColorByteIncrement = 2 + bytesPerPixel;
                              int xy;
                              int wh;
                              int h;
                              int w;
                              Color32 rectanglePixel = new Color32(0, 0, 0, 255);
                              for(int subRectangleIndex=0; subRectangleIndex<numberOfSubrectangles; subRectangleIndex++){
                                  // rectanglePixel = VNCUtils.VNCController.byteArrayToColor32(
                                  //     subRectangleBytes, bytesPerPixel,
                                  //     //subRectangleIndex * (bytesPerPixel+2)
                                  //     subRectangleColorByteIndex
                                  // );
                                  rectanglePixel.b = rfbBytes[subRectangleColorByteIndex];
                                  rectanglePixel.g = rfbBytes[subRectangleColorByteIndex+1];
                                  rectanglePixel.r = rfbBytes[subRectangleColorByteIndex+2];
                                  //int xy = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                                  //int xy  = subRectangleBytes[subRectangleIndex * (bytesPerPixel +2) + bytesPerPixel];
                                  subRectangleColorByteIndex += bytesPerPixel;
                                  xy  = rfbBytes[subRectangleColorByteIndex ++];
                                  x = (xy >> 4) + startX;
                                  y = (xy & 0x0F) + startY;
                                  //int wh = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                                  //int wh = subRectangleBytes[subRectangleIndex * (bytesPerPixel +2) + bytesPerPixel + 1];
                                  wh = rfbBytes[subRectangleColorByteIndex ++];
                                  w = (wh >> 4) + 1;
                                  h = (wh & 0x0F) + 1;
                                  fillRectangle(
                                      imageColors, rectanglePixel,
                                      x + rectangleX,
                                      y + rectangleY,
                                      w, h,
                                      imageWidth);
                                  //subRectangleColorByteIndex += subRectangleColorByteIncrement;
                              }
                              rfbBytesHead = subRectangleColorByteIndex;

                            // HextileSubDecoder.SubrectsColouredDecoder decoder = new HextileSubDecoder.SubrectsColouredDecoder();
                            // decoder.prepare(subRectangleBytes,
                            //                 rectangleX, rectangleY,
                            //                 startX, startY,
                            //                 imageWidth, numberOfSubrectangles,
                            //                 bytesPerPixel);
                            // Thread t = new Thread(new ThreadStart(() => {
                            //     decoder.decode(imageColors);
                            //                              }));
                            // t.Start();
                        } else {
                            // byte[] xywhBytes = VNCUtils.VNCController.streamReadTillEnd(
                            //     networkStream, 2 * numberOfSubrectangles
                            // );
                            //byte[] xywhBytes = bufferedNetworkStream.ReadStream(2 * numberOfSubrectangles);
                            int subRectangleByteIndex = rfbBytesHead;
                            for(int subRectangleIndex=0; subRectangleIndex<numberOfSubrectangles; subRectangleIndex++){
                                //byte[] xywhBytes = VNCUtils.VNCController.streamReadTillEnd(networkStream, 2);
                                //int xy = xywhBytes[subRectangleByteIndex];
                                int xy = rfbBytes[rfbBytesHead++];
                                //int xy = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                                x = (xy >> 4) + startX;
                                y = (xy & 0x0F) + startY;
                                int wh = rfbBytes[rfbBytesHead++];//xywhBytes[subRectangleByteIndex+1];
                                //int wh = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                                int w = (wh >> 4) + 1;
                                int h = (wh & 0x0F) + 1;
                                fillRectangle(
                                    imageColors, foregroundColor,
                                    x +rectangleX,
                                    y + rectangleY,
                                    w, h, imageWidth
                                );
                                subRectangleByteIndex += 2;
                            }
                        }
                    }
                }
                //startX += 16;
            }
            //Debug.Log("time-decode finish:" + System.DateTime.Now.Millisecond);
        }
        public static void decodeToColors(
            //NetworkStream networkStream,
            NetworkUtil.BufferedNetworkStream bufferedNetworkStream,
            //Color[] imageColors,
            Color32[] imageColors,
            int rectangleX, int rectangleY,
            int rectangleWidth, int rectangleHeight, int bytesPerPixel,
            int imageWidth, int imageHeight){
            Color32 backgroundColor = new Color32(0, 0, 0, 255);
            Color32 foregroundColor = new Color32(0, 0, 0, 255);
            Color32 pixelColor;
            int x;
            int y;
            //Debug.Log("time-startDecoding" + System.DateTime.Now.Millisecond);
            //Debug.Log("bytesperPixel=" + bytesPerPixel.ToString());
            //int requestBytes = rectangleHeight * rectangleWidth * bytesPerPixel;
            int defaultTileByteCount = bytesPerPixel << 8; // 16 * 16 * bytesPerPixel
            bool nonDefaultByteCount = false;
            int rawPixelBytesCount = defaultTileByteCount;

            for(int startY=0; startY < rectangleHeight; startY += 16){
                for(int startX=0; startX < rectangleWidth; startX += 16){
                    nonDefaultByteCount = false;
                    int numberOfSubrectangles = 0;
                    //byte[] subencoding = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1);
                    byte[] subencoding = bufferedNetworkStream.ReadStream(1);
                    int subencodingInt = (int)subencoding[0];
                    //Debug.Log("subEncoding=" + subencodingInt.ToString());
                    if (subencodingInt >= 32){
                        Debug.Log("Invalid subencoding=" + subencodingInt.ToString());
                        //Debug.Log("subencoding=" + subencodingInt.ToString());
                        throw new Exception("Invalid subencoding");
                        return;
                    }
                    int tileW = 16;
                    if (startX + 16 >=rectangleWidth){
                        tileW = rectangleWidth-startX;
                        nonDefaultByteCount = true;
                    }
                    int tileH = 16;
                    if (startY + 16 >= rectangleHeight){
                        tileH = rectangleHeight-startY;
                        nonDefaultByteCount = true;
                    }
                    if ((subencodingInt & 0x01) != 0){
                        //Debug.Log("raw encoding tile W=" + tileW.ToString() + ", tileH=" + tileH.ToString());
                        // fillRectangle(
                        //     imageColors, backgroundColor, 
                        //     startX, startY, tileW, tileH, 
                        //     imageWidth
                        // );
                        if (nonDefaultByteCount){
                            rawPixelBytesCount = tileW * tileH * bytesPerPixel;
                        } else {
                            rawPixelBytesCount = defaultTileByteCount;
                        }
                        //byte[] rawPixelBytes = VNCUtils.VNCController.streamReadTillEnd(networkStream, rawPixelBytesCount);
                        byte[] rawPixelBytes = bufferedNetworkStream.ReadStream(rawPixelBytesCount);
                        x = 0;
                        y = 0;
                        for(int rawByteIndex=0; rawByteIndex < rawPixelBytesCount; rawByteIndex += bytesPerPixel){
                            pixelColor = VNCUtils.VNCController.byteArrayToColor32(
                                rawPixelBytes, bytesPerPixel, rawByteIndex
                            );
                            setColor(
                                imageColors, pixelColor, x + startX + rectangleX, 
                                y + startY + rectangleY, imageWidth);
                            x ++;
                            if (x >= tileW){
                                y ++;
                                x = 0;
                            }
                        }
                        continue;
                    }
                    if ((subencodingInt & 0x02) != 0){ // Background Specified
                        //Debug.Log("background specified");
                        //byte[] backgroundColorByte = VNCUtils.VNCController.streamReadTillEnd(networkStream, bytesPerPixel);
                        byte[] backgroundColorByte = bufferedNetworkStream.ReadStream(bytesPerPixel);
                        backgroundColor = VNCUtils.VNCController.byteArrayToColor32(backgroundColorByte, bytesPerPixel);
                    }
                    if ((subencodingInt & 0x04) != 0){
                        //Debug.Log("foreground specified");
                        foregroundColor = VNCUtils.VNCController.byteArrayToColor32(
                            //VNCUtils.VNCController.streamReadTillEnd(networkStream, bytesPerPixel), bytesPerPixel
                            bufferedNetworkStream.ReadStream(bytesPerPixel), bytesPerPixel
                        );
                    }
                    fillRectangle(
                        imageColors, backgroundColor, 
                        startX + rectangleX,
                        startY + rectangleY,
                        tileW, tileH,
                        imageWidth
                    );
                    if ((subencodingInt & 0x08) != 0){
                        //Debug.Log("number of sub-rectangles");
                        //numberOfSubrectangles = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                        numberOfSubrectangles = bufferedNetworkStream.ReadStream(1)[0];
                        //Debug.Log("number of sub-rectangles=" + numberOfSubrectangles.ToString());
                    }
                    if (numberOfSubrectangles >= 1){
                        if ((subencodingInt & 16) != 0){
                            //Debug.Log("colour");
                            //byte[] subRectangleBytes = VNCUtils.VNCController.streamReadTillEnd(networkStream, (bytesPerPixel + 2) * numberOfSubrectangles
                            //);
                            byte[] subRectangleBytes = bufferedNetworkStream.ReadStream((2+bytesPerPixel) * numberOfSubrectangles);
                            int subRectangleColorByteIndex = 0;
                              int subRectangleColorByteIncrement = 2 + bytesPerPixel;
                              int xy;
                              int wh;
                              int h;
                              int w;
                              Color32 rectanglePixel = new Color32(0, 0, 0, 255);
                              for(int subRectangleIndex=0; subRectangleIndex<numberOfSubrectangles; subRectangleIndex++){
                                  // rectanglePixel = VNCUtils.VNCController.byteArrayToColor32(
                                  //     subRectangleBytes, bytesPerPixel,
                                  //     //subRectangleIndex * (bytesPerPixel+2)
                                  //     subRectangleColorByteIndex
                                  // );
                                  rectanglePixel.b = subRectangleBytes[subRectangleColorByteIndex];
                                  rectanglePixel.g = subRectangleBytes[subRectangleColorByteIndex+1];
                                  rectanglePixel.r = subRectangleBytes[subRectangleColorByteIndex+2];
                                  //int xy = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                                  //int xy  = subRectangleBytes[subRectangleIndex * (bytesPerPixel +2) + bytesPerPixel];
                                  subRectangleColorByteIndex += bytesPerPixel;
                                  xy  = subRectangleBytes[subRectangleColorByteIndex ++];
                                  x = (xy >> 4) + startX;
                                  y = (xy & 0x0F) + startY;
                                  //int wh = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                                  //int wh = subRectangleBytes[subRectangleIndex * (bytesPerPixel +2) + bytesPerPixel + 1];
                                  wh = subRectangleBytes[subRectangleColorByteIndex ++];
                                  w = (wh >> 4) + 1;
                                  h = (wh & 0x0F) + 1;
                                  fillRectangle(
                                      imageColors, rectanglePixel,
                                      x + rectangleX,
                                      y + rectangleY,
                                      w, h,
                                      imageWidth);
                                  //subRectangleColorByteIndex += subRectangleColorByteIncrement;
                              }

                            // HextileSubDecoder.SubrectsColouredDecoder decoder = new HextileSubDecoder.SubrectsColouredDecoder();
                            // decoder.prepare(subRectangleBytes,
                            //                 rectangleX, rectangleY,
                            //                 startX, startY,
                            //                 imageWidth, numberOfSubrectangles,
                            //                 bytesPerPixel);
                            // Thread t = new Thread(new ThreadStart(() => {
                            //     decoder.decode(imageColors);
                            //                              }));
                            // t.Start();
                        } else {
                            // byte[] xywhBytes = VNCUtils.VNCController.streamReadTillEnd(
                            //     networkStream, 2 * numberOfSubrectangles
                            // );
                            byte[] xywhBytes = bufferedNetworkStream.ReadStream(2 * numberOfSubrectangles);
                            int subRectangleByteIndex = 0;
                            for(int subRectangleIndex=0; subRectangleIndex<numberOfSubrectangles; subRectangleIndex++){
                                //byte[] xywhBytes = VNCUtils.VNCController.streamReadTillEnd(networkStream, 2);
                                int xy = xywhBytes[subRectangleByteIndex];
                                //int xy = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                                x = (xy >> 4) + startX;
                                y = (xy & 0x0F) + startY;
                                int wh = xywhBytes[subRectangleByteIndex+1];
                                //int wh = VNCUtils.VNCController.streamReadTillEnd(networkStream, 1)[0];
                                int w = (wh >> 4) + 1;
                                int h = (wh & 0x0F) + 1;
                                fillRectangle(
                                    imageColors, foregroundColor,
                                    x +rectangleX,
                                    y + rectangleY,
                                    w, h, imageWidth
                                );
                                subRectangleByteIndex += 2;
                            }
                        }
                    }
                }
                //startX += 16;
            }
            //Debug.Log("time-decode finish:" + System.DateTime.Now.Millisecond);
        }
    }
}

namespace VNCUtils{
    public class VNCController {
        // public static Color byteArrayToColor(byte[] byteArray, int bytesPerPixel){
        //     //return new Color(1.0f, 0.0f, 0.0f, 1.0f);
        //     // if (bytesPerPixel == 4){
        //     //     float blue = (float)(byteArray[0]/255.0);
        //     //     float green = (float)(byteArray[1]/255.0);
        //     //     float red = (float)(byteArray[2]/255.0);
        //     //     return new Color(red, green, blue, 1.0f);
        //     // } else if (bytesPerPixel == 2){
        //     //     // 2 bytes = 16 bits
        //     //     float blue =(float)(bytesA
        //     // )
        //     // } else {
        //     //     return new Color(0.0f, 0.0f, 0.0f,1.0f);
        //     // }
        //     return byteArrayToColor(
        //         byteArray, bytesPerPixel, 0
        //     );
        // }
        public static Color32 byteArrayToColor32(byte[] byteArray, int bytesPerPixel){
            return byteArrayToColor32(
                                    byteArray, bytesPerPixel, 0
                                    );
        }

        // public static Color byteArrayToColor(byte[] byteArray, int bytesPerPixel, int headIndex){
        //     //return new Color(1.0f, 0.0f, 0.0f, 1.0f);
        //     if (bytesPerPixel == 4){
        //         float blue = (float)(byteArray[headIndex]/255.0);
        //         float green = (float)(byteArray[headIndex+1]/255.0);
        //         float red = (float)(byteArray[headIndex+2]/255.0);
        //         return new Color(red, green, blue, 1.0f);
        //     } else if (bytesPerPixel == 2){
        //         //2 bytes = 16 bits
        //         int blueGreen = byteArray[headIndex];
        //         int redAlpha = byteArray[headIndex+1];
        //         float green = (blueGreen >> 4)/16.0f;
        //         float blue = (blueGreen & 0x0F)/16.0f;
        //         float red = (float)(redAlpha & 0x0F)/16.0f;
        //         //Debug.Log("RGB=" + red.ToString() + "," + green.ToString() + "," + blue.ToString());
        //         return new Color(red, green, blue, 1.0f);
        //     } else {
        //         return new Color(0.0f, 0.0f, 0.0f,1.0f);
        //     }
        // }
        public static Color32 byteArrayToColor32(byte[] byteArray, int bytesPerPixel, int headIndex){
            //return new Color(1.0f, 0.0f, 0.0f, 1.0f);
            if (bytesPerPixel == 4){
                //float blue = (float)(byteArray[headIndex]/255.0);
                //float green = (float)(byteArray[headIndex+1]/255.0);
                //float red = (float)(byteArray[headIndex+2]/255.0);
                //return new Color(red, green, blue, 1.0f);
                byte blue = byteArray[headIndex];
                byte green = byteArray[headIndex+1];
                byte red = byteArray[headIndex+2];
                return new Color32(red, green, blue, 255);
            } else if (bytesPerPixel == 2){
                //2 bytes = 16 bits
                int blueGreen = byteArray[headIndex];
                int redAlpha = byteArray[headIndex+1];
                // float green = (blueGreen >> 4)/16.0f;
                // float blue = (blueGreen & 0x0F)/16.0f;
                // float red = (float)(redAlpha & 0x0F)/16.0f;
                //Debug.Log("RGB=" + red.ToString() + "," + green.ToString() + "," + blue.ToString());
                //return new Color(red, green, blue, 1.0f);
                byte green = (byte)((blueGreen >> 4) << 4);
                byte blue = (byte)((blueGreen & 0x0F) << 4);
                byte red = (byte)((redAlpha & 0x0F) << 4);
                return new Color32(red, green, blue, 255);
            } else {
                //return new Color(0.0f, 0.0f, 0.0f,1.0f);
                return new Color32(0, 0, 0, 255);
            }
        }

        public int requestScreenHeight;
        public int requestScreenWidth;
        public int receivedPixelHehight;
        public int receivedPixelWidth;
        public TcpClient socketTcpClient;
        public void initSocket(String serverHostName, int serverPort){
            socketTcpClient = new TcpClient(
                serverHostName, serverPort
            );
            //socketTcpClient = socket;
            rfbProtocolVersionCheck();
        }
        private void rfbProtocolVersionCheck(){
            Byte[] bytes = new Byte[1024];
            //while (true) {
            NetworkStream stream = socketTcpClient.GetStream();
            int length = stream.Read(bytes, 0, 1024);
            TNDebug.Log("tcp received:");
            Byte[] receivedContent = new Byte[length];
            Array.Copy(bytes, 0, receivedContent, 0, length);
            TNDebug.Log(Encoding.ASCII.GetString(receivedContent));
            sendVNCVersion();
        }
        private void sendVNCVersion(){
            //tcpClient.send("RFB 003.003\n");
            NetworkStream stream = socketTcpClient.GetStream();
            Byte[] data = Encoding.ASCII.GetBytes("RFB 003.003\n");
            stream.Write(data, 0, data.Length);
        }
        public Byte[] authenticate(){
            Byte[] bytes = new Byte[1024];
            int length = 0;
            NetworkStream stream = socketTcpClient.GetStream();
            Byte[] receivedContent = streamReadTillEnd(stream, 20);
            //TNDebug.Log("length=" + length);
            TNDebug.Log("Received");
            int authType = receivedContent[3];
            TNDebug.Log("auth type=" + authType.ToString());
            //return authType;
            return receivedContent;
        }
        public void setEncodings(int[] encodings){
            TNDebug.Log("setEncodings");
            byte[] setEncodingsByte = new Byte[2+2+4*encodings.Length];
            setEncodingsByte[0] = 2;
            setEncodingsByte[1] = 0;
            setEncodingsByte[2] = (byte)(encodings.Length >> 8);
            setEncodingsByte[3] = (byte)(encodings.Length & 0x0F);
            TNDebug.Log(setEncodingsByte[2]);
            TNDebug.Log(setEncodingsByte[3]);
            for(int i=0; i<encodings.Length; i++){
                setEncodingsByte[4 + 4*i    ] = (byte)(encodings[i] << 24);
                setEncodingsByte[4 + 4*i + 1] = (byte)((encodings[i] << 16) & (0x0F));
                setEncodingsByte[4 + 4*i + 2] = (byte)((encodings[i] << 8) & 0x0F);
                setEncodingsByte[4 + 4*i + 3] = (byte)((encodings[i]) & 0x0F);
            }
            // setEncodingsByte[4] = 0;
            // setEncodingsByte[5] = 0;
            // setEncodingsByte[6] = 0;
            // setEncodingsByte[7] = 5;
            //Debug.Log("")
            writeToSOcket(socketTcpClient, setEncodingsByte);
        }

        public int authenticationResult(){
            // Byte[] bytes = new Byte[1024];
            //int length = 0;
            NetworkStream stream = socketTcpClient.GetStream();
            Byte[] bytes = streamReadTillEnd(stream, 4);
            int authenticateResult = bytes[3];
            TNDebug.Log(authenticateResult);
            return authenticateResult;
        }

        public void requestPixelBufferUpdate(int incremental, int x, int y, int width, int height){
            Byte[] requestPixelBufferBytes = new Byte[10];
            requestPixelBufferBytes[0] = 3;
            requestPixelBufferBytes[1] = (byte)incremental;
            requestPixelBufferBytes[2] = (byte) (x >> 8);
            requestPixelBufferBytes[3] = (byte) x;
            requestPixelBufferBytes[4] = (byte) (y >> 8);
            requestPixelBufferBytes[5] = (byte) y;
            requestPixelBufferBytes[6] = (byte) (width >> 8);
            requestPixelBufferBytes[7] = (byte) width;
            requestPixelBufferBytes[8] = (byte) (height >> 8);
            requestPixelBufferBytes[9] = (byte) height;
            writeToSOcket(socketTcpClient, requestPixelBufferBytes);
            receivedPixelHehight = height;
            receivedPixelWidth = width;
            //Debug.Log("requestPixelBufferUpdate");
        }

        public void getScreenInformation(){
            TcpClient client = socketTcpClient;
            Byte[] bytesConnectionEstablished = new Byte[1];
            bytesConnectionEstablished[0] = 0;
            writeToSOcket(client, bytesConnectionEstablished);
            //Byte[] bytes = new Byte[1024];
            //int length = 0;
            NetworkStream stream = client.GetStream();
            Byte[] bytes = streamReadTillEnd(stream, 24);
            int frameWidth = bytes[0] << 8 | bytes[1];
            int frameHeight = bytes[2] << 8 | bytes[3];
            TNDebug.Log("width, height");
            TNDebug.Log(frameWidth);
            TNDebug.Log(frameHeight);
            requestScreenHeight = frameHeight;
            requestScreenWidth = frameWidth;
            //initializeScreenTexture();
            Byte[] pixelFormatBytes = new Byte[16];
            for (int i=0; i<16; i++){
                pixelFormatBytes[i] = bytes[i + 4];
            }
            parsePixelFormat(pixelFormatBytes);
            TNDebug.Log(Encoding.ASCII.GetString(pixelFormatBytes));
            int nameLength = bytes[20] << 24 | bytes[21] << 16 | bytes[22] << 8 | bytes[23];
            TNDebug.Log("name length=" + nameLength);
            //TNDebug.Log("length=" + length);
            
            Byte[] nameBytes = streamReadTillEnd(stream, nameLength);
            // 名前部分を読み込む
        }

        public static Byte[] streamReadTillEnd(NetworkStream stream, int length){
            // 指定Byteまで読み込む
            int remainingLength = length;
            Byte[] streamOutputBytes = new Byte[length];
            Byte[] streamOutputFragment = new Byte[length];
            int bufferIndex = 0;
            while (remainingLength > 0){
                int streamLength = stream.Read(streamOutputFragment, 0, remainingLength);
                for (int i=0; i<streamLength; i++){
                    streamOutputBytes[bufferIndex] = streamOutputFragment[i];
                    bufferIndex ++;
                }
                remainingLength = remainingLength - streamLength;
            }
            return streamOutputBytes;
        }
        private void writeToSOcket(TcpClient client, Byte[] data){
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }

        private void parsePixelFormat(Byte[] pixelFormatBytes){
            int bitsPerPixel = (int)pixelFormatBytes[0];
            int depth = (int)pixelFormatBytes[1];
            int bigEndianFlag = (int)pixelFormatBytes[2];
            int trueColorFlag = (int)pixelFormatBytes[3];
            int redMax = (int)(pixelFormatBytes[4] << 8 | pixelFormatBytes[5]);
            int greenMax = (int)(pixelFormatBytes[6] << 8 | pixelFormatBytes[7]);
            int blueMax =(int)(pixelFormatBytes[8] << 8 | pixelFormatBytes[9]);
            int redShift = (int)pixelFormatBytes[10];
            int greenShift = (int)pixelFormatBytes[11] ;
            int blueShift = (int)pixelFormatBytes[12];
            TNDebug.Log("----------------------------");
            TNDebug.Log(bitsPerPixel);//32
            TNDebug.Log(depth);//24
            TNDebug.Log(bigEndianFlag);//0
            TNDebug.Log(trueColorFlag);//1
            TNDebug.Log(redMax);//255
            TNDebug.Log(greenMax);//255
            TNDebug.Log(blueMax);//255
            TNDebug.Log(redShift);//16
            TNDebug.Log(greenShift);//8
            TNDebug.Log(blueShift);//0
            TNDebug.Log("----------------------------");
        }
        public void setPixelFormat(int bitsPerPixel, int depth, 
        int redMax, int greenMax, int blueMax, int redShift, int greenShift, int blueShift){
            byte[] setPixelFormatBytes = new byte[16 + 4];
            setPixelFormatBytes[0] = 0;
            setPixelFormatBytes[4] = (byte)bitsPerPixel;
            setPixelFormatBytes[5] = (byte)depth;
            setPixelFormatBytes[6] = 0;
            setPixelFormatBytes[7] = 1;
            setPixelFormatBytes[8] = (byte)(redMax >> 8);
            setPixelFormatBytes[9] = (byte)(redMax & 0xFF);
            setPixelFormatBytes[10] = (byte)(greenMax >> 8);
            setPixelFormatBytes[11] = (byte)(greenMax & 0xFF);
            setPixelFormatBytes[12] = (byte)(blueMax >> 8);
            setPixelFormatBytes[13] = (byte)(blueMax & 0xFF);
            //Debug.Log("redMax " + (redMax >> 8) + "|" + (redMax & 0xFF));
            
            // setPixelFormatBytes[8] = 0;
            // setPixelFormatBytes[9] = 255;
            // setPixelFormatBytes[10] = 0;
            // setPixelFormatBytes[11] = 255;
            // setPixelFormatBytes[12] = 0;
            // setPixelFormatBytes[13] = 255;
            setPixelFormatBytes[14] = (byte)redShift;//16;
            setPixelFormatBytes[15] = (byte)greenShift;//8;
            setPixelFormatBytes[16] = (byte)blueShift;//0;
            setPixelFormatBytes[17] = 0;
            setPixelFormatBytes[18] = 0;
            setPixelFormatBytes[19] = 0;
            writeToSOcket(socketTcpClient, setPixelFormatBytes);
            //");
        }
        public static Byte[] getPointerEventBytes(Byte buttonMask, Vector2 pointerPosition){
            Byte[] pointerEventData = new byte[6];
            pointerEventData[0] = 5;
            pointerEventData[1] = buttonMask;
            int clickPositionX = (int)pointerPosition.x;
            int clickPositionY = (int)pointerPosition.y;
            pointerEventData[2] = (byte)(clickPositionX >> 8);
            pointerEventData[3] = (byte)(clickPositionX);
            pointerEventData[4] = (byte)(clickPositionY >> 8);
            pointerEventData[5] = (byte)(clickPositionY);
            return pointerEventData;
        }
        // public void asyncSendScroll(int scrollSize, Vector2 pointerPosition){
        //     Thread sendScrollThread = new Thread(new ThreadStart(sendScroll));
        //     sendScrollThread.IsBackground = true;
        //     sendScrollThread.Start();
        // }

        public void sendScroll(
                int scrollSize, Vector2 pointerPosition){
            // int scrollSizeAbs = scrollSize;
            // if (scrollSize == 0){
            //     return;
            // }
            // if (scrollSizeAbs < 0){
            //     scrollSizeAbs = - scrollSizeAbs;
            // }
            // byte buttonMask = 0x00;
            // if (scrollSize < 0){
            //     // scroll down
            //     buttonMask = 0b00010000;
            // } else {
            //     buttonMask = 0b00001000;
            // }
            // //writeToSOcket(tcpClientSocket);
            // for(int i=0; i<scrollSizeAbs; i++){
            //     writeToSOcket(socketTcpClient, getPointerEventBytes(buttonMask, pointerPosition));
            //     writeToSOcket(socketTcpClient, getPointerEventBytes(0x00, pointerPosition));
            // }
            //buttonMask = 0x00;
            AsyncScrollEventSender scrollEventSender = new AsyncScrollEventSender();
            scrollEventSender.prepare(socketTcpClient,
                                      scrollSize, pointerPosition);
            scrollEventSender.sendAsync();
        }
    }
    public class AsyncScrollEventSender{
        private Vector2 pointerPosition;
        private int scrollDistance;
        private TcpClient socketTcpClient;
        public void prepare(TcpClient client, int scrollDistance, Vector2 pointerPosition){
            this.socketTcpClient = client;
            this.scrollDistance = scrollDistance;
            this.pointerPosition = pointerPosition;
        }
        public static Byte[] getPointerEventBytes(Byte buttonMask, Vector2 pointerPosition){
            Byte[] pointerEventData = new byte[6];
            pointerEventData[0] = 5;
            pointerEventData[1] = buttonMask;
            int clickPositionX = (int)pointerPosition.x;
            int clickPositionY = (int)pointerPosition.y;
            pointerEventData[2] = (byte)(clickPositionX >> 8);
            pointerEventData[3] = (byte)(clickPositionX);
            pointerEventData[4] = (byte)(clickPositionY >> 8);
            pointerEventData[5] = (byte)(clickPositionY);
            return pointerEventData;
        }
        public void sendAsync(){
            Thread sendAsyncThread = new Thread(new ThreadStart(send));
            sendAsyncThread.IsBackground = true;
            sendAsyncThread.Start();
        }
        private void send(){
            int scrollSizeAbs = this.scrollDistance;
            if (this.scrollDistance == 0){
                return;
            }
            if (scrollSizeAbs < 0){
                scrollSizeAbs = - scrollSizeAbs;
            }
            byte buttonMask = 0x00;
            if (this.scrollDistance > 0){
                // scroll down
                buttonMask = 0b00010000;
            } else {
                buttonMask = 0b00001000;
            }
            //writeToSOcket(tcpClientSocket);
            writeToSOcket(socketTcpClient, getPointerEventBytes(0x00, this.pointerPosition));
            for(int i=0; i<scrollSizeAbs; i++){
                writeToSOcket(socketTcpClient, getPointerEventBytes(buttonMask, this.pointerPosition));
                writeToSOcket(socketTcpClient, getPointerEventBytes(0x00, this.pointerPosition));
            }
        }
        private void writeToSOcket(TcpClient client, Byte[] data){
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }
    }
    public class AsyncSocketWriter{
        // private Vector2 pointerPosition;
        // private int scrollDistance;
        private TcpClient socketTcpClient;
        private byte[] sendData;
        public void prepare(TcpClient client, byte[] data){
            this.sendData = data;
            this.socketTcpClient = client;
        }
        public void sendAsync(TcpClient client, byte[] data){
            this.prepare(client, data);
            Thread sendAsyncThread = new Thread(new ThreadStart(send));
            sendAsyncThread.IsBackground = true;
            sendAsyncThread.Start();
        }

        public void send(){
            this.writeToSocket(this.sendData);
        }
        private void writeToSocket(Byte[] data){
            NetworkStream stream = socketTcpClient.GetStream();
            stream.Write(data, 0, data.Length);
        }

    }
}

