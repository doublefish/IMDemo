using Adai.Core;
using System;

namespace IM.Client
{
	/// <summary>
	/// 消息
	/// </summary>
	public class Message
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="from"></param>
		/// <param name="type">类型：Connection/Normal</param>
		public Message(string from, string type = "Normal")
		{
			From = from;
			Type = type;
		}

		/// <summary>
		/// ID
		/// </summary>
		public string Id { get; set; }
		/// <summary>
		/// From
		/// </summary>
		public string From { get; set; }
		/// <summary>
		/// 类型：connection/normal
		/// </summary>
		public string Type { get; set; }
		/// <summary>
		/// 内容
		/// </summary>
		public string Content { get; set; }
		/// <summary>
		/// 密钥
		/// </summary>
		public string Key { get; set; }
		/// <summary>
		/// 签名
		/// </summary>
		public string Signature { get; set; }
		/// <summary>
		/// 时间戳
		/// </summary>
		public double Timestamp { get; set; }
	}
}
