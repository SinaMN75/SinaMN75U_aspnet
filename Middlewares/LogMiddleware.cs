namespace SinaMN75U.Middlewares;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class ApiRequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<ApiRequestLoggingMiddleware> logger,
    string logFilePath = "api_logs.txt") {
    public async Task InvokeAsync(HttpContext context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew(); // Start measuring time

        HttpRequest request = context.Request;
        Stream originalRequestBodyStream = request.Body;

        // Read request body
        string requestBody = "";
        try
        {
            request.EnableBuffering();
            using StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read request body");
        }

        // Capture response
        Stream originalResponseBodyStream = context.Response.Body;
        using MemoryStream responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        await next(context); // Continue processing the request

        // Read response body
        string responseBody = "";
        try
        {
            responseBodyStream.Position = 0;
            responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
            responseBodyStream.Position = 0;
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read response body");
        }

        stopwatch.Stop(); // Stop timer
        long elapsedMs = stopwatch.ElapsedMilliseconds;

        // Build log entry
        StringBuilder logEntry = new StringBuilder();
        logEntry.AppendLine($"{request.Method} - {request.Path} - {context.Response.StatusCode} | {elapsedMs}ms");
        logEntry.AppendLine("Headers:");
        foreach (KeyValuePair<string, StringValues> header in request.Headers)
        {
            logEntry.AppendLine($"  {header.Key}: {header.Value}");
        }
        logEntry.AppendLine($"Request Body: {requestBody}");
        logEntry.AppendLine($"Response Body: {responseBody}");
        logEntry.AppendLine(new string('-', 50)); // Separator

        // Write to file (async)
        try
        {
            await File.AppendAllTextAsync(logFilePath, logEntry.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write to log file");
        }
    }
}