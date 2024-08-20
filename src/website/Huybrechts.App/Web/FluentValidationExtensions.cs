using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Huybrechts.App.Web;

public static class FluentValidationExtensions
{
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState, string prefix = "")
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(prefix + error.PropertyName, error.ErrorMessage);
        }
    }
}
