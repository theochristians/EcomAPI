using EComAPI.Application.Auth.Interfaces;
using EComAPI.Application.Common.Constants;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Common.Result;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Common.Exceptions;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Transaction.Commands.OrderCommands.CreateOrder
{
    public class CreateOrderHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderStatusLogRepository _orderStatusLogRepository;
        private readonly IStockLogRepository _stockLogRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderHandler(
            IOrderRepository orderRepository,
            ICouponRepository couponRepository,
            IAddressRepository addressRepository,
            IProductRepository productRepository,
            IOrderStatusLogRepository orderStatusLogRepository,
            IStockLogRepository stockLogRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _couponRepository = couponRepository;
            _addressRepository = addressRepository;
            _productRepository = productRepository;
            _orderStatusLogRepository = orderStatusLogRepository;
            _stockLogRepository = stockLogRepository;
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(
            CreateOrderCommand createOrderCommand,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return Result<string>.Failure("User not authenticated");

                if (createOrderCommand.Items == null || !createOrderCommand.Items.Any())
                    return Result<string>.Failure("Order must have at least one item");

                // ==========================================
                // VALIDATE & FETCH ADDRESS
                // ==========================================
                if (createOrderCommand.AddressId == Guid.Empty)
                    return Result<string>.Failure("AddressId is required");

                var address = await _addressRepository.GetAddressByIdAsync(
                    createOrderCommand.AddressId, cancellationToken);

                if (address == null)
                    return Result<string>.Failure("Address not found");

                if (address.UserId != _currentUser.UserId)
                    return Result<string>.Failure("Address not found");

                // ==========================================
                // FETCH VARIANTS & BUILD SNAPSHOT DATA
                // ==========================================
                var orderItemSnapshots = new List<(Domain.Products.Entities.ProductVariant Variant, string ProductName, string VariantName, decimal Price, int Quantity)>();

                foreach (var itemDto in createOrderCommand.Items)
                {
                    if (itemDto.ProductVariantId == Guid.Empty)
                        return Result<string>.Failure("ProductVariantId is required");

                    if (itemDto.Quantity < 1)
                        return Result<string>.Failure("Quantity must be at least 1");

                    var variant = await _productRepository.GetProductVariantWithProductAsync(
                        itemDto.ProductVariantId, cancellationToken);

                    if (variant == null)
                        return Result<string>.Failure("Product variant not found");

                    if (!variant.IsActive)
                        return Result<string>.Failure("Product variant is not active");

                    if (variant.Product == null)
                        return Result<string>.Failure("Product data not found for variant");

                    if (variant.Stock < itemDto.Quantity)
                        return Result<string>.Failure(
                            $"Insufficient stock for {variant.Product.Name}. Available: {variant.Stock}, Requested: {itemDto.Quantity}");

                    var variantParts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(variant.Size)) variantParts.Add(variant.Size);
                    if (!string.IsNullOrWhiteSpace(variant.Color)) variantParts.Add(variant.Color);
                    var variantName = variantParts.Count > 0 ? string.Join(" / ", variantParts) : variant.Sku;

                    var price = variant.Product.BasePrice + (variant.PriceAdjustment ?? 0);

                    orderItemSnapshots.Add((
                        variant,
                        variant.Product.Name,
                        variantName,
                        price,
                        itemDto.Quantity));
                }

                // ==========================================
                // CALCULATE SHIPPING COST
                // ==========================================
                var shippingCost = ShippingCosts.GetCost(createOrderCommand.ShippingType);

                // ==========================================
                // COUPON VALIDATION
                // ==========================================
                Coupon? coupon = null;
                decimal discountAmount = 0;

                if (!string.IsNullOrWhiteSpace(createOrderCommand.CouponCode))
                {
                    coupon = await _couponRepository.GetCouponByCodeAsync(
                        createOrderCommand.CouponCode, cancellationToken);

                    if (coupon == null)
                        return Result<string>.Failure("Coupon not found");

                    if (!coupon.IsValid(JakartaTime.Now))
                        return Result<string>.Failure("Coupon is no longer valid");

                    var alreadyUsed = await _couponRepository.UserHasUsedCouponAsync(
                        _currentUser.UserId, coupon.Id, cancellationToken);

                    if (alreadyUsed)
                        return Result<string>.Failure("You have already used this coupon");

                    var totalForCoupon = orderItemSnapshots.Sum(s => s.Price * s.Quantity);

                    if (totalForCoupon < coupon.MinimumPurchase)
                        return Result<string>.Failure(
                            $"Minimum purchase of {coupon.MinimumPurchase:N0} is required to use this coupon");

                    discountAmount = coupon.CalculateDiscount(totalForCoupon);
                }

                // ==========================================
                // GENERATE ORDER NUMBER
                // ==========================================
                var orderNumber = $"INV-{JakartaTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

                while (await _orderRepository.OrderNumberExistsAsync(orderNumber, cancellationToken))
                    orderNumber = $"INV-{JakartaTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

                // ==========================================
                // CALCULATE AMOUNTS
                // ==========================================
                var subTotal = orderItemSnapshots.Sum(s => s.Price * s.Quantity);
                var finalAmount = subTotal + shippingCost - discountAmount;

                // ==========================================
                // CREATE ORDER
                // ==========================================
                var order = new Order(
                    userId: createOrderCommand.UserId,
                    orderNumber: orderNumber,
                    shippingRecipientName: address.RecipientName,
                    shippingPhone: address.RecipientPhone,
                    shippingFullAddress: address.FullAddress,
                    shippingCity: address.City,
                    shippingPostalCode: address.PostalCode,
                    totalAmount: subTotal,
                    shippingCost: shippingCost,
                    finalAmount: finalAmount,
                    createdBy: _currentUser.UserId,
                    addressId: createOrderCommand.AddressId,
                    couponId: coupon?.Id,
                    discountAmount: discountAmount,
                    customerNote: createOrderCommand.CustomerNote);

                await _orderRepository.AddOrderAsync(order, cancellationToken);

                // ==========================================
                // CREATE ORDER ITEMS
                // ==========================================
                foreach (var snapshot in orderItemSnapshots)
                {
                    var orderItem = new OrderItem(
                        orderId: order.Id,
                        productVariantId: snapshot.Variant.Id,
                        snapshotProductName: snapshot.ProductName,
                        snapshotVariantName: snapshot.VariantName,
                        snapshotPrice: snapshot.Price,
                        quantity: snapshot.Quantity,
                        createdBy: _currentUser.UserId);

                    await _orderRepository.AddOrderItemAsync(orderItem, cancellationToken);
                }

                // ==========================================
                // DEDUCT STOCK & CREATE STOCK LOGS
                // ==========================================
                foreach (var snapshot in orderItemSnapshots)
                {
                    var stockBefore = snapshot.Variant.Stock;
                    snapshot.Variant.DecreaseStock(snapshot.Quantity, _currentUser.UserId);
                    await _productRepository.UpdateProductVariantAsync(snapshot.Variant, cancellationToken);

                    var stockLog = new StockLog(
                        productVariantId: snapshot.Variant.Id,
                        type: "order",
                        quantityChange: -snapshot.Quantity,
                        stockBefore: stockBefore,
                        stockAfter: snapshot.Variant.Stock,
                        createdBy: _currentUser.UserId,
                        referenceType: "Order",
                        referenceId: order.Id,
                        note: $"Stock deducted for order {orderNumber}");

                    await _stockLogRepository.AddLogAsync(stockLog, cancellationToken);
                }

                // ==========================================
                // CREATE PAYMENT RECORD
                // ==========================================
                var payment = new Payment(
                    orderId: order.Id,
                    amount: finalAmount,
                    createdBy: _currentUser.UserId);

                await _orderRepository.AddPaymentAsync(payment, cancellationToken);

                // ==========================================
                // LOG INITIAL ORDER STATUS
                // ==========================================
                var statusLog = new OrderStatusLog(
                    orderId: order.Id,
                    status: OrderStatus.PendingPayment,
                    createdBy: _currentUser.UserId,
                    note: "Order created");

                await _orderStatusLogRepository.AddLogAsync(statusLog, cancellationToken);

                // ==========================================
                // APPLY COUPON USAGE
                // ==========================================
                if (coupon != null)
                {
                    coupon.Use(_currentUser.UserId);
                    await _couponRepository.UpdateCouponAsync(coupon, cancellationToken);

                    var couponUsage = new CouponUsage(
                        couponId: coupon.Id,
                        userId: _currentUser.UserId,
                        orderId: order.Id);

                    await _couponRepository.AddCouponUsageAsync(couponUsage, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(orderNumber);
            }
            catch (DomainException domainException)
            {
                return Result<string>.Failure(domainException.Message);
            }
            catch (Exception exception) when (exception.GetType().Name == "DbUpdateConcurrencyException")
            {
                return Result<string>.Failure("Stock changed while processing order. Please refresh cart and try again");
            }
            catch (Exception)
            {
                return Result<string>.Failure("An error occurred while creating the order");
            }
        }
    }
}
