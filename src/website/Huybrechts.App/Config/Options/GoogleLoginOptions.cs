using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.App.Config.Options;

public class GoogleLoginOptions
{
	[Required] public string ClientId { get; set; } = string.Empty;

	[Required] public string ClientSecret { get; set; } = string.Empty;
}
