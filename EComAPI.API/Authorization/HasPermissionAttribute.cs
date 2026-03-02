using Microsoft.AspNetCore.Authorization;

namespace EComAPI.API.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public string Permission { get; }

        public HasPermissionAttribute(string permission)
        {
            Permission = permission;
            Policy = $"HasPermission:{permission}";
        }
    }
}