using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Platform;
using Huybrechts.App.Features.Setup;
using Huybrechts.Core.Platform;
using MediatR;

namespace Huybrechts.App.Application;

public class CreateTenantWorker
{
    private readonly IMediator _mediator;
    private readonly ApplicationUserManager _userManager;
    private readonly ApplicationContext _applicationContext;
    private readonly IMultiTenantContextSetter _contextSetter;

    public CreateTenantWorker(
        IMediator mediator,
        ApplicationUserManager userManager,
        ApplicationContext applicationContext,
        IMultiTenantContextSetter contextSetter)
    {
        _mediator = mediator;
        _userManager = userManager;
        _applicationContext = applicationContext;
        _contextSetter = contextSetter;
    }

    public async Task StartAsync(string userId, string tenantId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(tenantId);

        try
        {
            _ = await _userManager.FindByIdAsync(userId) ??
                throw new InvalidOperationException($"Unable to find user with ID {userId}.");

            var tenant = await _applicationContext.ApplicationTenants.FindAsync([tenantId], cancellationToken: token) ??
                throw new Exception($"Tenant with ID {tenantId} not found.");
            
            _contextSetter.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = tenant.ToTenantInfo() };
            
            // SETUP
            await CreateSetupStates(token);
            await CreateSetupUnits(token);
            await CreateSetupLanguages(token);
            await CreateSetupCurrencies(token);
            await CreateSetupCountries(token);

            // PLATFORM
            await CreatePlatform(token);
        }
        catch(Exception ex)
        {
            var message = ex.Message;
        }
    }

    //
    // SETUP
    //

    private async Task CreateSetupStates(CancellationToken token = default)
    {
        SetupStateFlow.ImportQuery query = new() { };
        var request = await _mediator.Send(query, token);
        if (request.IsFailed)
            return;

        var entities = request.Value.Results.ToList();

        SetupStateFlow.ImportCommand command = new() { Items = entities };
        _ = await _mediator.Send(command, token);
    }

    private async Task CreateSetupUnits(CancellationToken token = default)
    {
        SetupUnitFlow.ImportQuery query = new() { };
        var request = await _mediator.Send(query, token);
        if (request.IsFailed)
            return;

        var entities = request.Value.Results.ToList();

        SetupUnitFlow.ImportCommand command = new() { Items = entities };
        _ = await _mediator.Send(command, token);
    }

    private async Task CreateSetupLanguages(CancellationToken token = default)
    {
        List<string> defaults = ["EN", "NL"];

        SetupLanguageFlow.ImportQuery query = new() { };
        var request = await _mediator.Send(query, token);
        if (request.IsFailed)
            return;

        var entities = request.Value.Results.Where(q => defaults.Contains(q.Code)).ToList();

        SetupLanguageFlow.ImportCommand command = new() { Items = entities };
        _ = await _mediator.Send(command, token);
    }

    private async Task CreateSetupCurrencies(CancellationToken token = default)
    {
        List<string> defaults = ["EUR", "USD"];

        SetupCurrencyFlow.ImportQuery query = new() { };
        var request = await _mediator.Send(query, token);
        if (request.IsFailed)
            return;

        var entities = request.Value.Results.Where(q => defaults.Contains(q.Code)).ToList();

        SetupCurrencyFlow.ImportCommand command = new() { Items = entities };
        _ = await _mediator.Send(command, token);
    }

    private async Task CreateSetupCountries(CancellationToken token = default)
    {
        List<string> defaults = ["BE", "US"];

        SetupCountryFlow.ImportQuery query = new() { };
        var request = await _mediator.Send(query, token);
        if (request.IsFailed)
            return;

        var entities = request.Value.Results.Where(q => defaults.Contains(q.Code)).ToList();

        SetupCountryFlow.ImportCommand command = new() { Items = entities };
        _ = await _mediator.Send(command, token);
    }

    //
    // PLATFORM
    //

    private async Task CreatePlatform(CancellationToken token = default)
    {
        await CreatePlatformAzure(token);
        await CreatePlatformOnPremise(token);
    }

    private async Task CreatePlatformAzure(CancellationToken token = default)
    {
        PlatformInfoFlow.CreateCommand platformCommand = new()
        {
            Id = Ulid.NewUlid(),
            Name = "Azure",
            Description = "Microsoft Azure is a cloud computing platform that offers a wide range of services for building, deploying, and managing applications through Microsoft-managed data centers.",
            Provider = PlatformProvider.Azure,
            Remark = null,
            SkipIfExists = true
        };
        var response = await _mediator.Send(platformCommand, token);
        var platformInfoId = response.Value;

        // REGION

        PlatformRegionFlow.ImportQuery regionQuery = new()
        {
            PlatformInfoId = platformInfoId,
            SearchText = "europe",
            Page = 1
        };
        var regionRequest = await _mediator.Send(regionQuery, token);
        if (regionRequest.IsFailed) return;
        var regions = regionRequest.Value.Results.ToList();
        PlatformRegionFlow.ImportCommand regionCommand = new() { PlatformInfoId = platformInfoId, Items = regions };
        _ = await _mediator.Send(regionCommand, token);

        // SERVICE

        PlatformServiceFlow.ImportQuery serviceQuery = new()
        {
            PlatformInfoId = platformInfoId,
            SearchText = "API Management",
            Page = 1
        };
        var serviceRequest = await _mediator.Send(serviceQuery, token);
        if (serviceRequest.IsFailed) return;
        var services = serviceRequest.Value.Results.ToList();
        PlatformServiceFlow.ImportCommand serviceCommand = new() { PlatformInfoId = platformInfoId, Items = services };
        _ = await _mediator.Send(serviceCommand, token);

        // UNITS

        PlatformDefaultUnitFlow.ImportQuery unitQuery = new() { PlatformInfoId = platformInfoId };
        var unitRequest = await _mediator.Send(unitQuery, token);
        if (unitRequest.IsFailed) return;
        var units = unitRequest.Value.Results.ToList();
        PlatformDefaultUnitFlow.ImportCommand unitCommand = new() { PlatformInfoId = platformInfoId, Items = units };
        _ = await _mediator.Send(unitCommand, token);

        // PRODUCTS

        PlatformProductFlow.ImportQuery productQuery = new()
        {
            PlatformInfoId = platformInfoId,
            SearchText = "API Management",
            Page = 1
        };
        var productRequest = await _mediator.Send(productQuery, token);
        if (productRequest.IsFailed) return;
        var products = productRequest.Value.Results.ToList();
        PlatformProductFlow.ImportCommand productCommand = new() { PlatformInfoId = platformInfoId, Items = products };
        _ = await _mediator.Send(productCommand, token);

        // RATES

        PlatformRegionFlow.ListQuery regionListQuery = new()
        {
            PlatformInfoId = platformInfoId,
            SearchText = "westeurope",
            Page = 1
        };
        var regionListRequest = await _mediator.Send(regionListQuery, token);
        var regionInfo = regionListRequest.Value.Results.First();

        PlatformServiceFlow.ListQuery serviceListQuery = new()
        {
            PlatformInfoId = platformInfoId,
            SearchText = "API Management",
            Page = 1
        };
        var serviceListRequest = await _mediator.Send(serviceListQuery, token);
        var serviceInfo = serviceListRequest.Value.Results.First();

        PlatformProductFlow.ListQuery productListQuery = new()
        {
            PlatformInfoId = platformInfoId,
            SearchText = "API Management",
            Page = 1
        };
        var productListRequest = await _mediator.Send(productListQuery, token);
        var productInfo = productListRequest.Value.Results.First();

        List<string> defaultRates = ["Basic v2"];
        PlatformRateFlow.ImportQuery rateQuery = new()
        {
            PlatformRegionId = regionInfo.Id,
            PlatformServiceId = serviceInfo.Id,
            PlatformProductId = productInfo.Id,
            CurrencyCode = "EUR",
            SearchText = "Basic v2"
        };
        var rateRequest = await _mediator.Send(rateQuery, token);
        if (rateRequest.IsFailed) return;
        var rates = rateRequest.Value.Results.Where(q => defaultRates.Contains(q.SkuName)).ToList();
        PlatformRateFlow.ImportCommand rateCommand = new()
        {
            PlatformProductId = productInfo.Id,
            PlatformRegionId = regionInfo.Id,
            PlatformServiceId = serviceInfo.Id,
            Items = rates
        };
        _ = await _mediator.Send(rateCommand, token);
    }

    private async Task CreatePlatformOnPremise(CancellationToken token = default)
    {
        PlatformInfoFlow.CreateCommand platformCommand = new()
        {
            Id = Ulid.NewUlid(),
            Name = "On Premise",
            Description = "On-premise refers to IT infrastructure and software that are hosted and operated within a company's physical premises rather than in the cloud.",
            Provider = PlatformProvider.None,
            Remark = null,
            SkipIfExists = true
        };
        var response = await _mediator.Send(platformCommand, token);
        var platformInfoId = response.Value;

        // REGION

        PlatformRegionFlow.CreateCommand regionCommand = new()
        {
            Id = Ulid.NewUlid(),
            PlatformInfoId = platformInfoId,
            Name = "HQ",
            Label = "Headquarters",
            Description = "On Premise Datacenter",
            Remark = null
        };
        _ = await _mediator.Send(regionCommand, token);

        // SERVICE

        PlatformServiceFlow.CreateCommand serviceCommand = new()
        {
            Id = Ulid.NewUlid(),
            PlatformInfoId = platformInfoId,
            Name = "VM",
            Label = "Virtual Machine",
            Description = "A virtual machine (VM) is a software-based emulation of a physical computer, allowing multiple operating systems to run on a single physical machine.",
            Category = "Infrastructure",
            Remark = null
        };
        _ = await _mediator.Send(serviceCommand, token);

        // UNITS

        SetupUnitFlow.ListQuery unitListQuery = new()
        {
            SearchText = "DEFAULT"
        };
        var unitListResponse = await _mediator.Send(unitListQuery, token);
        var defaultUnit = unitListResponse.Value.Results.First();

        PlatformDefaultUnitFlow.CreateCommand unitCommand = new()
        {
            Id = Ulid.NewUlid(),
            PlatformInfoId = platformInfoId,
            SetupUnitId = defaultUnit.Id,
            Description = defaultUnit.Description ?? string.Empty,
            UnitOfMeasure = defaultUnit.Name,
            UnitFactor = defaultUnit.Factor,
            DefaultValue = 1
        };
        _ = await _mediator.Send(unitCommand, token);

        // PRODUCTS

        PlatformProductFlow.CreateCommand productCommand = new()
        {
            Id = Ulid.NewUlid(),
            PlatformInfoId = platformInfoId,
            Name = "VM",
            Label = "Virtual Machine",
            Description = "A virtual machine (VM) is a software-based emulation of a physical computer, allowing multiple operating systems to run on a single physical machine.",
            Category = "Infrastructure",
            CostBasedOn = "VM Size, Runtime"
        };
        _ = await _mediator.Send(productCommand, token);

        // RATES

        PlatformRegionFlow.ListQuery regionListQuery = new()
        {
            PlatformInfoId = platformInfoId,
            SearchText = "HQ",
            Page = 1
        };
        var regionListRequest = await _mediator.Send(regionListQuery, token);
        var regionInfo = regionListRequest.Value.Results.First();

        PlatformServiceFlow.ListQuery serviceListQuery = new()
        {
            PlatformInfoId = platformInfoId,
            SearchText = "VM",
            Page = 1
        };
        var serviceListRequest = await _mediator.Send(serviceListQuery, token);
        var serviceInfo = serviceListRequest.Value.Results.First();

        PlatformProductFlow.ListQuery productListQuery = new()
        {
            PlatformInfoId = platformInfoId,
            SearchText = "VM",
            Page = 1
        };
        var productListRequest = await _mediator.Send(productListQuery, token);
        var productInfo = productListRequest.Value.Results.First();

        PlatformRateFlow.CreateCommand rateCommand = new()
        {
            Id = Ulid.NewUlid(),
            PlatformInfoId = platformInfoId,
            PlatformProductId = productInfo.Id,
            PlatformProductLabel = productInfo.Label,
            PlatformRegionId = regionInfo.Id,
            PlatformRegionLabel = regionInfo.Label,
            PlatformServiceId = serviceInfo.Id,
            PlatformServiceLabel = serviceInfo.Label,
            CurrencyCode = "EUR",
            MeterName = "Runtime",
            MinimumUnits = 0,
            ProductName = productInfo.Name,
            RateType = "Consumption",
            RetailPrice = 10,
            UnitPrice = 10,
            ServiceFamily = serviceInfo.Category ?? productCommand.Category,
            ServiceName = serviceInfo.Name,
            SkuName = "Machine",
            UnitOfMeasure = "Hour",
            ValidFrom = DateTime.Today,
            IsPrimaryRegion = true
        };
        _ = await _mediator.Send(rateCommand, token);
    }
}
