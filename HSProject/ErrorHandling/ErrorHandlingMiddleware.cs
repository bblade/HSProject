
using HSProject.Models;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;


namespace HSProject.ErrorHandling;

public class ErrorHandlingMiddleware {
    private readonly RequestDelegate next;
    private readonly ILogger<ErrorHandlingMiddleware> logger;
    public ErrorHandlingMiddleware(RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger) {
        this.next = next;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext context) {
        try {
            await next(context);

        } catch (ValidationException ex) {

            await WriteResponse(context, ex,
                "Bad request body", "validation error", 400);

        } catch (ManifestInvalidException ex) {

            await WriteResponse(context, ex,
                "Manifest file is invalid", "import error", 400);

        } catch (Exception ex) {

            await WriteResponse(context, ex, "An error occured while processing your request", "unknown", 500);

        }
    }

    private async Task WriteResponse(HttpContext context, Exception ex, string message, string errorCode, int httpCode) {
        logger.LogError($"Url: {context.Request.Path} Exception: {ex}");
        context.Response.StatusCode = httpCode;
        context.Response.ContentType = "application/json";

        ResponseDto errorResponse = new() { Message = message, ErrorCode = errorCode };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }
}

