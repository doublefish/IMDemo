using System;

namespace IM.Server
{
	/// <summary>
	/// 客户端
	/// </summary>
	public class Client
	{
		/// <summary>
		/// 用户名
		/// </summary>
		public string Username { get; set; }
		/// <summary>
		/// 主机
		/// </summary>
		public string Host { get; set; }
		/// <summary>
		/// 公钥
		/// </summary>
		public string PublicKey { get; set; }
		/// <summary>
		/// 公钥过期时间
		/// </summary>
		public DateTime Expiry { get; set; }
	}
}
