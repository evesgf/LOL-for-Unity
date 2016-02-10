/*------------------------------------------
用户连接池，用于存放链接的玩家对象
杨定鹏
2015年11月22日14:44:26
------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFrame
{
    public class UserTokenPool
    {
        private Stack<UserToken> _pool;             //用户连接池

        /// <summary>
        /// 实例化连接池
        /// </summary>
        /// <param name="max">传入最大限制数量</param>
        public UserTokenPool(int max)
        {
            _pool=new Stack<UserToken>(max);
        }

        /// <summary>
        /// 当用户连接时从连接池取出一个链接对象
        /// </summary>
        public UserToken Pop()
        {
            return _pool.Pop();                 //从堆栈中取出并返回最顶部的一个对象
        }

        /// <summary>
        /// 当用户断开连接时释放链接并存回连接池
        /// </summary>
        public void Push(UserToken token)
        {
            if (token != null)
            {
                _pool.Push(token);              //存回堆栈
            }
        }

        /// <summary>
        /// 封装，返回连接池大小
        /// </summary>
        public int Size
        {
            get { return _pool.Count; }
        }
    }
}
