using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFrame.Auto
{
    class LengthEncoding
    {
        /// <summary>
        /// 粘包长度编码
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static byte[] Encode(byte[] buff)
        {
            MemoryStream ms=new MemoryStream();             //创建内存流对象
            BinaryWriter sw=new BinaryWriter(ms);           //写入二进制对象流
            sw.Write(buff.Length);                          //写入消息长度
            sw.Write(buff);                                 //写入消息体
            byte[] result=new byte[ms.Length];
            Buffer.BlockCopy(ms.GetBuffer(),0,result,0,(int)ms.Length);
            sw.Close();
            ms.Close();
            return result;
        }

        /// <summary>
        /// 粘包长度解码
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        public static byte[] Decode(ref List<byte> cache)
        {
            //int型4字节，小于4无法解析
            if (cache.Count < 4) return null;

            //创建内存流并将缓存数据写入
            MemoryStream ms =new MemoryStream(cache.ToArray());
            BinaryReader br=new BinaryReader(ms);           //二进制读取流
            //从缓存中读取int型消息体长度
            int length = br.ReadInt32();

            //如果消息体长度大于缓存中数据的长度，说明消息还未读取完
            //等待下次消息到达后再进行处理
            if (length > ms.Length - ms.Position)
            {
                return null;
            }
            
            //读取正确长度的数据
            byte[] result = br.ReadBytes(length);
            //清空缓存
            cache.Clear();
            //将读取后的剩余数据写入缓存
            cache.AddRange(br.ReadBytes((int)(ms.Length - ms.Position)));
            br.Close();
            ms.Close();
            return result;
        }
    }
}
