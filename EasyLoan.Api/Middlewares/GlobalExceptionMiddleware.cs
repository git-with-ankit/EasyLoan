using EasyLoan.Business.Exceptions;
using EasyLoan.Dtos.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace EasyLoan.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
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
            catch (EasyLoanException ex)
            {
                await HandleEasyLoanException(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                await WriteProblemDetailsResponseAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Internal server error",
                    "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    "An unexpected error occurred.");
            }
        }

        private static async Task HandleEasyLoanException(HttpContext context, EasyLoanException exception)
        {
            var (status, title, type) = exception switch
            {

                AuthenticationFailedException =>
                    (HttpStatusCode.Unauthorized,
                     "Authentication failed",
                     "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1"),

                ForbiddenException =>
                    (HttpStatusCode.Forbidden,
                     "Access denied",
                     "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3"),

                NotFoundException =>
                    (HttpStatusCode.NotFound,
                     "Resource not found",
                     "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4"),

                BusinessRuleViolationException =>
                    (HttpStatusCode.UnprocessableEntity,
                     "Unprocessable Entity",
                     "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2"),

                _ =>
                    (HttpStatusCode.InternalServerError,
                     "Internal server error",
                     "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
            };

            await WriteProblemDetailsResponseAsync(
                context,
                status,
                title,
                type,
                exception.Message
            );
        }

        //private static async Task WriteResponseAsync(
        //    HttpContext context,
        //    HttpStatusCode statusCode,
        //    string message)
        //{
        //    context.Response.ContentType = "application/json";
        //    context.Response.StatusCode = (int)statusCode;

        //    var response = new ApiResponseDto<object>
        //    {
        //        Success = false,
        //        Message = message,
        //        Data = null
        //    };

        //    await context.Response.WriteAsync(
        //        JsonSerializer.Serialize(response));
        //}

        private static async Task WriteProblemDetailsResponseAsync(HttpContext context, HttpStatusCode status, string title, string type, string detail)
        {
            var problemDetails = new ProblemDetails()
            {
                Title = title,
                Status = (int)status,
                Detail = detail,
                Type = type,
                Instance = context.Request.Path
            };
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(
               JsonSerializer.Serialize(problemDetails));
        }
    }
}
