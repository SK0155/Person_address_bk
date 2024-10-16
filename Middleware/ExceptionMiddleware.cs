using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using reactpersonaddress.Models;

namespace reactpersonaddress.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var response = new ApiResponse((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.", ex.Message);
            context.Response.ContentType = "application/json";

            switch (ex)
            {
                case ArgumentNullException _:
                    response = new ApiResponse((int)HttpStatusCode.BadRequest, "Bad request", ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case InvalidOperationException _:
                    response = new ApiResponse((int)HttpStatusCode.Conflict, "Conflict", ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    break;

                // Add more exception types as needed
                // case SomeCustomException _:
                //     response = new ApiResponse((int)HttpStatusCode.Custom, "Custom error", ex.Message);
                //     context.Response.StatusCode = (int)HttpStatusCode.Custom;
                //     break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}

