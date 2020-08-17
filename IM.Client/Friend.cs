using System;

namespace IM.Client
{
	/// <summary>
	/// 好友
	/// </summary>
	public class Friend
	{
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="username"></param>
		/// <param name="host"></param>
		public Friend(string username, string host)
		{
			Username = username;
			Host = host;
		}

		/// <summary>
		/// 用户名
		/// </summary>
		public string Username { get; }
		/// <summary>
		/// 主机
		/// </summary>
		public string Host { get; set; }
		/// <summary>
		/// 公钥
		/// </summary>
		public string PublicKey { get; set; }
		/// <summary>
		/// 过期时间
		/// </summary>
		public DateTime Expiry { get; set; }
		/// <summary>
		/// TempKey
		/// </summary>
		public string TempKey { get; set; }
		/// <summary>
		/// TempIV
		/// </summary>
		public string TempIV { get; set; }
	}
}
