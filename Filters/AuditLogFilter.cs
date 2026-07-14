using Microsoft.AspNetCore.Mvc.Filters;

namespace TmsApi.Filters;

// Module 6 - Session 2 - Exercise 4, Part D: cross-cutting concern.
// Logs every API call and its status code — the smallest possible breadcrumb
// an operations team needs. This does NOT belong in every controller action;
// filters are for cross-cutting concerns, not business logic. If you find
// yourself writing "if (course.IsFull)" inside a filter, that decision
// belongs in the service instead.
public class AuditLogFilter(ILogger<AuditLogFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var route = context.HttpContext.Request.Path;
        var method = context.HttpContext.Request.Method;
        logger.LogInformation("TMS API call: {Method} {Route}", method, route);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var status = context.HttpContext.Response.StatusCode;
        logger.LogInformation("TMS API response: {StatusCode}", status);
    }
}