using Huybrechts.Website.Models;
using System.Globalization;

namespace Huybrechts.Helpers
{
    public class SettingHelper
	{
		private readonly IConfiguration _configuration;

		public static CultureInfo[] SupportedCultures => new[] { new CultureInfo("EN"), new CultureInfo("NL") };

		public SettingHelper(IConfiguration configuration)
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

		public AuthenticationSettings GetGoogleAuthentication()
		{
			AuthenticationSettings item = new();
			_configuration.GetSection("Authentication:Google").Bind(item);
            return item;
        }

        public MessagingAuthentication GetMessagingAuthentication()
        {
            MessagingAuthentication item = new();
            _configuration.GetSection("MessagingAuthentication").Bind(item);
            return item;
        }

        public MessagingSettings GetMessagingSettings()
        {
            MessagingSettings item = new();
            _configuration.GetSection("MessagingSettings").Bind(item);
            return item;
        }
    }
}
