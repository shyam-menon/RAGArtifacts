using System;
using System.ComponentModel.DataAnnotations;
using Postgrest.Attributes;
using Postgrest.Models;
using Newtonsoft.Json;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace TechnicalDocsAssistant.Core.Models
{
    [Table("assets")]
    public class Asset : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; }

        [Column("title")]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Column("markdown_content")]
        [Required]
        public string MarkdownContent { get; set; }

        [Column("content_vector")]
        [JsonConverter(typeof(VectorJsonConverter))]
        public float[] ContentVector { get; set; }

        [Column("created_at")]
        public DateTime Created { get; set; }

        [Column("modified_at")]
        public DateTime Modified { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }

    public class VectorJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(float[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                // Parse the string representation of the vector
                string vectorStr = (string)reader.Value;
                if (string.IsNullOrEmpty(vectorStr)) return null;

                // Remove brackets and split by comma
                vectorStr = vectorStr.Trim('[', ']');
                var values = vectorStr.Split(',');

                // Convert string values to float array
                var result = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    if (float.TryParse(values[i].Trim(), out float value))
                    {
                        result[i] = value;
                    }
                }
                return result;
            }

            // If it's already an array, read it directly
            if (reader.TokenType == JsonToken.StartArray)
            {
                var list = new List<float>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.EndArray)
                        break;

                    if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                    {
                        list.Add(Convert.ToSingle(reader.Value));
                    }
                }
                return list.ToArray();
            }

            throw new JsonException("Unexpected token type when parsing vector");
        }

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var array = (float[])value;
            writer.WriteStartArray();
            foreach (var item in array)
            {
                writer.WriteValue(item);
            }
            writer.WriteEndArray();
        }
    }
}
