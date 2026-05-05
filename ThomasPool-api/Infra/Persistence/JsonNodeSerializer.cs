using System.Text.Json.Nodes;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ThomasPool.Infra.Persistence;

public class JsonNodeSerializer : SerializerBase<JsonNode>
{
    public static readonly JsonNodeSerializer Instance = new();

    public override JsonNode Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        if (context.Reader.CurrentBsonType == BsonType.Null)
        {
            context.Reader.ReadNull();
            return null!;
        }

        var doc = BsonDocumentSerializer.Instance.Deserialize(context, args);
        var json = doc.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson });
        return JsonNode.Parse(json)!;
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JsonNode value)
    {
        if (value is null)
        {
            context.Writer.WriteNull();
            return;
        }

        var doc = BsonDocument.Parse(value.ToJsonString());
        BsonDocumentSerializer.Instance.Serialize(context, args, doc);
    }
}
