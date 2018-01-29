using System;
using System.Security.Cryptography;
using System.Text;

namespace PeerReviewWeb
{
	public static class Extensions
	{

		private static readonly SHA256 _hashTool = SHA256.Create();

		public static string HashUTF8ToBase64(this string input)
		{
			var _bytes = Encoding.UTF8.GetBytes(input);
			return Convert.ToBase64String(_hashTool.ComputeHash(_bytes));
		}
	}
}