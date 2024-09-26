using FluentResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Huybrechts.App.Web;

[Serializable]
public class StatusResult
{
    public static StatusResult Deserialize(string json)
    {
        if (IsProbablyJson(json))
            return System.Text.Json.JsonSerializer.Deserialize<StatusResult>(json) ?? new();
        else
            return new()
            {
                IsException = false,
                IsFailed = false,
                IsSuccess = false,
                Message = json
            };
    }

    private static bool IsProbablyJson(string input)
    {
        input = input.Trim();
        return (input.StartsWith('{') && input.EndsWith('}')) || (input.StartsWith('[') && input.EndsWith(']'));
    }

    public bool IsException { get; set; } = false;

    public bool IsFailed { get; set; } = false;

    public bool IsSuccess { get; set; } = false;

    public string Message { get; set; } = string.Empty;

    public StatusResult() { }
}

public static class FluentResultExtensions
{
    public static void AddToModelState(this Result result, ModelStateDictionary modelState, string prefix = "")
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(prefix, error.Message);
        }
    }

    public static void AddToModelState<T>(this Result<T> result, ModelStateDictionary modelState, string prefix = "")
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(prefix, error.Message);
        }
    }

    public static bool HasStatusMessage(this Result result)
    {
        if (result.Reasons.Count > 0)
            return true;
        return false;
    }

    public static bool HasStatusMessage<T>(this Result<T> result)
    {
        if (result.Reasons.Count > 0)
            return true;
        return false;
    }

    public static StatusResult ToStatusResult(this Result result)
    {
        return new StatusResult()
        {
            IsFailed = result.IsFailed,
            IsSuccess = result.IsSuccess,
            Message =
                (result.IsFailed ? string.Join(Environment.NewLine, result.Errors.Select(s => s.Message)) :
                result.IsSuccess ? string.Join(Environment.NewLine, result.Successes.Select(s => s.Message)) :
                string.Join(Environment.NewLine, result.Reasons.Select(s => s.Message)))
        };
    }

    public static string ToStatusMessage(this Result result)
    {
        StatusResult value = result.ToStatusResult();
        return System.Text.Json.JsonSerializer.Serialize(value);
    }

    public static StatusResult ToStatusResult<T>(this Result<T> result)
    {
        return new StatusResult()
        {
            IsFailed = result.IsFailed,
            IsSuccess = result.IsSuccess,
            Message =
                (result.IsFailed ? string.Join(Environment.NewLine, result.Errors.Select(s => s.Message)) :
                result.IsSuccess ? string.Join(Environment.NewLine, result.Successes.Select(s => s.Message)) :
                string.Join(Environment.NewLine, result.Reasons.Select(s => s.Message)))
        };
    }

    public static string ToStatusMessage<T>(this Result<T> result)
    {
        StatusResult value = result.ToStatusResult();
        return System.Text.Json.JsonSerializer.Serialize(value);
    }
}
