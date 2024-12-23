using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Postgrest.Attributes;
using Postgrest.Models;

using Column = Postgrest.Attributes.ColumnAttribute;
using Table = Postgrest.Attributes.TableAttribute;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonConverterAttribute = Newtonsoft.Json.JsonConverterAttribute;

namespace TechnicalDocsAssistant.Core.Models
{
    [Table("assets")]
    public class Asset : BaseModel
    {
        [PrimaryKey("id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [Column("title")]
        [Required]
        [MaxLength(200)]
        [JsonProperty("title")]
        public string Title { get; set; }

        [Column("markdown_content")]
        [Required]
        [JsonProperty("markdown_content")]
        public string MarkdownContent { get; set; }

        [Column("content_vector")]
        [Required]
        [JsonProperty("content_vector")]
        [JsonConverter(typeof(VectorJsonConverter))]
        public float[] ContentVector { get; set; }

        [Column("created_at")]
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [Column("modified_at")]
        [JsonProperty("modified")]
        public DateTime Modified { get; set; }

        [Column("is_deleted")]
        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("similarity")]
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        [NotMapped]
        public float? Similarity { get; set; }
    }

    public class VectorJsonConverter : JsonConverter<float[]>
    {
        public override float[] ReadJson(JsonReader reader, Type objectType, float[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.String)
            {
                var vectorStr = reader.Value.ToString();
                if (string.IsNullOrEmpty(vectorStr))
                    return null;

                // Remove the brackets and split by comma
                vectorStr = vectorStr.Trim('[', ']');
                var values = vectorStr.Split(',');
                var result = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    if (float.TryParse(values[i], out float value))
                    {
                        result[i] = value;
                    }
                }
                return result;
            }

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

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing vector");
        }

        public override void WriteJson(JsonWriter writer, float[] value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();
            foreach (var item in value)
            {
                writer.WriteValue(item);
            }
            writer.WriteEndArray();
        }
    }
}
