namespace EComAPI.Application.Common.Authorization
{
    public static class Permissions
    {
        public static class Categories
        {
            public const string Read = "categories.read";
            public const string Create = "categories.create";
            public const string Update = "categories.update";
            public const string Delete = "categories.delete";
            public const string Restore = "categories.restore";
        }

        public static class Products
        {
            public const string Read = "products.read";
            public const string Create = "products.create";
            public const string Update = "products.update";
            public const string Delete = "products.delete";
            public const string Restore = "products.restore";
        }
    }
}