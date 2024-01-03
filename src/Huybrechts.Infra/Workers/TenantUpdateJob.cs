using Huybrechts.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Huybrechts.Infra.Workers;

public class TenantUpdateJob : IJob
{
    private readonly AdministrationContext _adminContext;
    private readonly ILogger<TenantUpdateJob> _logger;

    public TenantUpdateJob(AdministrationContext adminContext, ILogger<TenantUpdateJob> logger)
    {
        _adminContext = adminContext;
        _logger = logger;
    }

    // have a public key that is easy reference in DI configuration for example
    // group helps you with targeting specific jobs in maintenance operations, 
    // like pause all jobs in group "integration"
    public static readonly JobKey Key = new("update-tenant-job", "identity");

    public async Task Execute(IJobExecutionContext context)
    {
        // we might not ever succeed!
        if (context.RefireCount > 10)
        {
            // maybe log a warning, throw another type of error, inform the engineer on call
            return;
        }

        try
        {
            var tenantid = context.MergedJobDataMap.GetString("tenantid");

            // ... do work
            var appTenant = await _adminContext.Tenants.FirstOrDefaultAsync(q => q.Id == tenantid);
            if (appTenant == null)
            {
                // maybe log a warning,
                return;
            }

            TenantContextFactory factory = new();
            var _tenantContext = factory.BuildContext(appTenant);
            await _tenantContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            // do you want the job to refire?
            throw new JobExecutionException(msg: "", refireImmediately: true, cause: ex);
        }

        return;
    }
}
