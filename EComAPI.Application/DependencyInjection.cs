using EComAPI.Application.Auth.Commands.LoginUser;
using EComAPI.Application.Auth.Commands.RegisterUser;
using EComAPI.Application.Auth.Queries.GetCurrentUser;
using EComAPI.Application.Categories.Commands.CreateCategory;
using EComAPI.Application.Categories.Commands.DeleteCategory;
using EComAPI.Application.Categories.Commands.RestoreCategory;
using EComAPI.Application.Categories.Commands.UpdateCategory;
using EComAPI.Application.Categories.Queries.GetCategories;
using EComAPI.Application.Products.Commands.AddProductImage;
using EComAPI.Application.Products.Commands.AddProductVariant;
using EComAPI.Application.Products.Commands.CreateProduct;
using EComAPI.Application.Products.Commands.DeleteProduct;
using EComAPI.Application.Products.Commands.RestoreProduct;
using EComAPI.Application.Products.Queries.GetProductBySlug;
using EComAPI.Application.Products.Queries.GetProducts;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // ===== AUTH =====
            services.AddScoped<RegisterUserHandler>();
            services.AddScoped<LoginUserHandler>();
            services.AddScoped<GetCurrentUserHandler>();

            // ===== PRODUCTS =====
            services.AddScoped<CreateProductHandler>();
            services.AddScoped<AddProductVariantHandler>();
            services.AddScoped<AddProductImageHandler>();
            services.AddScoped<GetProductsHandler>();
            services.AddScoped<GetProductBySlugHandler>();
            services.AddScoped<DeleteProductHandler>();
            services.AddScoped<RestoreProductHandler>();

            // ===== CATEGORIES =====
            services.AddScoped<CreateCategoryHandler>();
            services.AddScoped<GetCategoriesHandler>();
            services.AddScoped<UpdateCategoryHandler>();
            services.AddScoped<DeleteCategoryHandler>();
            services.AddScoped<RestoreCategoryHandler>();

            return services;
        }
    }
}