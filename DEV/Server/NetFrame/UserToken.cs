/*-------------------------------------------
用户连接信息对象，每一个代表一个用户连接
杨定鹏
2015年11月22日14:49:42
-------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace NetFrame
{
    /// <summary>
    /// 用户连接信息对象
    /// </summary>
    public class UserToken
    {
        public Socket Conn;                                   //客户端连接
        public SocketAsyncEventArgs ReceiveSAEA;              //用户异步接收数据对象
        public SocketAsyncEventArgs SendSAEA;                 //用户异步发送数据对象

        List<byte> cache =new List<byte>();                   //缓存接收的数据等待粘包

        public LengthEncode LE;
        public LengthDecode LD;
        public Encode encode;
        public Decode decode;

        private bool isReading = false;
        private bool isWriting = false;

        //写入的数据队列
        Queue<byte[]> writeQueue=new Queue<byte[]>();

        public delegate void SendProcess(SocketAsyncEventArgs e);

        public SendProcess sendProcess;

        public UserToken()
        {
            ReceiveSAEA = new SocketAsyncEventArgs();
            SendSAEA = new SocketAsyncEventArgs();

            ReceiveSAEA.UserToken = this;
            SendSAEA.UserToken = this;
        }

        //网络消息到达
        public void Receive(byte[] buff)
        {
            //将消息加入缓存
            cache.AddRange(buff);
            if (!isReading)
            {
                isReading = true;
                onData();
            }
        }

        //缓存中有数据处理
        private void onData()
        {
            //解码消息存储对象
            byte[] buff = null;
            //当粘包解码器存在的时候进行粘包处理
            if (LD != null)
            {
                buff = LD(ref cache);
                //消息未接收全，退出数据处理，等待下次消息到达
                if (buff == null)
                {
                    isReading = false;
                    return;
                }
            }
            else
            {
                //缓存区中没有数据，直接跳出消息处理，等待下次消息到达
                if (cache.Count == 0)
                {
                    isReading = false;
                    return;
                }
            }
            //反序列化方法是否存在
            if (decode == null)
            {
                throw new Exception("message decode process is null");
            }
            //进行消息反序列化
            object message = decode(buff);

            //TODO 通知应用层有消息到达
            //尾递归 防止在消息处理过程中有其他消息到达而没有经过处理
            onData();
        }

        public void Write(byte[] value)
        {
            if (Conn == null)
            {
                //此连接已经断开
                return;
            }

            writeQueue.Enqueue(value);
            if (!isWriting)
            {
                isWriting = true;
                onWrite();
            }
        }

        public void onWrite()
        {
            //判断发送队列是否有消息
            if (writeQueue.Count == 0)
            {

                isWriting = false;
                return;
            }

            //取出第一条待发消息
            byte[] buff = writeQueue.Dequeue();
            //设置消息发送异步对象的发送数据缓冲区数据
            SendSAEA.SetBuffer(buff, 0, buff.Length);
            //开启异步发送
            bool result = Conn.SendAsync(SendSAEA);
            //是否挂起
            if (!result)
            {
                sendProcess(SendSAEA);
            }
        }

        public void Writed()
        {
            //与onData尾递归同理
            onWrite();
        }

        public void Close()
        {
            try
            {
                writeQueue.Clear();
                cache.Clear();
                isReading = false;
                isWriting = false;
                Conn.Shutdown(SocketShutdown.Both);
                Conn.Close();
                Conn = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
