using FluentResults;
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

    public static Result AsResult(this ValidationResult validation)
    {
        if (validation.IsValid)
            return Result.Ok();
        Result result = new();
        validation.Errors.ForEach(error => { result.WithError(error.ErrorMessage); });
        return result;
    }
}
