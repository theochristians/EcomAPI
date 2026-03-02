namespace EComAPI.Application.Common.Constants
{
    public static class SystemUsers
    {
        /// <summary>
        /// System user ID untuk operasi yang tidak dilakukan oleh user (register, seed data, background jobs)
        /// </summary>
        public static readonly Guid SystemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        /// <summary>
        /// System user name
        /// </summary>
        public const string SystemUserName = "SYSTEM";

        /// <summary>
        /// System user email
        /// </summary>
        public const string SystemUserEmail = "system@ecomapi.internal";
    }
}