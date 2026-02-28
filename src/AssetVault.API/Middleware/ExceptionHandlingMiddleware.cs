using System.Net;
using System.Text.Json;

namespace AssetVault.API.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage });
                await WriteErrorResponse(context, HttpStatusCode.BadRequest, errors);
            }
            catch (UnauthorizedAccessException ex)
            {
                await WriteErrorResponse(context, HttpStatusCode.Forbidden, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception)
            {
                await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }

        private static Task WriteErrorResponse(HttpContext context, HttpStatusCode status, string message)
        {
            return WriteErrorResponse(context, status, [new { field = (string?)null, message }]);
        }

        private static Task WriteErrorResponse(HttpContext context, HttpStatusCode status, IEnumerable<object> errors)
        {
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(JsonSerializer.Serialize(new { errors }));
        }
    }
}
