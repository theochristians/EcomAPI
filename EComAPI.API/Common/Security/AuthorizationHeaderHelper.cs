namespace EComAPI.API.Common.Security
{
    public static class AuthorizationHeaderHelper
    {
        public static string? ExtractBearerToken(string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))
                return null;

            var header = authorizationHeader.Trim().Trim('"');

            // Jika ada multiple value: "Bearer a, Bearer b"
            var commaIndex = header.IndexOf(',');
            if (commaIndex >= 0)
                header = header[..commaIndex].Trim();

            var parts = header.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return null;

            if (!parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                return null;

            var token = parts[1].Trim().Trim('"');
            return string.IsNullOrWhiteSpace(token) ? null : token;
        }
    }
}
