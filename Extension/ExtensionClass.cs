using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShenNiu.LogTool.Extension
{
    public static class ExtensionClass
    {
        //是否属于下载文件
        public readonly static string[] _FileExtension = new string[] { ".jpg", ".png", ".xls", ".xlsx", ".cer", ".dll", ".rar" };
        //图标
        public readonly static string[] _IconExtension = new string[] {
            ".txt",".jpg", ".png", ".xls", ".xlsx", ".cer", ".dll", ".rar",
            ".aspx", ".cs", ".config", ".xml", ".json" , ".cshtml",".log" };

        //允许访问的文件后缀
        public readonly static string[] _AllowExtension = new string[] { ".log", ".txt" };


        public static string GetExtensionIcon(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { return "Non1.jpg"; }

            var icon = _IconExtension.SingleOrDefault(b => { return b.ToUpper() == str.ToUpper(); });
            return $"{(icon ?? "Non").Replace(".", "")}1.jpg";
        }

        #region ISession扩展

        /// <summary>
        /// 设置session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool Set<T>(this ISession session, string key, T val)
        {
            if (string.IsNullOrWhiteSpace(key) || val == null) { return false; }

            var strVal = JsonConvert.SerializeObject(val);
            var bb = Encoding.UTF8.GetBytes(strVal);
            session.Set(key, bb);
            return true;
        }

        /// <summary>
        /// 获取session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(this ISession session, string key)
        {
            var t = default(T);
            if (string.IsNullOrWhiteSpace(key)) { return t; }

            if (session.TryGetValue(key, out byte[] val))
            {
                var strVal = Encoding.UTF8.GetString(val);
                t = JsonConvert.DeserializeObject<T>(strVal);
            }
            return t;
        }

        #endregion


        #region  _Md5 Md5加密

        public static string _Md5(string input, string key = "我爱祖国")
        {
            var hash = string.Empty;

            using (MD5 md5Hash = MD5.Create())
            {

                hash = GetMd5Hash(md5Hash, input + key);
            }
            return hash;
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString().ToUpper();
        }

        #endregion
    }
}
