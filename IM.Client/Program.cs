using System;
using System.Collections.Generic;

namespace IM.Client
{
	/// <summary>
	/// Program
	/// </summary>
	class Program
	{
		/// <summary>
		/// Main
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			Console.WriteLine("请输入用户名：");
			var username = Console.ReadLine();
			var client = new Client(username);

			Console.WriteLine("请输入好友用户名：");
			var friend = Console.ReadLine();

			client.Connect(friend);

			while (true)
			{
				Console.WriteLine("请输入信息：");
				var content = Console.ReadLine();
				client.Send(friend, content);
			}
		}
	}
}
