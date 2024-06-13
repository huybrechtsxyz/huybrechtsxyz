using Huybrechts.Core.Application;
using Huybrechts.Infra.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Huybrechts.Web.Pages.Account.Tenant
{
	public class IndexModel : PageModel
    {
		private readonly ApplicationTenantManager tenantManager;
        private readonly ILogger<IndexModel> logger;

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public List<ApplicationTenant> ApplicationTenants { get; set; } = [];
        }

        public IndexModel(
			ApplicationTenantManager tenantManager,
			ILogger<IndexModel> logger)
		{
			this.tenantManager = tenantManager;
            this.logger = logger;
        }

		public void OnGet()
        {

        }
    }
}
