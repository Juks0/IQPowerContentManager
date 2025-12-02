using System;

namespace IQPowerContentManager.Api.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public string ErrorMessage { get; set; }

        public static ApiResponse<T> Ok(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Error(string error, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = error,
                Message = message
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static new ApiResponse Error(string error, string message = null)
        {
            return new ApiResponse
            {
                Success = false,
                ErrorMessage = error,
                Message = message
            };
        }
    }
}

