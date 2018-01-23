using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Universal.Global
{
    public class ToBytes<T>
    {
        /// <summary>
        /// 把可序列化类序列化
        /// </summary>
        /// <param name="data">可序列化的类</param>
        /// <param name="tar">输出目标</param>
        public static void GetBytes(ref T data, out byte[] tar)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream buf = new MemoryStream();
            bf.Serialize(buf, data);
            tar = buf.ToArray();
            buf.Close();
            buf.Dispose();
        }
    }
}
