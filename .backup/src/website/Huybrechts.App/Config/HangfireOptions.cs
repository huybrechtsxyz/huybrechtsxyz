using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Huybrechts.App.Config;

public class HangfireOptions
{
	public bool InMemoryStorage { get; set; } = true;
}
