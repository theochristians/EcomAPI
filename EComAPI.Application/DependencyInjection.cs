using EComAPI.Application.Auth.Commands.CreateAddress;
using EComAPI.Application.Auth.Commands.DeleteAddress;
using EComAPI.Application.Auth.Commands.DeleteOwnAccount;
using EComAPI.Application.Auth.Commands.LoginUser;
using EComAPI.Application.Auth.Commands.LogoutUser;
using EComAPI.Application.Auth.Commands.ForgotPassword;
using EComAPI.Application.Auth.Commands.RefreshTokens;
using EComAPI.Application.Auth.Commands.RegisterUser;
using EComAPI.Application.Auth.Commands.ResetPassword;
using EComAPI.Application.Auth.Commands.SendEmailVerification;
using EComAPI.Application.Auth.Commands.SendEmailVerificationByEmail;
using EComAPI.Application.Auth.Commands.SetDefaultAddress;
using EComAPI.Application.Auth.Commands.UpdateAddress;
using EComAPI.Application.Auth.Commands.UpdateOwnProfile;
using EComAPI.Application.Auth.Commands.VerifyEmail;
using EComAPI.Application.Auth.Commands.VerifyEmailByEmail;
using EComAPI.Application.Common.Commands.Uploads.ConfirmUpload;
using EComAPI.Application.Common.Commands.Uploads.GenerateUploadSas;
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
using EComAPI.Application.Shopping.Commands.CartCommands.AddToCart;
using EComAPI.Application.Shopping.Commands.CartCommands.ClearCart;
using EComAPI.Application.Shopping.Commands.CartCommands.CreateCart;
using EComAPI.Application.Shopping.Commands.CartCommands.RemoveFromCart;
using EComAPI.Application.Shopping.Commands.CartCommands.UpdateCartItem;
using EComAPI.Application.Shopping.Commands.WishlistCommands.AddToWishlist;
using EComAPI.Application.Shopping.Commands.WishlistCommands.RemoveFromWishlist;
using EComAPI.Application.Shopping.Queries.GetCart;
using EComAPI.Application.Shopping.Queries.GetWishlist;
using EComAPI.Application.Transaction.Commands.CouponCommands.CreateCoupon;
using EComAPI.Application.Transaction.Commands.OrderCommands.CreateOrder;
using EComAPI.Application.Transaction.Queries.CouponQueries.GetAllCoupons;
using EComAPI.Application.Transaction.Commands.OrderCommands.UpdateOrderStatus;
using EComAPI.Application.Transaction.Commands.PaymentCommands.ConfirmPayment;
using EComAPI.Application.Transaction.Commands.PaymentCommands.SubmitPaymentProof;
using EComAPI.Application.Transaction.Commands.ReviewCommands.CreateReview;
using EComAPI.Application.Transaction.Commands.ReviewCommands.UpdateReview;
using EComAPI.Application.Transaction.Commands.ReviewCommands.DeleteReview;
using EComAPI.Application.Transaction.Commands.ReturnCommands.CreateReturn;
using EComAPI.Application.Transaction.Commands.ReturnCommands.ApproveReturn;
using EComAPI.Application.Transaction.Commands.ReturnCommands.RejectReturn;
using EComAPI.Application.Transaction.Commands.ReturnCommands.MarkReturnRefunded;
using EComAPI.Application.Transaction.Queries.OrderQueries.GetOrderById;
using EComAPI.Application.Transaction.Queries.OrderQueries.GetOrdersByUser;
using EComAPI.Application.Transaction.Queries.ReviewQueries.GetReviewsByProduct;
using EComAPI.Application.Transaction.Queries.ReturnQueries.GetReturnById;
using EComAPI.Application.Transaction.Queries.ReturnQueries.GetReturnsByUser;
using EComAPI.Application.Transaction.Queries.OrderStatusLogQueries.GetOrderStatusLogs;
using EComAPI.Application.Transaction.Queries.StockLogQueries.GetStockLogs;
using Microsoft.Extensions.DependencyInjection;

namespace EComAPI.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //Auth System
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
            services.AddScoped<SendEmailVerificationByEmailHandler>();
            services.AddScoped<VerifyEmailByEmailHandler>();
            services.AddScoped<ForgotPasswordHandler>();
            services.AddScoped<ResetPasswordHandler>();

            // Uploads (SAS direct upload flow)
            services.AddScoped<GenerateUploadSasHandler>();
            services.AddScoped<ConfirmUploadHandler>();

            // Categories
            services.AddScoped<CreateCategoryHandler>();
            services.AddScoped<GetCategoriesHandler>();
            services.AddScoped<GetCategoryBySlugHandler>();
            services.AddScoped<UpdateCategoryHandler>();
            services.AddScoped<DeleteCategoryHandler>();
            services.AddScoped<RestoreCategoriesHandler>();

            // Products
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

            // Shopping
            services.AddScoped<CreateCartHandler>();
            services.AddScoped<GetCartHandler>();
            services.AddScoped<AddToCartHandler>();
            services.AddScoped<UpdateCartItemHandler>();
            services.AddScoped<RemoveFromCartHandler>();
            services.AddScoped<ClearCartHandler>();
            services.AddScoped<GetWishlistHandler>();
            services.AddScoped<AddToWishlistHandler>();
            services.AddScoped<RemoveFromWishlistHandler>();

            // Transactions
            services.AddScoped<CreateOrderHandler>();
            services.AddScoped<UpdateOrderStatusHandler>();
            services.AddScoped<SubmitPaymentProofHandler>();
            services.AddScoped<ConfirmPaymentHandler>();
            services.AddScoped<CreateCouponHandler>();
            services.AddScoped<GetAllCouponsHandler>();
            services.AddScoped<GetOrderByIdHandler>();
            services.AddScoped<GetOrdersByUserHandler>();

            // Reviews
            services.AddScoped<CreateReviewHandler>();
            services.AddScoped<UpdateReviewHandler>();
            services.AddScoped<DeleteReviewHandler>();
            services.AddScoped<GetReviewsByProductHandler>();

            // Returns
            services.AddScoped<CreateReturnHandler>();
            services.AddScoped<ApproveReturnHandler>();
            services.AddScoped<RejectReturnHandler>();
            services.AddScoped<MarkReturnRefundedHandler>();
            services.AddScoped<GetReturnByIdHandler>();
            services.AddScoped<GetReturnsByUserHandler>();

            // Logs
            services.AddScoped<GetOrderStatusLogsHandler>();
            services.AddScoped<GetStockLogsHandler>();

            return services;
        }
    }
}
