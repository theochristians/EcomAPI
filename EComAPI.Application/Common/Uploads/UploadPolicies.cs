namespace EComAPI.Application.Common.Uploads
{
    internal static class UploadPolicies
    {
        private static readonly HashSet<string> AllowedContentTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "image/jpeg",
                "image/jpg",
                "image/png",
                "image/webp"
            };

        public static bool IsAllowedContentType(string contentType)
            => AllowedContentTypes.Contains(contentType);

        public static long GetMaxSizeBytes(UploadPurpose purpose)
            => purpose == UploadPurpose.UserAvatar
                ? 2L * 1024L * 1024L
                : 10L * 1024L * 1024L;

        public static string GetExtension(string contentType, string? fileName)
        {
            var extension = Path.GetExtension(fileName ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(extension))
                return extension.ToLowerInvariant();

            return contentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => ".bin"
            };
        }

        public static bool TryParsePurpose(string value, out UploadPurpose purpose)
        {
            if (Enum.TryParse(value, ignoreCase: true, out purpose))
                return true;

            // Support snake_case values from frontend.
            var normalized = value.Trim().ToLowerInvariant();
            return normalized switch
            {
                "user_avatar" => TrySet(UploadPurpose.UserAvatar, out purpose),
                "category_image" => TrySet(UploadPurpose.CategoryImage, out purpose),
                "product_image" => TrySet(UploadPurpose.ProductImage, out purpose),
                "payment_proof" => TrySet(UploadPurpose.PaymentProof, out purpose),
                "return_image" => TrySet(UploadPurpose.ReturnImage, out purpose),
                "review_image" => TrySet(UploadPurpose.ReviewImage, out purpose),
                _ => false
            };
        }

        private static bool TrySet(UploadPurpose value, out UploadPurpose purpose)
        {
            purpose = value;
            return true;
        }
    }
}
