/*--------------------------------------------------------
服务器入口
杨定鹏
2015年11月22日14:29:04

--------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetFrame
{
    public class ServerStart
    {
        private Socket _server;                         //服务器Socket监听对象
        private readonly int _maxClient;                //最大客户端连接数
        private UserTokenPool _pool;                    //连接池对象
        private Semaphore _acceptClients;               //信号量

        public LengthEncode LE;
        public LengthDecode LD;
        public Encode encode;
        public Decode decode;

        /// <summary>
        /// 初始化通信监听
        /// </summary>
        /// <param name="max">最大连接数</param>
        /// <param name="port">监听的端口</param>
        public ServerStart(int max,int port)
        {
            _server = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            _maxClient = max;                               //实例化socket监听对象

            _pool=new UserTokenPool(max);                   //实例化连接池对象

            //实例化一个最大允许max个线程允许的信号量
            //并将它的计数器的初始值设为max
            //这就是说除了调用该信号量的线程都将被阻塞
            _acceptClients = new Semaphore(max,max);

            //初始化创建max个数的链接对象并存入连接池
            for (var i = 0; i < max; i++)
            {
                //初始化token信息
                var token=new UserToken();

                //绑定接收事件
                token.ReceiveSAEA.Completed+=
                    new EventHandler<SocketAsyncEventArgs>(IO_Comleted);

                //绑定发送事件
                token.SendSAEA.Completed+=
                    new EventHandler<SocketAsyncEventArgs>(IO_Comleted);

                token.LD = LD;
                token.LE = LE;
                token.encode = encode;
                token.decode = decode;
                token.sendProcess = ProccessSend;

                _pool.Push(token);
            }
        }

        public void Start(int port)
        {
            //监听当前服务器网卡所有可用IP地址的port端口
            //监听外网IP，内网IP，和本机IP
            _server.Bind(new IPEndPoint(IPAddress.Any, port));

            //置于监听状态，并设置10个超出监听超度后的排队等待位置
            _server.Listen(10);
            //开启监听
            StartAccept(null);
        }

        /// <summary>
        /// 开始客户端连接监听
        /// </summary>
        /// <param name="e"></param>
        public void StartAccept(SocketAsyncEventArgs e)
        {
            //如果当前传入为空，说明调用新的客户端连接监听事件
            //否则移除当前客户端连接
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Comleted);
            }
            else
            {
                e.AcceptSocket = null;
            }
            //信号量减一
            _acceptClients.WaitOne();
            bool result = _server.AcceptAsync(e);

            //判断异步事件是否挂起，没挂起说明立刻执行完成 直接处理事件
            //否则会在处理完成后触发Accept_Comleted事件
            if (!result)
            {
                ProccessAccept(e);
            }
        }

        public void ProccessAccept(SocketAsyncEventArgs e)
        {
            //从连接池取出连接对象供新用户使用
            UserToken token= _pool.Pop();
            token.Conn = e.AcceptSocket;
            //TODO 通知应用层有客户端连接

            //开启消息到达监听
            StartReceive(token);

            //释放当前异步对象
            StartAccept(e);
        }

        public void Accept_Comleted(object sender, SocketAsyncEventArgs e)
        {
            ProccessAccept(e);
        }

        public void StartReceive(UserToken token)
        {
            //用户连接对象，开启异步数据接收
            bool result=token.Conn.ReceiveAsync(token.ReceiveSAEA);
            //检测异步事件是否挂起
            if (!result)
            {
                ProcessReceive(token.ReceiveSAEA);
            }
        }

        public void IO_Comleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                ProcessReceive(e);
            }
            else
            {
                ProccessSend(e);
            }
        }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            UserToken token=e.UserToken as UserToken;
            //判断网络消息接收是否成功
            if (token.ReceiveSAEA.BytesTransferred > 0 && token.ReceiveSAEA.SocketError == SocketError.Success)
            {
                byte[] message = new byte[token.ReceiveSAEA.BytesTransferred];
                Buffer.BlockCopy(token.ReceiveSAEA.Buffer, 0, message, 0, token.
                    ReceiveSAEA.BytesTransferred);

                //处理接收到的消息
                token.Receive(message);

                StartReceive(token);
            }
            else
            {
                if (token.ReceiveSAEA.SocketError != SocketError.Success)
                {
                    ClientClose(token,token.ReceiveSAEA.SocketError.ToString());
                }
                else
                {
                    ClientClose(token,"客户端主动断开连接");
                }
            }
        }

        public void ProccessSend(SocketAsyncEventArgs e)
        {
            UserToken token = e.UserToken as UserToken;
            if (e.SocketError != SocketError.Success)
            {
                ClientClose(token, e.SocketError.ToString());
            }
            else
            {
                //消息发送成功，回调成功
                token.Writed();
            }
        }

        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="token">断开的用户对象</param>
        /// <param name="error">断开连接的错误编码</param>
        public void ClientClose(UserToken token, string error)
        {
            if (token.Conn != null)
            {
                lock (token)
                {
                    //通知应用层客户端断开连接
                    token.Close();
                    //将连接还给连接池
                    _pool.Push(token);
                    //释放信号量,加回一个供其他用户使用
                    _acceptClients.Release();
                }
            }
        }
    }
}
