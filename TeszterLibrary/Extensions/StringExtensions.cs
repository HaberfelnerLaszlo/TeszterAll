using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace TeszterLibrary.Extensions
{
	public static class StringExtensions
	{
		public static IEnumerable<Claim> ParseClaimsFromJwt(this string jwt)
		{
			var claims = new List<Claim>();
			var payload = jwt.Split('.')[1];
			var jsonBytes = ParseBase64WithoutPadding(payload);
			Dictionary<string, object> keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

			keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);
			if (roles != null)
			{
				if (roles.ToString().Trim().StartsWith("["))
				{
#pragma warning disable CS8604 // Possible null reference argument.
					var parsedRoles = JsonSerializer.Deserialize<string[]>(json: roles.ToString());
#pragma warning restore CS8604 // Possible null reference argument.

					foreach (var parsedRole in parsedRoles)
					{
						claims.Add(new Claim(ClaimTypes.Role, parsedRole));
					}
				}
				else
				{
#pragma warning disable CS8604 // Possible null reference argument.
					claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
#pragma warning restore CS8604 // Possible null reference argument.
				}

				keyValuePairs.Remove(ClaimTypes.Role);
			}


#pragma warning disable CS8604 // Possible null reference argument.
			claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
#pragma warning restore CS8604 // Possible null reference argument.

			return claims;
		}

		private static byte[] ParseBase64WithoutPadding(string base64)
		{
			switch (base64.Length % 4)
			{
				case 2: base64 += "=="; break;
				case 3: base64 += "="; break;
			}
			return Convert.FromBase64String(base64);
		}
	}
}
