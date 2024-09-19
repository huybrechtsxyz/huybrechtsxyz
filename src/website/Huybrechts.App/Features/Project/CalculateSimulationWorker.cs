using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Huybrechts.App.Data;
using MediatR;

namespace Huybrechts.App.Features.Project;

public class CalculateSimulationWorker
{
    private readonly IMediator _mediator;
    private readonly ApplicationContext _applicationContext;
    private readonly IMultiTenantContextSetter _contextSetter;

    public CalculateSimulationWorker(
        IMediator mediator,
        ApplicationContext applicationContext,
        IMultiTenantContextSetter contextSetter)
    {
        _mediator = mediator;
        _contextSetter = contextSetter;
        _applicationContext = applicationContext;
    }

    public async Task StartAsync(string tenantId, Ulid projectSimulationId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentOutOfRangeException.ThrowIfEqual(projectSimulationId, Ulid.Empty);

        try
        {
            var tenant = await _applicationContext.ApplicationTenants.FindAsync([tenantId], cancellationToken: token) ??
                throw new Exception($"Tenant with ID {tenantId} not found.");
            
            _contextSetter.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = tenant.ToTenantInfo() };
            
            // Set IsCalculating

            // CALCULATE

            // Reset IsCalculation
            
        }
        catch(Exception ex)
        {
            var message = ex.Message;
        }
    }
}
