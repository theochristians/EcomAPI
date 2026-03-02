using System.Text.Json.Serialization;

namespace EComAPI.API.Common
{
    public class PaginatedApiResponse<T>
    {
        [JsonPropertyOrder(1)]
        public bool Success { get; init; }

        [JsonPropertyOrder(2)]
        public string Message { get; init; }

        [JsonPropertyOrder(3)]
        public T Data { get; init; }

        [JsonPropertyOrder(4)]
        public int TotalCount { get; init; }

        [JsonPropertyOrder(5)]
        public int CurrentPage { get; init; }

        [JsonPropertyOrder(6)]
        public int TotalPages { get; init; }

        [JsonPropertyOrder(7)]
        public int PageSize { get; init; }

        [JsonPropertyOrder(8)]
        public bool HasPreviousPage { get; init; }

        [JsonPropertyOrder(9)]
        public bool HasNextPage { get; init; }

        // ⭐ NEW: Navigation links
        [JsonPropertyOrder(10)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PreviousPageUrl { get; init; }

        [JsonPropertyOrder(11)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? NextPageUrl { get; init; }

        [JsonPropertyOrder(12)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FirstPageUrl { get; init; }

        [JsonPropertyOrder(13)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LastPageUrl { get; init; }

        public PaginatedApiResponse(
            bool success,
            string message,
            T data,
            int totalCount,
            int currentPage,
            int totalPages,
            int pageSize,
            bool hasPreviousPage,
            bool hasNextPage,
            string? previousPageUrl = null,
            string? nextPageUrl = null,
            string? firstPageUrl = null,
            string? lastPageUrl = null)
        {
            Success = success;
            Message = message;
            Data = data;
            TotalCount = totalCount;
            CurrentPage = currentPage;
            TotalPages = totalPages;
            PageSize = pageSize;
            HasPreviousPage = hasPreviousPage;
            HasNextPage = hasNextPage;
            PreviousPageUrl = previousPageUrl;
            NextPageUrl = nextPageUrl;
            FirstPageUrl = firstPageUrl;
            LastPageUrl = lastPageUrl;
        }

        public static PaginatedApiResponse<T> Create(
            T data,
            int totalCount,
            int currentPage,
            int pageSize,
            string baseUrl,
            string queryString,
            string message = "Success")
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var hasPreviousPage = currentPage > 1;
            var hasNextPage = currentPage < totalPages;

            // ⭐ Build navigation URLs
            string? previousPageUrl = null;
            string? nextPageUrl = null;
            string? firstPageUrl = null;
            string? lastPageUrl = null;

            if (hasPreviousPage)
            {
                previousPageUrl = BuildPageUrl(baseUrl, queryString, currentPage - 1, pageSize);
                firstPageUrl = BuildPageUrl(baseUrl, queryString, 1, pageSize);
            }

            if (hasNextPage)
            {
                nextPageUrl = BuildPageUrl(baseUrl, queryString, currentPage + 1, pageSize);
                lastPageUrl = BuildPageUrl(baseUrl, queryString, totalPages, pageSize);
            }

            return new PaginatedApiResponse<T>(
                true,
                message,
                data,
                totalCount,
                currentPage,
                totalPages,
                pageSize,
                hasPreviousPage,
                hasNextPage,
                previousPageUrl,
                nextPageUrl,
                firstPageUrl,
                lastPageUrl
            );
        }

        private static string BuildPageUrl(string baseUrl, string queryString, int page, int pageSize)
        {
            // Parse existing query string
            var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);

            // Update page and pageSize
            queryParams["page"] = page.ToString();
            queryParams["pageSize"] = pageSize.ToString();

            return $"{baseUrl}?{queryParams}";
        }
    }
}