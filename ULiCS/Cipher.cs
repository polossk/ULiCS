using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace polossk.Universal.Global
{
    public class Cipher
    {
        /// <summary>
        /// 允许字符集
        /// </summary>
        public static char[] arrChar = new char[]{
                'a', 'b', 'd', 'c', 'e', 'f', 'g',
                'h', 'i', 'j', 'k', 'l', 'm', 'n',
                'o', 'p', 'r', 'q', 's', 't',
                'u', 'v', 'w', 'z', 'y', 'x',
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                'A', 'B', 'C', 'D', 'E', 'F', 'G',
                'H', 'I', 'J', 'K', 'L', 'M', 'N',
                'O', 'Q', 'P', 'R', 'T', 'S',
                'U', 'V', 'W', 'X', 'Y', 'Z'
        };
        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        /// <param name="raw">原始字符串</param>
        /// <returns>字符串的MD5值</returns>
        public static string md5Encrypt(string raw)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] rawData = System.Text.Encoding.Unicode.GetBytes(raw);
            byte[] tarData = md5.ComputeHash(rawData);
            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < rawData.Length; i++)
            {
                buf.Append(rawData[i].ToString("x"));
            }
            return buf.ToString();
        }
        /// <summary>
        /// 获得一次初始向量用于加密
        /// </summary>
        /// <param name="n">长度</param>
        /// <returns>初始向量字符串</returns>
        public static string getInitVector(int n)
        {
            StringBuilder buf = new StringBuilder();
            var guid = new Guid();
            Random rnd = new Random(guid.GetHashCode());
            for (int i = 0; i < n; i++)
            {
                buf.Append(arrChar[rnd.Next(0, arrChar.Length)].ToString());
            }
            return buf.ToString();
        }
        /// <summary>
        /// AES加密方法的接口函数
        /// </summary>
        /// <param name="raw">二进制原始数据</param>
        /// <param name="key">秘钥</param>
        /// <param name="iv">初始向量</param>
        /// <returns>二进制加密数据</returns>
        public static byte[] AESEncrypt(byte[] raw, string key, string iv)
        {
            byte[] keyBytes = System.Text.Encoding.Unicode.GetBytes(key);
            byte[] ivBytes = System.Text.Encoding.Unicode.GetBytes(iv);
            using (AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider())
            {
                aesCSP.Key = keyBytes;
                aesCSP.IV = ivBytes;
                aesCSP.Mode = CipherMode.CBC;
                aesCSP.Padding = PaddingMode.PKCS7;
                using (MemoryStream buf = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(buf, aesCSP.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(raw, 0, raw.Length);
                    cryptoStream.FlushFinalBlock();
                    return buf.ToArray();
                }
            }
        }
        /// <summary>
        /// AES解密方法的接口函数
        /// </summary>
        /// <param name="raw">二进制加密数据</param>
        /// <param name="key">秘钥</param>
        /// <param name="iv">初始向量</param>
        /// <returns>二进制原始数据二进制原始数据</returns>
        public static byte[] AESDecrypt(byte[] cipher, string key, string iv)
        {
            byte[] keyBytes = System.Text.Encoding.Unicode.GetBytes(key);
            byte[] ivBytes = System.Text.Encoding.Unicode.GetBytes(iv);
            using (AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider())
            {
                aesCSP.Key = keyBytes;
                aesCSP.IV = ivBytes;
                aesCSP.Mode = CipherMode.CBC;
                aesCSP.Padding = PaddingMode.PKCS7;
                using (MemoryStream buf = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(buf, aesCSP.CreateDecryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(cipher, 0, cipher.Length);
                    cryptoStream.FlushFinalBlock();
                    return buf.ToArray();
                }
            }
        }

        public static int ModPositive(int dividend, int divisor)
        {
            return ((dividend % divisor) + divisor) % divisor;
        }
    }
}
