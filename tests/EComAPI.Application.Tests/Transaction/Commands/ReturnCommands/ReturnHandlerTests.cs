using FluentAssertions;
using Moq;
using Xunit;
using EComAPI.Application.Common.Interfaces;
using EComAPI.Application.Common.Interfaces.Identity;
using EComAPI.Application.Transaction.Commands.ReturnCommands.CreateReturn;
using EComAPI.Application.Transaction.Commands.ReturnCommands.ApproveReturn;
using EComAPI.Application.Transaction.Commands.ReturnCommands.RejectReturn;
using EComAPI.Application.Transaction.Commands.ReturnCommands.MarkReturnRefunded;
using EComAPI.Application.Products.Interfaces;
using EComAPI.Application.Transaction.Interfaces;
using EComAPI.Domain.Transaction.Entities;

namespace EComAPI.Application.Tests.Transaction.Commands.ReturnCommands
{
    public class ReturnHandlerTests
    {
        private readonly Mock<IReturnRepository> _mockReturnRepository;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IStockLogRepository> _mockStockLogRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _orderId = Guid.NewGuid();

        public ReturnHandlerTests()
        {
            _mockReturnRepository = new Mock<IReturnRepository>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockStockLogRepository = new Mock<IStockLogRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
        }

        private void SetupAuthenticatedUser()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(true);
            _mockCurrentUser.Setup(x => x.UserId).Returns(_userId);
        }

        private Order CreateCompletedOrder()
        {
            var order = new Order(
                userId: _userId,
                orderNumber: "ORD-20250101-001",
                shippingRecipientName: "Test User",
                shippingPhone: "08123456789",
                shippingFullAddress: "Jl. Test No. 1",
                shippingCity: "Jakarta",
                shippingPostalCode: "12345",
                totalAmount: 100000,
                shippingCost: 15000,
                finalAmount: 115000,
                createdBy: _userId);

            // Transition to completed
            order.MarkAsPaid(_userId);
            order.MarkAsProcessing(_userId);
            order.MarkAsShipped("JNE", "TRACK123", _userId);
            order.MarkAsCompleted(_userId);

            // Add an order item
            var orderItem = new OrderItem(
                orderId: order.Id,
                productVariantId: Guid.NewGuid(),
                snapshotProductName: "Test Product",
                snapshotVariantName: "Default",
                snapshotPrice: 50000,
                quantity: 2,
                createdBy: _userId);
            order.AddItem(orderItem);

            return order;
        }

        // =========================================================
        // CREATE RETURN
        // =========================================================

        [Fact]
        public async Task CreateReturn_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var handler = new CreateReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var command = new CreateReturnCommand(
                _orderId, "Defective product",
                new List<CreateReturnItemDto> { new(Guid.NewGuid(), 1) });

            var result = await handler.Handle(command);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task CreateReturn_OrderTidakAda_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockOrderRepository
                .Setup(x => x.GetOrderByIdWithItemsAsync(_orderId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Order?)null);

            var handler = new CreateReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var command = new CreateReturnCommand(
                _orderId, "Defective",
                new List<CreateReturnItemDto> { new(Guid.NewGuid(), 1) });

