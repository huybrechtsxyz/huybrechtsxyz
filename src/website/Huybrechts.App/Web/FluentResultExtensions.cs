using FluentResults;

namespace Huybrechts.App.Web;

public class StatusResult
{
    public bool IsFailed { get; set; } = false;

    public bool IsSuccess { get; set; } = false;

    public IEnumerable<string> Errors { get; set; } = [];

    public IEnumerable<string> Successes { get; set; } = [];

    public IEnumerable<string> Reasons { get; set; } = [];
}

public static class FluentResultExtensions
{
    public static StatusResult ToStatusResult(this Result result)
    {
        return new StatusResult()
        {
            IsFailed = result.IsFailed,
            IsSuccess = result.IsSuccess,
            Errors = result.Errors.Select(s => s.Message),
            Successes = result.Successes.Select(s => s.Message),
            Reasons = result.Reasons.Select(s => s.Message)
        };
    }

    public static StatusResult ToStatusResult<T>(this Result<T> result)
    {
        return new StatusResult()
        {
            IsFailed = result.IsFailed,
            IsSuccess = result.IsSuccess,
            Errors = result.Errors.Select(s => s.Message),
            Successes = result.Successes.Select(s => s.Message),
            Reasons = result.Reasons.Select(s => s.Message)
        };
    }

    public static string ToStatusMessage(this Result result)
    {
        if (result.IsFailed)
            return "Error: " + string.Join(Environment.NewLine, result.Errors.Select(s => s.Message));
        else if (result.IsSuccess)
            return "Success: " + string.Join(Environment.NewLine, result.Successes.Select(s => s.Message));
        else
            return string.Join(Environment.NewLine, result.Reasons.Select(s => s.Message));
    }

    public static string ToStatusMessage<T>(this Result<T> result)
    {
        if (result.IsFailed)
            return "Error: " + string.Join(Environment.NewLine, result.Errors.Select(s => s.Message));
        else if (result.IsSuccess)
            return "Success: " + string.Join(Environment.NewLine, result.Successes.Select(s => s.Message));
        else
            return string.Join(Environment.NewLine, result.Reasons.Select(s => s.Message));
    }
}
