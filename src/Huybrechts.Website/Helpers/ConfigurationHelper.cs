namespace Huybrechts.Helpers
{
	public class ConfigurationHelper
	{
		private readonly IConfiguration _configuration;

		public ConfigurationHelper(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public bool IsResetEnvironment
		{
			get
			{
				string env = _configuration["Environment:Initialization"] ?? string.Empty;
				if (!string.IsNullOrEmpty(env) && env.Equals("RESET", StringComparison.CurrentCultureIgnoreCase))
					return true;
				return false;
			}
		}

		public bool IsInitializeEnvironment
		{
			get
			{
				string env = _configuration["Environment:Initialization"] ?? string.Empty;
				if (!string.IsNullOrEmpty(env) && env.Equals("INITIALIZE", StringComparison.CurrentCultureIgnoreCase))
					return true;
				return false;
			}
		}
	}
}
