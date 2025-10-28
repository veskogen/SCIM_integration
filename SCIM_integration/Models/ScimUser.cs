namespace ScimProvisioningApp.Models
{


    public record UserModel(
        string Id,
        string UserName,
        string FirstName,
        string LastName,
        string Email,
        bool Active
    );




    // The main DTO for the SCIM User create payload
    public class ScimUserCreateModel
    {
        public List<string> Schemas { get; set; } = new();
        public string UserName { get; set; }
        public ScimNameModel Name { get; set; }
        public List<ScimEmailModel> Emails { get; set; } = new();
        public bool Active { get; set; }
    }

    // A nested DTO for the 'name' attribute
    public class ScimNameModel
    {
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
    }

    // A nested DTO for the 'emails' attribute
    public class ScimEmailModel
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public bool Primary { get; set; }
    }
}
