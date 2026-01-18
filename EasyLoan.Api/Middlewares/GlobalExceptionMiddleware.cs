using EasyLoan.Business.Exceptions;
using EasyLoan.Dtos.Common;
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

                await WriteResponseAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Internal server error");
            }
        }

        private static async Task HandleEasyLoanException(
            HttpContext context,
            EasyLoanException exception)
        {
            var statusCode = exception switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                AuthenticationFailedException => HttpStatusCode.Unauthorized,
                ForbiddenException => HttpStatusCode.Forbidden,
                NotFoundException => HttpStatusCode.NotFound,
                BusinessRuleViolationException => HttpStatusCode.UnprocessableEntity,
                _ => HttpStatusCode.InternalServerError
            };

            await WriteResponseAsync(context, statusCode, exception.Message);
        }

        private static async Task WriteResponseAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new ApiResponseDto<object>
            {
                Success = false,
                Message = message,
                Data = null
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}
