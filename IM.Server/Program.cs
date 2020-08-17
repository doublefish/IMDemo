using Adai.Core;
using Adai.Core.Ext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IM.Server
{
	/// <summary>
	/// Program
	/// </summary>
	class Program
	{
		/// <summary>
		/// 客户端地址
		/// </summary>
		static readonly ICollection<Client> Clients = new HashSet<Client>();

		/// <summary>
		/// 服务器地址
		/// </summary>
		const string Host = "http://127.0.0.1:8080/";

		/// <summary>
		/// Main
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			//启动Http监听
			HttpListenerHelper.Start(Host, Request);
			Console.WriteLine("服务监听已启动，地址：{0}", Host);
		}

		/// <summary>
		/// 接收请求
		/// </summary>
		/// <param name="request"></param>
		/// <param name="parameters"></param>
		public static string Request(HttpListenerRequest request, IDictionary<string, string> parameters)
		{
			try
			{
				Console.WriteLine("接收来自【{0}】的请求=>{1}", request.RemoteEndPoint, parameters.ToQueryString());
				if (request.RawUrl.StartsWith("/login"))
				{
					//登录
					var username = parameters["username"];
					var password = parameters["password"];
					var host = parameters["host"];

					Clients.Add(new Client()
					{
						Username = username,
						Host = host,
						PublicKey = null,
						Expiry = DateTime.MinValue
					});
				}
				else if (request.RawUrl.StartsWith("/upload"))
				{
					//上传公钥
					var username = parameters["username"];
					var publicKey = parameters["publicKey"];

					var client = Clients.Where(o => o.Username == username).FirstOrDefault();
					client.PublicKey = publicKey;
					client.Expiry = DateTime.Now.AddDays(1);
				}
				else if (request.RawUrl.StartsWith("/connect"))
				{
					//连接好友
					var username = parameters["username"];

					var client = Clients.Where(o => o.Username == username).FirstOrDefault();
					return JsonHelper.SerializeObject(new
					{
						host = client.Host,
						publicKey = client.PublicKey,
						expiry = client.Expiry
					});
				}
				else
				{
				}
				return "ok";
			}
			catch (Exception ex)
			{
				Console.WriteLine("接收来自【{0}】的请求报错=>{1}", request.RemoteEndPoint, ex.Message);
				return ex.Message;
			}
		}
	}
}
