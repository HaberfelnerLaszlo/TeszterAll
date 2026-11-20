using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeszterLibrary.Services
{
    /// <summary>
    /// A System.Text.Json beállításai az API-válaszok olvasásához. A korábbi Newtonsoft.Json-os olvasás engedékenyebb
    /// volt; ezek a beállítások azt a viselkedést tartják meg (lásd ApiJsonCompatibilityTests):
    /// enum névvel vagy számmal, szám a szöveg mezőben, bool 0/1-ként vagy szövegként, komment és záró vessző.
    /// A kérések küldése nem változott (JsonSerializerDefaults.Web, az enum számként megy).
    /// </summary>
    public static class ApiJson
    {
        /// <summary>A kérés törzse: a PostAsJsonAsync alapbeállításaival egyező (camelCase).</summary>
        public static readonly JsonSerializerOptions Request = new(JsonSerializerDefaults.Web);

        /// <summary>A válasz olvasása (Web alapbeállítás: kis-nagybetű független nevek, szám szövegből is).</summary>
        public static readonly JsonSerializerOptions Response = CreateResponseOptions();

        /// <summary>A JSON-dokumentum beolvasása: komment és záró vessző megengedett.</summary>
        public static readonly JsonDocumentOptions Document = new()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        };

        private static JsonSerializerOptions CreateResponseOptions()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                IncludeFields = true,   // a Newtonsoft a publikus mezőket is kitöltötte
            };
            options.Converters.Add(new JsonStringEnumConverter());   // névvel (kis-nagybetű függetlenül) és számmal is
            options.Converters.Add(new LenientStringConverter());
            options.Converters.Add(new LenientBooleanConverter());
            options.MakeReadOnly(populateMissingResolver: true);   // megosztott példány: utólag ne lehessen módosítani
            return options;
        }

        /// <summary>Szöveg mezőbe érkező szám vagy bool is elfogadott (pl. 5 → "5", true → "True"), mint a Newtonsoftnál.</summary>
        private sealed class LenientStringConverter : JsonConverter<string>
        {
            public override bool HandleNull => false;

            public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.TryGetInt64(out var whole)
                    ? whole.ToString(CultureInfo.InvariantCulture)
                    : reader.GetDecimal().ToString(CultureInfo.InvariantCulture),
                JsonTokenType.True => bool.TrueString,
                JsonTokenType.False => bool.FalseString,
                _ => throw new JsonException($"Szöveget vártunk, de {reader.TokenType} érkezett."),
            };

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) => writer.WriteStringValue(value);
        }

        /// <summary>Bool mezőbe érkező 0/1 vagy "true"/"false" is elfogadott, mint a Newtonsoftnál.</summary>
        private sealed class LenientBooleanConverter : JsonConverter<bool>
        {
            public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Number => reader.GetDecimal() != 0,
                JsonTokenType.String when bool.TryParse(reader.GetString(), out var parsed) => parsed,
                _ => throw new JsonException($"Logikai értéket vártunk, de {reader.TokenType} érkezett."),
            };

            public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteBooleanValue(value);
        }
    }
}
