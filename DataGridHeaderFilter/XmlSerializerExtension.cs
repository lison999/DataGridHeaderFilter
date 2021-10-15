using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Tongyu.Smart.Models
{
	public class XmlSerializerExtension
	{
        /// <summary>
        /// 将对象序列化为指定的文件名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool Serialize<T>(T obj, string fileName)
        {
            try
            {
                FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                XmlSerializer xs = new XmlSerializer(typeof(T));
                xs.Serialize(fs, obj);
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally 
            {
            }
        }

        /// <summary>
        /// 将xml文件进行反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string fileName)
        {
            try
            {
                if (File.Exists(fileName) == false)
                    return default(T);
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                XmlSerializer xs = new XmlSerializer(typeof(T));
                T obj = (T)xs.Deserialize(fs);
                return obj;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}