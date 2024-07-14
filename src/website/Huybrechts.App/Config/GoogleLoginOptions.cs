using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Huybrechts.App.Config;

public class GoogleLoginOptions
{
	[Required] public string ClientId { get; set; } = string.Empty;

	[Required] public string ClientSecret { get; set; } = string.Empty;

	public string ToLogString() 
	{ 
		var builder = new StringBuilder();
		builder.AppendLine("GoogleLoginOptions");
		builder.Append("    ClientId: ").AppendLine(ClientId);
		builder.Append("    ClientSecret: ").AppendLine(ClientSecret);
		return builder.ToString();
	}
}
