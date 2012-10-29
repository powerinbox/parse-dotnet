using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Parse
{
    internal class ParseUserContract : DefaultContractResolver
    {
        private static readonly Dictionary<string, string> _knownProperties = new Dictionary<string, string>
        {
            {"username", "username"},
            {"password", "password"},
            {"authdata", "authData"},
            {"sessiontoken", "sessionToken"}
        };

        protected override string ResolvePropertyName(string propertyName)
        {
            return ResolveName(propertyName);
        }

        public static string ResolveName(string propertyName)
        {
            return _knownProperties.ContainsKey(propertyName.ToLower()) ? _knownProperties[propertyName.ToLower()] : propertyName;
        }
    }
}