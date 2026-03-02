using EComAPI.Application.Categories.Interfaces;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Products.Entities;

namespace EComAPI.Application.Products.Commands.ProductCommands.CreateProduct
{
    public class CreateProductHandler
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoriesRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductHandler(
            IProductRepository productRepository,
            ICategoryRepository categoriesRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoriesRepository = categoriesRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateProductCommand createProductCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<Guid>.Failure("User not authenticated");

                if (createProductCommand.CategoryId == Guid.Empty)
                    return Result<Guid>.Failure("Category ID is required");

                if (string.IsNullOrWhiteSpace(createProductCommand.Name))
                    return Result<Guid>.Failure("Product name is required");

                if (string.IsNullOrWhiteSpace(createProductCommand.Slug))
                    return Result<Guid>.Failure("Product slug is required");

                if (createProductCommand.BasePrice < 0)
                    return Result<Guid>.Failure("Base price cannot be negative");

                var categoryExists = await _categoriesRepository.CategoryExistsAsync(
                    createProductCommand.CategoryId,
                    cancellationToken);

                if (!categoryExists)
                    return Result<Guid>.Failure("Category not found");

                var productSlugExists = await _productRepository.ProductExistsBySlugAsync(
                    createProductCommand.Slug,
                    cancellationToken);

                if (productSlugExists)
                    return Result<Guid>.Failure("Product slug already exists");

                var product = new Product(
                    createProductCommand.CategoryId,
                    createProductCommand.Name,
                    createProductCommand.Slug,
                    createProductCommand.BasePrice,
                    _currentUser.UserId,
                    createProductCommand.Description
                );

                await _productRepository.AddProductAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(product.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to create product: {exception.Message}");
            }
        }
    }
}