            var result = await handler.Handle(command);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Order not found");
        }

        [Fact]
        public async Task CreateReturn_BukanPemilikOrder_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var otherUserId = Guid.NewGuid();
            var order = new Order(
                userId: otherUserId,
                orderNumber: "ORD-20250101-001",
                shippingRecipientName: "Other User",
                shippingPhone: "08123456789",
                shippingFullAddress: "Jl. Other No. 1",
                shippingCity: "Jakarta",
                shippingPostalCode: "12345",
                totalAmount: 100000,
                shippingCost: 15000,
                finalAmount: 115000,
                createdBy: otherUserId);

            _mockOrderRepository
                .Setup(x => x.GetOrderByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var handler = new CreateReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var command = new CreateReturnCommand(
                order.Id, "Defective",
                new List<CreateReturnItemDto> { new(Guid.NewGuid(), 1) });

            var result = await handler.Handle(command);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("You can only request returns for your own orders");
        }

        [Fact]
        public async Task CreateReturn_OrderBelumCompleted_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            var order = new Order(
                userId: _userId,
                orderNumber: "ORD-20250101-001",
                shippingRecipientName: "Test User",
                shippingPhone: "08123456789",
                shippingFullAddress: "Jl. Test No. 1",
                shippingCity: "Jakarta",
                shippingPostalCode: "12345",
                totalAmount: 100000,
                shippingCost: 15000,
                finalAmount: 115000,
                createdBy: _userId);
            // Order is still pending_payment - not completed

            _mockOrderRepository
                .Setup(x => x.GetOrderByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            var handler = new CreateReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var command = new CreateReturnCommand(
                order.Id, "Defective",
                new List<CreateReturnItemDto> { new(Guid.NewGuid(), 1) });

            var result = await handler.Handle(command);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Returns can only be requested for completed orders");
        }

        [Fact]
        public async Task CreateReturn_ReturnSudahAda_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var order = CreateCompletedOrder();

            _mockOrderRepository
                .Setup(x => x.GetOrderByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _mockReturnRepository
                .Setup(x => x.ReturnExistsForOrderAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var handler = new CreateReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var orderItem = order.Items.First();
            var command = new CreateReturnCommand(
                order.Id, "Defective",
                new List<CreateReturnItemDto> { new(orderItem.Id, 1) });

            var result = await handler.Handle(command);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("A return request already exists for this order");
        }

        [Fact]
        public async Task CreateReturn_ItemTidakAdaDiOrder_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();
            var order = CreateCompletedOrder();

            _mockOrderRepository
                .Setup(x => x.GetOrderByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _mockReturnRepository
                .Setup(x => x.ReturnExistsForOrderAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockReturnRepository
                .Setup(x => x.GenerateReturnNumberAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("RET-20250101-001");

            var handler = new CreateReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var fakeItemId = Guid.NewGuid();
            var command = new CreateReturnCommand(
                order.Id, "Defective",
                new List<CreateReturnItemDto> { new(fakeItemId, 1) });

            var result = await handler.Handle(command);

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Contain("does not belong to this order");
        }

        [Fact]
        public async Task CreateReturn_Valid_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();
            var order = CreateCompletedOrder();

            _mockOrderRepository
                .Setup(x => x.GetOrderByIdWithItemsAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(order);

            _mockReturnRepository
                .Setup(x => x.ReturnExistsForOrderAsync(order.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockReturnRepository
                .Setup(x => x.GenerateReturnNumberAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("RET-20250101-001");

            var handler = new CreateReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object,
                _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var orderItem = order.Items.First();
            var command = new CreateReturnCommand(
                order.Id, "Product defective",
                new List<CreateReturnItemDto> { new(orderItem.Id, 1) },
                BankName: "BCA",
                BankAccountNumber: "1234567890",
                AccountHolderName: "Test User");

            var result = await handler.Handle(command);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();
        }

        // =========================================================
        // APPROVE RETURN
        // =========================================================

        [Fact]
        public async Task ApproveReturn_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var handler = new ApproveReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object, _mockProductRepository.Object,
                _mockStockLogRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new ApproveReturnCommand(Guid.NewGuid(), 50000));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task ApproveReturn_ReturnTidakAda_ShouldReturnFailure()
        {
            SetupAuthenticatedUser();

            _mockReturnRepository
                .Setup(x => x.GetReturnWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Return?)null);

            var handler = new ApproveReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object, _mockProductRepository.Object,
                _mockStockLogRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new ApproveReturnCommand(Guid.NewGuid(), 50000));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Return not found");
        }

        [Fact]
        public async Task ApproveReturn_Valid_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();

            var returnEntity = new Return(
                _orderId, _userId, "RET-20250101-001", "Defective", _userId);

            _mockReturnRepository
                .Setup(x => x.GetReturnWithDetailsAsync(returnEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnEntity);

            var handler = new ApproveReturnHandler(
                _mockReturnRepository.Object, _mockOrderRepository.Object, _mockProductRepository.Object,
                _mockStockLogRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new ApproveReturnCommand(returnEntity.Id, 50000));

            result.IsSuccess.Should().BeTrue();
        }

        // =========================================================
        // REJECT RETURN
        // =========================================================

        [Fact]
        public async Task RejectReturn_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var handler = new RejectReturnHandler(
                _mockReturnRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new RejectReturnCommand(Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task RejectReturn_Valid_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();

            var returnEntity = new Return(
                _orderId, _userId, "RET-20250101-001", "Defective", _userId);

            _mockReturnRepository
                .Setup(x => x.GetReturnByIdAsync(returnEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnEntity);

            var handler = new RejectReturnHandler(
                _mockReturnRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new RejectReturnCommand(returnEntity.Id));

            result.IsSuccess.Should().BeTrue();
        }

        // =========================================================
        // MARK RETURN REFUNDED
        // =========================================================

        [Fact]
        public async Task MarkReturnRefunded_UserBelumLogin_ShouldReturnFailure()
        {
            _mockCurrentUser.Setup(x => x.IsAuthenticated).Returns(false);

            var handler = new MarkReturnRefundedHandler(
                _mockReturnRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new MarkReturnRefundedCommand(Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User not authenticated");
        }

        [Fact]
        public async Task MarkReturnRefunded_Valid_ShouldReturnSuccess()
        {
            SetupAuthenticatedUser();

            var returnEntity = new Return(
                _orderId, _userId, "RET-20250101-001", "Defective", _userId);
            returnEntity.Approve(50000, _userId); // Must be approved first

            _mockReturnRepository
                .Setup(x => x.GetReturnByIdAsync(returnEntity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnEntity);

            var handler = new MarkReturnRefundedHandler(
                _mockReturnRepository.Object, _mockCurrentUser.Object, _mockUnitOfWork.Object);

            var result = await handler.Handle(new MarkReturnRefundedCommand(returnEntity.Id));

            result.IsSuccess.Should().BeTrue();
        }
    }
}
