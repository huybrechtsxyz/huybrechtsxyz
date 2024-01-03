﻿using Huybrechts.Infra.Data;
using Huybrechts.Infra.Entities;
using Microsoft.Extensions.Logging;

namespace Huybrechts.Infra.Services;

public interface ITenantManager
{

}

public class TenantManager : ITenantManager
{
    private readonly ApplicationUserManager _userManager;
    private readonly AdministrationContext _context;
    private readonly ILogger _logger;

    public TenantManager(ApplicationUserManager userManager, AdministrationContext context, ILogger logger)
    {
        _logger = logger;
        _userManager = userManager;
        _context = context;
    }

    public List<ApplicationTenant> GetTenants()
    {


        return new List<ApplicationTenant> { new() { Id = "jef" } };
    }
}
