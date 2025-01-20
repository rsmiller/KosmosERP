using System.ComponentModel.DataAnnotations;

namespace Prometheus.Models.Helpers;

public class ModelValidationHelper
{
    public static Response<T> ValidateModel<T>(T model)
        where T : class
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);

        Validator.TryValidateObject(model, context, validationResults, true);

        if (validationResults.Count > 0)
        {
            var errorMessages = validationResults.Select(x => x.ErrorMessage).ToList();
            return new Response<T>(errorMessages, ResultCode.Invalid);
        }

        return new Response<T>();
    }
}
