namespace EComAPI.API.Common
{
    public class PaginatedResponse<T> : ApiResponse<T>
    {
        public int TotalCount { get; init; }
        public int CurrentPage { get; init; }
        public int TotalPages { get; init; }

        private PaginatedResponse(
            string message,
            T data,
            int totalCount,
            int currentPage,
            int pageSize
        ) : base(true, message, data)
        {
            TotalCount = totalCount;
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public static PaginatedResponse<T> Ok(T data,
            int totalCount,
            int currentPage,
            int pageSize,
            string message = "Success"
        )
        {
            return new PaginatedResponse<T>(
                message,
                data,
                totalCount,
                currentPage,
                pageSize
            );
        }
    }
}