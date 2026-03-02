using EComAPI.Domain.Auth.Entities;

namespace EComAPI.Application.Auth.Interfaces
{
    public interface IAddressRepository
    {
        // =========================
        // QUERY (READ-ONLY)
        // Ambil data saja, tidak mengubah state.
        // =========================
        Task<Address?> GetAddressByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Address>> GetUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Address?> GetDefaultAddressAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<int> CountUserAddressesAsync(Guid userId, CancellationToken cancellationToken = default);

        // =========================
        // COMMAND (WRITE/PERSISTENCE)
        // Simpan perubahan state entity ke database.
        // =========================
        Task AddAddressAsync(Address address, CancellationToken cancellationToken = default);
        Task UpdateAddressAsync(Address address, CancellationToken cancellationToken = default);

        // =========================
        // CONSISTENCY OPERATION
        // Operasi teknis untuk konsistensi data (default address tunggal).
        // =========================
        Task UnsetAllAddressDefaultsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
