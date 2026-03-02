using EComAPI.Application.Auth.Interfaces;
using EComAPI.Domain.Auth.Entities;
using EComAPI.Infrastructure.Common.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EComAPI.Infrastructure.Auth.Persistence.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _appDbContext;

        public AddressRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<Address?> GetAddressByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Addresses
                .FirstOrDefaultAsync(address => address.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Address>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Addresses
                .Where(address => address.UserId == userId)
                .OrderByDescending(address => address.IsDefault)
                .ThenByDescending(address => address.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Address?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Addresses
                .AsNoTracking()
                .Where(address => address.UserId == userId && address.IsDefault)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<int> CountUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Addresses
                .CountAsync(address => address.UserId == userId, cancellationToken);
        }

        public async Task AddAddressAsync(Address address, CancellationToken cancellationToken = default)
        {
            await _appDbContext.Addresses.AddAsync(address, cancellationToken);
        }

        public Task UpdateAddressAsync(Address address, CancellationToken cancellationToken = default)
        {
            _appDbContext.Addresses.Update(address);
            return Task.CompletedTask;
        }

        public async Task UnsetAllAddressDefaultsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var addresses = await _appDbContext.Addresses
                .Where(address => address.UserId == userId && address.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var address in addresses)
            {
                address.UnsetDefault(userId);
            }
        }
    }
}
