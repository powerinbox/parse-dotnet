using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Parse
{
    internal class ParseObjectContract : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            // Filter properties
            properties = properties.Where(p => p.PropertyName != "createdAt" && p.PropertyName != "updatedAt").ToList();

            return properties;
        }
    }
}
