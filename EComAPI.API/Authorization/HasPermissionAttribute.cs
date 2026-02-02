using Microsoft.AspNetCore.Authorization;

namespace EComAPI.API.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
        {
            Policy = $"PERMISSION:{permission}";
        }
    }
}