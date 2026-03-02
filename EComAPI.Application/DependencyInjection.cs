using EComAPI.Application.Auth.Commands.CreateAddress;
using EComAPI.Application.Auth.Commands.DeleteAddress;
using EComAPI.Application.Auth.Commands.DeleteOwnAccount;
using EComAPI.Application.Auth.Commands.LoginUser;
using EComAPI.Application.Auth.Commands.LogoutUser;
using EComAPI.Application.Auth.Commands.RefreshTokens;
using EComAPI.Application.Auth.Commands.RegisterUser;
using EComAPI.Application.Auth.Commands.SendEmailVerification;
using EComAPI.Application.Auth.Commands.SetDefaultAddress;
using EComAPI.Application.Auth.Commands.UpdateAddress;
using EComAPI.Application.Auth.Commands.UpdateOwnProfile;
using EComAPI.Application.Auth.Commands.VerifyEmail;
using EComAPI.Application.Auth.Queries.GetDefaultAddress;
using EComAPI.Application.Auth.Queries.GetOwnProfile;
using EComAPI.Application.Auth.Queries.GetUserAddresses;
using EComAPI.Application.Categories.Commands.CreateCategories;
using EComAPI.Application.Categories.Commands.DeleteCategories;
using EComAPI.Application.Categories.Commands.RestoreCategories;
using EComAPI.Application.Categories.Commands.UpdateCategories;
using EComAPI.Application.Categories.Queries.GetCategories;
using EComAPI.Application.Categories.Queries.GetCategoriesBySlug;
using EComAPI.Application.Products.Commands.ProductCommands.ActivateProduct;
using EComAPI.Application.Products.Commands.ProductCommands.CreateProduct;
using EComAPI.Application.Products.Commands.ProductCommands.DeactivateProduct;
using EComAPI.Application.Products.Commands.ProductCommands.DeleteProduct;
using EComAPI.Application.Products.Commands.ProductCommands.RecordProductView;
using EComAPI.Application.Products.Commands.ProductCommands.RestoreProduct;
using EComAPI.Application.Products.Commands.ProductCommands.UpdateProduct;
using EComAPI.Application.Products.Commands.ProductCommands.UpdateProductStock;
using EComAPI.Application.Products.Commands.ProductImageCommands.AddProductImage;
using EComAPI.Application.Products.Commands.ProductImageCommands.RemoveProductImage;
using EComAPI.Application.Products.Commands.ProductImageCommands.RestoreProductImage;
using EComAPI.Application.Products.Commands.ProductImageCommands.UpdateProductImage;
using EComAPI.Application.Products.Commands.ProductVariantCommands.AddProductVariant;
using EComAPI.Application.Products.Commands.ProductVariantCommands.RemoveProductVariant;
using EComAPI.Application.Products.Commands.ProductVariantCommands.RestoreProductVariant;
using EComAPI.Application.Products.Commands.ProductVariantCommands.UpdateProductVariant;
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
            services.AddScoped<GetOwnProfileHandler>();
            services.AddScoped<UpdateOwnProfileHandler>();
            services.AddScoped<DeleteOwnAccountHandler>();
            services.AddScoped<CreateAddressHandler>();
            services.AddScoped<UpdateAddressHandler>();
            services.AddScoped<DeleteAddressHandler>();
            services.AddScoped<SetDefaultAddressHandler>();
            services.AddScoped<GetUserAddressesHandler>();
            services.AddScoped<GetDefaultAddressHandler>();
            services.AddScoped<RefreshTokenHandler>();
            services.AddScoped<LogoutUserHandler>();
            services.AddScoped<SendEmailVerificationHandler>();
            services.AddScoped<VerifyEmailHandler>();

            // ===== CATEGORIES =====
            services.AddScoped<CreateCategoryHandler>();
            services.AddScoped<GetCategoryHandler>();
            services.AddScoped<GetCategoryBySlugHandler>();
            services.AddScoped<UpdateCategoryHandler>();
            services.AddScoped<DeleteCategoryHandler>();
            services.AddScoped<RestoreCategoryHandler>();

            // ===== PRODUCTS =====
            services.AddScoped<CreateProductHandler>();
            services.AddScoped<UpdateProductHandler>();
            services.AddScoped<ActivateProductHandler>();
            services.AddScoped<DeactivateProductHandler>();
            services.AddScoped<DeleteProductHandler>();
            services.AddScoped<RestoreProductHandler>();
            services.AddScoped<RecordProductViewHandler>();
            services.AddScoped<GetProductsHandler>();
            services.AddScoped<GetProductBySlugHandler>();

            // Product Variants
            services.AddScoped<AddProductVariantHandler>();
            services.AddScoped<UpdateProductVariantHandler>();
            services.AddScoped<UpdateProductStockHandler>();
            services.AddScoped<RemoveProductVariantHandler>();
            services.AddScoped<RestoreProductVariantHandler>();

            // Product Images
            services.AddScoped<AddProductImageHandler>();
            services.AddScoped<UpdateProductImageHandler>();
            services.AddScoped<RemoveProductImageHandler>();
            services.AddScoped<RestoreProductImageHandler>();

            return services;
        }
    }
}
