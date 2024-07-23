using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Huybrechts.App.Data;

public class ContextTransactionPageFilter<T> : IAsyncPageFilter where T : FeatureContext
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<T>();

        try
        {
            await dbContext.BeginTransactionAsync();

            var actionExecuted = await next();
            if (actionExecuted.Exception != null && !actionExecuted.ExceptionHandled)
            {
                dbContext.RollbackTransaction();
            }
            else
            {
                await dbContext.CommitTransactionAsync();
            }
        }
        catch (Exception)
        {
            dbContext.RollbackTransaction();
            throw;
        }
    }
}
