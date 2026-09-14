using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KosmosERP.Api.Filters;

public class ValidationErrorLoggingFilter : IActionFilter
{
    private readonly ILogger<ValidationErrorLoggingFilter> _logger;

    public ValidationErrorLoggingFilter(ILogger<ValidationErrorLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            _logger.LogWarning(
                "Action {Action} on {Controller} failed model validation. Errors: {@Errors}",
                context.ActionDescriptor.DisplayName,
                context.Controller.GetType().Name,
                errors
            );
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Nothing to do after action execution
    }
}
