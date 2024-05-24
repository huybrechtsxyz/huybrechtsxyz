using Huybrechts.App.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Huybrechts.Infra.Data;

public class ApplicationContext: IdentityDbContext, IApplicationContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
    {

    }
}
