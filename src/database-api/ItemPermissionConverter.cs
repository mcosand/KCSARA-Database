using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sar.Database.Model;

namespace Kcsara.Database.Api
{
  class ItemPermissionConverter : JsonConverter
  {
    public override bool CanConvert(Type objectType)
    {
      return (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ItemPermissionWrapper<>));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      var item = value as IItemPermissionWrapper;
      JObject o = (JObject)JToken.FromObject(item.Item, serializer);
      o.Add("_u", JToken.FromObject(item.U ? 1 : 0));
      o.Add("_d", JToken.FromObject(item.D ? 1 : 0));
      if (item.More.Count > 0)
      {
        o.Add("__", JToken.FromObject(item.More.ToDictionary(f => f.Key, f => f.Value ? 1 : 0)));
      }
      o.WriteTo(writer);
    }
  }
}
