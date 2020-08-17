using Adai.Core;
using Adai.Core.Ext;
using Adai.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace IM.Client
{
	/// <summary>
	/// 客户端
	/// </summary>
	public class Client
	{
		/// <summary>
		/// 服务器地址
		/// </summary>
		const string ServerHost = "http://127.0.0.1:8080/";
		/// <summary>
		/// 本地地址
		/// </summary>
		readonly string Host;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="username"></param>
		public Client(string username)
		{
			var port = new Random().Next(8090, 8099);
			Host = string.Format("http://127.0.0.1:{0}/", port);
			//启动Http监听
			HttpListenerHelper.Start(Host, Request);
			Console.WriteLine("本地监听已启动，地址：{0}", Host);
			Username = username;

			//登录
			HttpHelper.SendPost(string.Format("{0}{1}", ServerHost, "login"), new Dictionary<string, string>() {
				{ "username", Username },
				{ "password", "123456" },
				{ "host", Host }
			});

			//初始化好友列表
			Friends = new HashSet<Friend>();
			GenerateRsaKey();
		}

		/// <summary>
		/// 用户名
		/// </summary>
		public string Username { get; }
		/// <summary>
		/// 私钥
		/// </summary>
		public string PrivateKey { get; set; }
		/// <summary>
		/// 公钥
		/// </summary>
		public string PublicKey { get; set; }
		/// <summary>
		/// 好友列表
		/// </summary>
		public ICollection<Friend> Friends { get; }

		/// <summary>
		/// 生成Rsa密钥
		/// </summary>
		public void GenerateRsaKey()
		{
			var rsa = new RSACryptoServiceProvider();
			PrivateKey = rsa.ToXmlString(true);
			PublicKey = rsa.ToXmlString(false);

			//上传公钥
			HttpHelper.SendPost(string.Format("{0}{1}", ServerHost, "upload"), new Dictionary<string, string>() {
				{ "username", Username },
				{ "publicKey", PublicKey }
			});
		}

		/// <summary>
		/// 接收请求
		/// </summary>
		/// <param name="request"></param>
		/// <param name="parameters"></param>
		public string Request(HttpListenerRequest request, IDictionary<string, string> parameters)
		{
			//给好友发送连接请求
			var message = new Message(parameters["From"], parameters["Type"])
			{
				Id = parameters["Id"],
				Content = parameters["Content"],
				Key = parameters["Key"],
				Signature = parameters["Signature"],
				Timestamp = parameters["Timestamp"].ToDouble()
			};
			if (message.Type == "Connection")
			{
				if (!OnConnect(message.From, message.Key, message.Signature))
				{
					return "连接失败";
				}
				else
				{
					return "ok";
				}
			}
			else
			{
				Receive(message);
				return "ok";
			}
		}

		/// <summary>
		/// 发起连接请求
		/// </summary>
		/// <param name="to"></param>
		/// <returns></returns>
		public bool Connect(string to)
		{
			//下载好友公钥
			var response = HttpHelper.SendPost(string.Format("{0}{1}", ServerHost, "connect"), new Dictionary<string, string>() {
				{ "username", to }
			});
			var json = JsonHelper.DeserializeObject<JObject>(response.Content);

			var host = json.Value<string>("host");
			var publicKey = json.Value<string>("publicKey");
			var exiry = json.Value<DateTime>("expiry");

			//判断时效
			//...
			//保存好友的公钥并生成对称密钥
			var tempKey = StringHelper.GenerateRandom(32);
			var tempIV = StringHelper.GenerateRandom(16);
			Friends.Add(new Friend(to, host)
			{
				PublicKey = publicKey,
				Expiry = exiry,
				TempKey = tempKey,
				TempIV = tempIV
			});
			//使用好友的公钥对对称密钥加密
			var original = string.Format("{0}|{1}", tempKey, tempIV);
			var ciphertext = RSAHelper.EncryptByXml(original, publicKey);
			//使用SHA256摘要算法计算对称密钥的hash值
			var hash = SHAHelper.Encrypt(original, HashHalg.SHA256);
			//然后使用自己本地的私钥对hash值进行签名
			var signature = RSAHelper.SignByXml(hash, PrivateKey);

			//给好友发送连接请求
			var message = new Message(Username, "Connection")
			{
				Content = "请求连接",
				Key = ciphertext,
				Signature = signature
			};
			var result = Send(to, message);
			return result == "ok";
		}

		/// <summary>
		/// 处理连接请求
		/// </summary>
		/// <param name="from"></param>
		/// <param name="ciphertext"></param>
		/// <param name="signature"></param>
		public bool OnConnect(string from, string ciphertext, string signature)
		{
			//下载好友公钥
			var response = HttpHelper.SendPost(string.Format("{0}{1}", ServerHost, "connect"), new Dictionary<string, string>() {
				{ "username", from }
			});
			var json = JsonHelper.DeserializeObject<JObject>(response.Content);

			var host = json.Value<string>("host");
			var publicKey = json.Value<string>("publicKey");
			var exiry = json.Value<DateTime>("expiry");

			//判断时效
			//...
			//使用自己本地的私钥对对称密钥的加密数据进行解密获取对称加密密钥
			var original = RSAHelper.DecryptByXml(ciphertext, PrivateKey);
			//使用SHA256摘要算法计算解密的对称密钥的hash值
			var hash = SHAHelper.Encrypt(original, HashHalg.SHA256);
			//使用hash值和好友的公钥对签名数据进行验签操作
			if (!RSAHelper.VerifyByXml(signature, hash, publicKey))
			{
				Console.WriteLine("对来自【{0}】的连接请求验签失败", Username, from);
				return false;
			}
			//确认对方确实为好友
			var array = original.Split("|");
			var tempKey = array[0];
			var tempIV = array[1];
			//保存好友的公钥和对称密钥
			Friends.Add(new Friend(from, host)
			{
				PublicKey = publicKey,
				Expiry = exiry,
				TempKey = tempKey,
				TempIV = tempIV
			});
			return true;
		}

		/// <summary>
		/// 发送消息
		/// </summary>
		/// <param name="to"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public string Send(string to, string message)
		{
			return Send(to, new Message(Username, "Normal")
			{
				Content = message
			});
		}

		/// <summary>
		/// 发送消息
		/// </summary>
		/// <param name="to"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public string Send(string to, Message message)
		{
			var friend = Friends.Where(o => o.Username == to).FirstOrDefault();
			if (friend == null)
			{
				Console.WriteLine("好友列表中未找到好友【{0}】", to);
				return "不可以给陌生人发送消息";
			}

			message.Id = Guid.NewGuid().ToString();
			//对称密钥加密信息
			message.Content = AESHelper.RijndaelEncrypt(message.Content, friend.TempKey, friend.TempIV);
			message.Timestamp = DateTimeHelper.TimestampOfMilliseconds;

			var url = string.Format("{0}{1}", friend.Host, "connect");
			var request = new HttpRequest(url, HttpMethod.Post, HttpContentType.Json)
			{
				Content = JsonHelper.SerializeObject(message)
			};
			var response = HttpHelper.SendRequest(request);
			Console.WriteLine("发送消息给【{0}】=>{1}", to, request.Content);
			return response.Content;
		}

		/// <summary>
		/// 接收信息
		/// </summary>
		/// <param name="from"></param>
		/// <param name="message"></param>
		public string Receive(Message message)
		{
			var friend = Friends.Where(o => o.Username == message.From).FirstOrDefault();
			if (friend == null)
			{
				Console.WriteLine("【{0}】的好友列表未找到【{1}】", Username, message.From);
				return "拒绝来自陌生人的消息";
			}

			//对称密钥解密信息
			message.Content = AESHelper.RijndaelDecrypt(message.Content, friend.TempKey, friend.TempIV);
			Console.WriteLine("接收来自【{0}】的消息=>{1}", message.From, message.Content);
			return "ok";
		}
	}
}
