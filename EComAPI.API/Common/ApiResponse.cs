namespace EComAPI.API.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; }
        public T? Data { get; init; }

        protected ApiResponse(bool success, string message, T? data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static ApiResponse<T> Ok(T data, string message = "Success")
            => new(true, message, data);

        public static ApiResponse<T> Fail(string message)
            => new(false, message, default);
    }
}