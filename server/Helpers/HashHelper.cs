using System;
using System.Security.Cryptography;
using System.Text;

namespace Server.Helpers
{
	public static class HashHelper
	{
		public static string Sha256(string s)
		{
			using (var sha = SHA256.Create())
			{
				var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
				return Convert.ToBase64String(bytes);
			}
		}
	}
}
