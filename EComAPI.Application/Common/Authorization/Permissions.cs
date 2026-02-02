namespace EComAPI.Application.Common.Authorization
{
    public static class Permissions
    {
        public static class Categories
        {
            public const string View = "Categories.View";
            public const string Create = "Categories.Create";
            public const string Update = "Categories.Update";
            public const string Delete = "Categories.Delete";
            public const string Restore = "Categories.Restore";
        }

        public static class Products
        {
            public const string View = "Products.View";
            public const string Create = "Products.Create";
            public const string Update = "Products.Update";
            public const string Delete = "Products.Delete";
        }
    }
}