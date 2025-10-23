using Newtonsoft.Json;
using System.Collections.Generic;

namespace ScimProvisioningApp.Models
{
    public class ScimUser
    {
        [JsonProperty("schemas")]
        public string[] Schemas { get; set; } = { "urn:ietf:params:scim:schemas:core:2.0:User" };

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("name")]
        public ScimUserName Name { get; set; }

        [JsonProperty("emails")]
        public List<ScimEmail> Emails { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
    }

    public class ScimUserName
    {
        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("familyName")]
        public string FamilyName { get; set; }

        [JsonProperty("formatted")]
        public string Formatted { get; set; }
    }

    public class ScimEmail
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("primary")]
        public bool Primary { get; set; }
    }
}
