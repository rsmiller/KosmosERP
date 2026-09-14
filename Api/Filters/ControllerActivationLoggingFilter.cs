using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KosmosERP.Api.Filters;

public class ControllerActivationLoggingFilter : IActionFilter
{
    private readonly ILogger<ControllerActivationLoggingFilter> _logger;

    public ControllerActivationLoggingFilter(ILogger<ControllerActivationLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        
        _logger.LogInformation(
            "Executing action {Action} on controller {Controller}. Parameters: {@Parameters}",
            controllerActionDescriptor?.ActionName,
            controllerActionDescriptor?.ControllerName,
            context.ActionArguments
        );

        // Log if there are any constructor parameters that might have failed
        try
        {
            var controllerType = context.Controller.GetType();
            var constructors = controllerType.GetConstructors();
            
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                _logger.LogDebug(
                    "Controller {Controller} has constructor with parameters: {Parameters}",
                    controllerType.Name,
                    string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while inspecting controller constructor");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null)
        {
            _logger.LogError(
                context.Exception,
                "Action {Action} on controller {Controller} threw an exception",
                context.ActionDescriptor.DisplayName,
                context.Controller.GetType().Name
            );
        }
    }
}
