using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Shopping.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Shopping.Entities;

namespace EComAPI.Application.Shopping.Commands.CartCommands.CreateCart
{
    public class CreateCartHandler
    {
        private readonly ICartRepository _cartRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCartHandler(
            ICartRepository cartRepository,
            IUnitOfWork unitOfWork)
        {
            _cartRepository = cartRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(
            CreateCartCommand createCartCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (createCartCommand.UserId == Guid.Empty)
                    return Result<Guid>.Failure("UserId is required");

                var existingCart = await _cartRepository.GetCartByUserIdAsync(
                    createCartCommand.UserId, cancellationToken);

                if (existingCart != null)
                    return Result<Guid>.Success(existingCart.Id);

                var cart = new Cart(createCartCommand.UserId, createCartCommand.UserId);

                await _cartRepository.AddCartAsync(cart, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<Guid>.Success(cart.Id);
            }
            catch (DomainException domainException)
            {
                return Result<Guid>.Failure(domainException.Message);
            }
            catch (Exception exception)
            {
                return Result<Guid>.Failure($"Failed to create cart: {exception.Message}");
            }
        }
    }
}
