namespace EComAPI.Application.Common.Uploads
{
    internal static class UploadPathBuilder
    {
        public static string BuildPath(
            UploadPurpose purpose,
            Guid userId,
            string extension,
            Guid uploadId)
        {
            var folder = purpose switch
            {
                UploadPurpose.UserAvatar => "avatars",
                UploadPurpose.CategoryImage => "categories",
                UploadPurpose.ProductImage => "products",
                UploadPurpose.PaymentProof => "payments",
                UploadPurpose.ReturnImage => "returns",
                UploadPurpose.ReviewImage => "reviews",
                _ => "misc"
            };

            return $"users/{userId:N}/{folder}/{uploadId:N}{extension}";
        }

        public static bool IsPathOwnedByUser(UploadPurpose purpose, Guid userId, string blobPath)
        {
            if (string.IsNullOrWhiteSpace(blobPath))
                return false;

            var folder = purpose switch
            {
                UploadPurpose.UserAvatar => "avatars",
                UploadPurpose.CategoryImage => "categories",
                UploadPurpose.ProductImage => "products",
                UploadPurpose.PaymentProof => "payments",
                UploadPurpose.ReturnImage => "returns",
                UploadPurpose.ReviewImage => "reviews",
                _ => "misc"
            };

            var expectedPrefix = $"users/{userId:N}/{folder}/";
            return blobPath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
