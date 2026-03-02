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

        public static class Users
        {
            public const string ReadAll = "users.read.all";
            public const string Create = "users.create";
            public const string UpdateAny = "users.update.any";
            public const string DeleteAny = "users.delete.any";
            public const string RestoreAny = "users.restore.any";
            public const string HardDeleteAny = "users.harddelete.any";
            public const string ReadOwn = "users.read.own";
            public const string UpdateOwn = "users.update.own";
            public const string DeleteOwn = "users.delete.own";
        }

        public static class Orders
        {
            public const string ReadAll = "orders.read.all";
            public const string UpdateAny = "orders.update.any";
            public const string DeleteAny = "orders.delete.any";
            public const string ReadOwn = "orders.read.own";
            public const string Create = "orders.create";
            public const string CancelOwn = "orders.cancel.own";
        }
    }
}