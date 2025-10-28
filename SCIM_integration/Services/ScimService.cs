using ScimProvisioningApp.Models;
using System.Collections.Concurrent;

namespace ScimProvisioningApp.Services
{
    public class ScimService
    {
        private readonly ConcurrentDictionary<string, UserModel> _employeeDatabase = new();

        public ScimService()
        {
            // Seed the database with some initial data for testing
            var initialUser = new UserModel(
                Guid.NewGuid().ToString(),
                "initial.user@example.com",
                "Initial",
                "User",
                "initial.user@example.com",
                true
            );
            _employeeDatabase.TryAdd(initialUser.Id, initialUser);
        }

        public object GetAllUsers()
        {
            return new
            {
                schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:ListResponse" },
                totalResults = _employeeDatabase.Count,
                Resources = _employeeDatabase.Values
            };
        }

        public UserModel GetUserById(string id)
        {
            _employeeDatabase.TryGetValue(id, out var user);
            return user;
        }

        public UserModel CreateUser(UserModel userModel)
        {
            var newUser = userModel with { Id = Guid.NewGuid().ToString() };
            _employeeDatabase.TryAdd(newUser.Id, newUser);
            return newUser;
        }

        public UserModel? UpdateUser(string id, UserModel userModel)
        {
            if (_employeeDatabase.TryGetValue(id, out var existingUser))
            {
                var updatedUser = existingUser with
                {
                    UserName = userModel.UserName,
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    Email = userModel.Email,
                    Active = userModel.Active
                };

                _employeeDatabase.TryUpdate(id, updatedUser, existingUser);
                return updatedUser;
            }
            return null;
        }

        public bool DeleteUser(string id)
        {
            return _employeeDatabase.TryRemove(id, out _);
        }
    }
}
