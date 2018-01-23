using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace polossk.Universal.Global
{
    public class Registry
    {
        /// <summary>
        /// 向注册表添加值
        /// </summary>
        /// <param name="address">键值地址</param>
        /// <param name="name">键值名称</param>
        /// <param name="value">键值</param>
        public static void AddKey2Registry(string address, string name, string value)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser;
            Microsoft.Win32.RegistryKey NanoWare = key.CreateSubKey("SOFTWARE\\NanoWare");
            Microsoft.Win32.RegistryKey Addr = NanoWare.CreateSubKey(address);
            Addr.SetValue(name, value);
        }
        /// <summary>
        /// 从注册表读取值
        /// </summary>
        /// <param name="address">键值地址</param>
        /// <param name="name">键值名称</param>
        /// <returns>键值</returns>
        public static string ReadKey4Registry(string address, string name)
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser;
            Microsoft.Win32.RegistryKey NanoWare = key.OpenSubKey("SOFTWARE\\NanoWare");
            Microsoft.Win32.RegistryKey Addr = NanoWare.OpenSubKey(address);
            if (Addr.GetValue(name) != null)
                return Addr.GetValue(name).ToString();
            else return null;
        }
    }
}
