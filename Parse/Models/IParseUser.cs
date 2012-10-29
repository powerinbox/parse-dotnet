using Newtonsoft.Json;

namespace Parse
{
    public interface IParseUser
    {
        string UserName { get; }
        string Password { get; }
    }

    public class AuthData
    {
        [JsonProperty("facebook")]
        public FacebookAuthData Facebook { get; set; }

        [JsonProperty("twitter")]
        public TwitterAuthData Twitter { get; set; }

        [JsonProperty("anonymous")]
        public AnonymousAuthData Anonymous { get; set; }

        public bool IsValid()
        {
            // Check only one value is set
            if (Facebook != null && Twitter == null && Anonymous == null)
            {
                return Facebook.IsValid();
            }

            if (Twitter != null && Facebook == null && Anonymous == null)
            {
                return Twitter.IsValid();
            }

            if (Anonymous != null && Facebook == null && Twitter == null)
            {
                return Anonymous.IsValid();
            }

            return false;
        }
    }

    public class FacebookAuthData
    {
        /// <summary>User's Facebook id number as a string.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>An authorized Facebook access token for the user.</summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>Token expiration date of the format: yyyy-MM-dd'T'HH:mm:ss.SSS'Z'.</summary>
        [JsonProperty("expiration_date")]
        public string ExpirationDate { get; set; }

        public bool IsValid()
        {
            return (!string.IsNullOrEmpty(Id)
                 && !string.IsNullOrEmpty(AccessToken)
                 && !string.IsNullOrEmpty(ExpirationDate));
        }
    }

    public class TwitterAuthData
    {
        /// <summary>User's Twitter id number as a string.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>User's Twitter screen name.</summary>
        [JsonProperty("screen_name")]
        public string ScreenName { get; set; }

        /// <summary>Your application's consumer key.</summary>
        [JsonProperty("consumer_key")]
        public string ConsumerKey { get; set; }

        /// <summary>Your application's consumer secret.</summary>
        [JsonProperty("consumer_secret")]
        public string ConsumerSecret { get; set; }

        /// <summary>An authorized Twitter token for the user with your application.</summary>
        [JsonProperty("auth_token")]
        public string AuthToken { get; set; }

        /// <summary>The secret associated with the auth_token.</summary>
        [JsonProperty("auth_token_secret")]
        public string AuthTokenSecret { get; set; }

        public bool IsValid()
        {
            return (!string.IsNullOrEmpty(Id)
                 && !string.IsNullOrEmpty(ScreenName)
                 && !string.IsNullOrEmpty(ConsumerKey)
                 && !string.IsNullOrEmpty(ConsumerSecret)
                 && !string.IsNullOrEmpty(AuthToken)
                 && !string.IsNullOrEmpty(AuthTokenSecret));
        }
    }

    public class AnonymousAuthData
    {
        /// <summary>Random UUID with lowercase hexadecimal digits.</summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        public bool IsValid()
        {
            return (!string.IsNullOrEmpty(Id));
        }
    }
}