namespace EComAPI.Application.Common.Interfaces
{
    public interface ITokenBlacklistLifetimeProvider
    {
        TimeSpan FallbackLifetime { get; }
        TimeSpan MaxLifetime { get; }
    }
}
