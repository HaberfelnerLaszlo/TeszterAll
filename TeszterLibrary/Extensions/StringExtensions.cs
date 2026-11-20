using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace TeszterLibrary.Extensions
{
	public static class StringExtensions
	{
		/// <summary>Szövegként kapott másodpercek olvasható alakban (lásd IntExtension.ToTime). Érvénytelen bemenetre üres szöveg.</summary>
		public static string ToTime(this string? value)
			=> value.ToIntOrNull() is int seconds ? seconds.ToTime() : string.Empty;

		/// <summary>Guid, vagy null, ha a szöveg üres vagy nem érvényes azonosító (kivétel helyett).</summary>
		public static Guid? ToGuidOrNull(this string? value)
			=> Guid.TryParse(value, out var id) ? id : null;

		/// <summary>Egész szám (kultúrafüggetlenül), vagy null, ha a szöveg nem szám (kivétel helyett).</summary>
		public static int? ToIntOrNull(this string? value)
			=> int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var number) ? number : null;
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
