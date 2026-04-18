namespace EComAPI.Application.Common.Constants
{
    public static class CacheKeys
    {
        public const string CategoriesNamespace = "categories";
        public const string ProductsNamespace = "products";
        public const string TokenBlacklistPrefix = "auth:blacklist:";

        public static string NamespaceVersion(string namespaceName)
            => $"cache:version:{namespaceName}";
    }
}
