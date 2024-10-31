using Huybrechts.Landing.Web.Pages;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace Huybrechts.Landing.Tests.Pages;

public class PrivacyModelTests
{
    [Fact]
    public void OnGet_DoesNotThrowException()
    {
        // Arrange
        var pageModel = new PrivacyModel();

        // Act & Assert
        var exception = Record.Exception(() => pageModel.OnGet());

        // Verify that no exception is thrown
        Assert.Null(exception);
    }

    [Fact]
    public void IndexModel_HasAllowAnonymousAttribute()
    {
        // Arrange
        var allowAnonymousAttribute = typeof(PrivacyModel)
            .GetCustomAttribute<AllowAnonymousAttribute>();

        // Assert
        Assert.NotNull(allowAnonymousAttribute);
    }
}