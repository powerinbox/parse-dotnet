using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Parse
{
    internal class DateConverter : DateTimeConverterBase
   {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
         serializer.Serialize(writer, new Dictionary<string, object>
                                      {
                                         {"__type", "Date"},
                                         {"iso", ((DateTime) value).ToUniversalTime().ToString("o")}
                                      });
      }

      public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
      {
         if (reader.TokenType != JsonToken.StartObject)
            throw new InvalidOperationException("Input needs to be wrapped in an object");

         var dict = serializer.Deserialize<Dictionary<string, string>>(reader);
         if (!dict.ContainsKey("__type") || !dict.ContainsKey("iso"))
            throw new Exception("Wrong Token Type");

         if (dict["__type"] != "Date")
           throw new Exception("Wrong serialized object");

         return DateTime.Parse(dict["iso"]);
      }
   }
}