namespace EComAPI.API.DependencyInjection;

public sealed class ApiRateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public string RejectionMessage { get; init; } =
        "Too many requests. Please try again later.";

    public FixedWindowPolicyOptions AuthRegister { get; init; } = new();
    public FixedWindowPolicyOptions AuthLogin { get; init; } = new();
    public FixedWindowPolicyOptions EmailVerifySend { get; init; } = new();
    public FixedWindowPolicyOptions EmailVerifyCheck { get; init; } = new();
    public FixedWindowPolicyOptions UploadSas { get; init; } = new();
    public FixedWindowPolicyOptions UploadConfirm { get; init; } = new();
}

public sealed class FixedWindowPolicyOptions
{
    public int PermitLimit { get; init; } = 1;
    public int WindowMinutes { get; init; } = 1;
}