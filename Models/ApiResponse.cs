using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace reactpersonaddress.Models
{
    // Models/ApiResponse.cs

    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Detail { get; set; } // Use object to hold any type of data

        public ApiResponse(int statusCode, string message, object data = null)
        {
            StatusCode = statusCode;
            Message = message;
            Detail = data;
        }
    }
}