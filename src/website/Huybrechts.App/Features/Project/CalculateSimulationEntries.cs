using FluentResults;
using Huybrechts.App.Data;
using Huybrechts.App.Features.Project.ProjectSimulationFlow;
using Huybrechts.Core.Platform;
using Huybrechts.Core.Project;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace Huybrechts.App.Features.Project;

internal class CalculateSimulationEntries
{
    private readonly FeatureContext _dbcontext;
    private readonly Jace.CalculationEngine _calculationEngine;

    private ProjectInfo _project = null!;
    private ProjectSimulation _simulation = null!;
    private List<ProjectScenario> _scenarioList = null!;
    private List<ProjectDesign> _designList = null!;
    private List<ProjectComponent> _componentList = null!;
    private List<ProjectQuantity> _quantityList = null!;

    private List<ProjectSimulationEntry> _entryList = null!;

    public CalculateSimulationEntries(FeatureContext dbcontext)
    {
        _dbcontext = dbcontext;
        

        // Preparing the calculation engine
        _calculationEngine = new();
        _calculationEngine.AddFunction("roundup", ((Func<double, double>)((a) => (double)decimal.Ceiling((decimal)a))));
        _calculationEngine.AddFunction("rounddown", ((Func<double, double>)((a) => (double)decimal.Floor((decimal)a))));
    }

    /// <summary>
    /// Starts the calculation
    /// </summary>
    public async Task<Result<List<ProjectSimulationEntry>>> StartAsync(Ulid simluationId, CancellationToken token)
    {
        try
        {
            _entryList = [];
            Result result = new();
            if (!await GetSimulation(simluationId, token)) return ProjectSimulationHelper.EntityNotFound(simluationId);
            if (!await GetProjectInfo(_simulation.ProjectInfoId, token)) return ProjectSimulationHelper.ProjectNotFound(simluationId);

            await LoopScenarioAndDesigns(token);

            return _entryList;
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Get the simulation
    /// </summary>
    private async Task<bool> GetSimulation(Ulid id, CancellationToken token)
    {
        var entity = await _dbcontext.Set<ProjectSimulation>().FirstOrDefaultAsync(f => f.Id == id, token);
        if (entity == null)
            return false;
        _simulation = entity;
        return true;
    }

    /// <summary>
    /// Get the project related data
    /// </summary>
    private async Task<bool> GetProjectInfo(Ulid id, CancellationToken token)
    {
        var entity = await _dbcontext.Set<ProjectInfo>().FirstOrDefaultAsync(f => f.Id == id, token);
        if (entity == null)
            return false;

        _project = entity;

        _scenarioList = await _dbcontext.Set<ProjectScenario>()
               .Where(q => q.ProjectInfoId == _project.Id)
               .Include(i => i.Units)
               .ThenInclude(i => i.SetupUnit)
               .OrderBy(o => o.Name)
               .ToListAsync(token);

        _designList = await _dbcontext.Set<ProjectDesign>()
            .Where(q => q.ProjectInfoId == _project.Id)
            .OrderBy(o => o.Name)
            .ToListAsync(token);

        _componentList = await _dbcontext.Set<ProjectComponent>()
            .Where(q => q.ProjectInfoId == _project.Id)
            .Include(i => i.ProjectComponentUnits)
            .ThenInclude(i => i.SetupUnit)
            .OrderBy(o => o.ProjectDesignId).ThenBy(o => o.Sequence).ThenBy(o => o.Name)
            .ToListAsync(token);

        // Take into account that there are not quantities when looping the bill of quantities
        _quantityList = [];
        _quantityList.Add(new ProjectQuantity());
        var list = await _dbcontext.Set<ProjectQuantity>()
            .Where(q => q.ProjectInfoId == _project.Id)
            .OrderBy(o => o.Name)
            .ToListAsync(token);
        if (list.Count > 0)
            _quantityList.AddRange(list);

        return true;
    }

    /// <summary>
    /// Make a loop over the project data
    /// </summary>
    private async Task LoopScenarioAndDesigns(CancellationToken token)
    {
        foreach (var scenarioInfo in _scenarioList)
        {
            foreach (var designInfo in _designList)
            {
                foreach (var componentInfo in _componentList.Where(q => q.ProjectDesignId == designInfo.Id))
                {
                    await ProcessComponent(scenarioInfo, designInfo, componentInfo, token);
                }
            }
        }
    }

    /// <summary>
    /// Process the component and the units
    /// </summary>
    private async Task ProcessComponent(ProjectScenario scenarioInfo, ProjectDesign designInfo, ProjectComponent componentInfo, CancellationToken token)
    {
        Dictionary<string, double> variables = [];
        ProcessVariables(scenarioInfo, componentInfo, variables);

        foreach(var quantityInfo in  _quantityList)
        {
            if (componentInfo.SourceType == SourceType.None)
                ProcessSourceTypeNone(scenarioInfo, designInfo, componentInfo, quantityInfo, variables);

            else if (componentInfo.SourceType == SourceType.Platform)
                await ProcessSourceTypePlatform(scenarioInfo, designInfo, componentInfo, quantityInfo, variables, token);
        }
    }

    /// <summary>
    /// Calculate all the variable values
    /// </summary>
    private void ProcessVariables(ProjectScenario scenarioInfo, ProjectComponent componentInfo, Dictionary<string, double> variables)
    {
        foreach (var metric in scenarioInfo.Units.OrderBy(o => o.Sequence).ThenBy(o => o.Variable))
        {
            ProcessVariablesFor(variables, metric.SetupUnit?.Name, metric.Variable, 0, metric.Expression);
        }

        foreach (var measure in componentInfo.ProjectComponentUnits.OrderBy(o => o.Sequence).ThenBy(o => o.Variable))
        {
            ProcessVariablesFor(variables, measure.SetupUnit?.Name, measure.Variable, (double)measure.Quantity, measure.Expression);
        }
    }

    /// <summary>
    /// Calculate the value of a variable
    /// </summary>
    private void ProcessVariablesFor(Dictionary<string, double> variables, string? unitname, string? variable, double quantity, string? expression)
    {
        if (string.IsNullOrEmpty(variable))
            variable = unitname;

        if (string.IsNullOrEmpty(variable))
            return;

        double value = quantity;
        if (!string.IsNullOrEmpty(expression))
            value = _calculationEngine.Calculate(expression.ToLower(), variables);

        if (!variables.ContainsKey(variable.ToLower()))
            variables.Add(variable.ToLower(), value);
        else
            variables[variable.ToLower()] = value;
    }

    /// <summary>
    /// Process the component based on the bill of quantities
    /// </summary>
    private void ProcessSourceTypeNone(
        ProjectScenario scenarioInfo,
        ProjectDesign designInfo,
        ProjectComponent componentInfo,
        ProjectQuantity quantityInfo,
        Dictionary<string, double> variables)
    {
        foreach (var item in componentInfo.ProjectComponentUnits.Where(q => q.ProjectQuantityId == null))
            item.ProjectQuantityId = Ulid.Empty;

        foreach (var componentUnit in componentInfo.ProjectComponentUnits
            .Where(q =>
                (q.ProjectQuantityId.HasValue && q.ProjectQuantityId == quantityInfo.Id)
                || (!q.ProjectQuantityId.HasValue && q.ProjectQuantityId == Ulid.Empty)
            ))
        {
            // Only make entries when there is a setupunit involved
            if (componentUnit.SetupUnitId is null || componentUnit.SetupUnitId == Ulid.Empty || componentUnit.SetupUnit is null)
                continue;

            ProjectSimulationEntry entity = new();
            SetSimulationEntry(entity, _project, _simulation);
            SetSimulationEntry(entity, scenarioInfo, designInfo, componentInfo, quantityInfo);

            if (string.IsNullOrEmpty(componentUnit.Variable))
                componentUnit.Variable = componentUnit.SetupUnit.Name;

            CalculateEntry(entity, (decimal)variables[componentUnit.Variable.ToLower().Trim()]);
            _entryList.Add(entity);
        }
    }

    private async Task ProcessSourceTypePlatform(
        ProjectScenario scenarioInfo, 
        ProjectDesign designInfo,
        ProjectComponent componentInfo,
        ProjectQuantity quantityInfo,
        Dictionary<string, double> variables,
        CancellationToken token)
    {
        PlatformInfo? platformInfo = await _dbcontext.Set<PlatformInfo>().FirstOrDefaultAsync(e => e.Id == componentInfo.PlatformInfoId, token);
        if (platformInfo is null)
            return;

        PlatformProduct? productInfo = await _dbcontext.Set<PlatformProduct>().FirstAsync(e => e.Id == componentInfo.PlatformProductId, token);
        if (productInfo is null)
            return;

        List<PlatformRegion> regionList = await _dbcontext.Set<PlatformRegion>().Where(e => e.PlatformInfoId == platformInfo.Id).ToListAsync(token);
        List<PlatformService> serviceList = await _dbcontext.Set<PlatformService>().Where(e => e.PlatformInfoId == platformInfo.Id).ToListAsync(token);

        List<PlatformRate> rates = await _dbcontext.Set<PlatformRate>()
            .Where(e => e.PlatformProductId == productInfo.Id)
            .Include(e => e.RateUnits)
            .OrderBy(o => o.ServiceName)
            .ThenBy(o => o.ProductName)
            .ThenBy(o => o.SkuName)
            .ThenBy(o => o.MeterName)
            .ThenBy(o => o.RateType)
            .ThenBy(o => o.CurrencyCode)
            .ThenBy(o => o.ValidFrom)
            .ToListAsync(token);

        for (int idex = 0; idex < rates.Count; ++idex)
        {
            PlatformRate rateInfo = rates.ElementAt(idex);
            PlatformRate? ratePeek = null;
            if (idex + 1 < rates.Count)
                ratePeek = rates.ElementAt(idex + 1);

            var regionInfo = regionList.First(e => e.Id == rateInfo.PlatformRegionId);
            var serviceInfo = serviceList.First(e => e.Id == rateInfo.PlatformServiceId);

            ProjectSimulationEntry entity = new();
            SetSimulationEntry(entity, _project, _simulation);
            SetSimulationEntry(entity, scenarioInfo, designInfo, componentInfo, quantityInfo);
            SetSimulationEntry(entity, platformInfo, productInfo, regionInfo, serviceInfo, rateInfo);

            decimal rateQuantity = 0;

            if (rateInfo.RateUnits is not null && rateInfo.RateUnits.Count > 0)
            {
                var rateUnits = rateInfo.RateUnits.OrderBy(o => o.SetupUnit.Name);

                foreach (var rateUnit in rateInfo.RateUnits)
                {
                    if (rateUnit.UnitFactor != 0)
                        rateQuantity *= rateUnit.UnitFactor;

                    var appliedUnits = componentInfo.ProjectComponentUnits.Where(q => q.SetupUnitId == rateUnit.SetupUnitId).ToList();
                    if (appliedUnits.Count > 0)
                    {
                        foreach(var unit in appliedUnits)
                        {
                            if (!string.IsNullOrEmpty(unit.Expression))
                            {
                                if (string.IsNullOrEmpty(unit.Variable) && unit.SetupUnit is not null)
                                    unit.Variable = unit.SetupUnit.Name.ToLower().Trim();
                                if (string.IsNullOrEmpty(unit.Variable))
                                    continue;
                                rateQuantity *= (decimal)variables[unit.Variable];
                            }
                            else
                                rateQuantity *= unit.Quantity;
                        }
                    }
                    else
                        rateQuantity *= rateUnit.DefaultValue;
                }
            }
            else
            {
                // NO QUANTITY CAN BE CALCULATED : NO UNIT / QTY TO DERIVE
                rateQuantity = 1;
            }

            if (rateInfo.MinimumUnits > 0)
                rateQuantity -= rateInfo.MinimumUnits;
            if (entity.Quantity < 0)
                rateQuantity = 0;

            if (ratePeek is not null && ratePeek.MinimumUnits != 0 && entity.Quantity > ratePeek.MinimumUnits)
                rateQuantity = ratePeek.MinimumUnits;

            CalculateEntry(entity, rateQuantity);
            _entryList.Add(entity);
        }
    }

    private static void SetSimulationEntry(
        ProjectSimulationEntry entity, 
        ProjectInfo projectInfo,
        ProjectSimulation simulationInfo)
    {
        entity.Id = Ulid.NewUlid();
        entity.TenantId = simulationInfo.TenantId;
        entity.CreatedDT = DateTime.UtcNow;
        entity.ProjectInfo = projectInfo;
        entity.ProjectSimulation = simulationInfo;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    private static void SetSimulationEntry(
        ProjectSimulationEntry entity,
        ProjectScenario scenarioInfo,
        ProjectDesign designInfo,
        ProjectComponent componentInfo,
        ProjectQuantity? quantityInfo)
    {
        entity.ProjectScenario = scenarioInfo;
        entity.ProjectDesign = designInfo;
        entity.ProjectComponent = componentInfo;
        //entity.ProjectQuantity = quantityInfo;

        entity.OwnershipPercentage = componentInfo.OwnershipPercentage;
    }

    private static void SetSimulationEntry(
        ProjectSimulationEntry entity,
        PlatformInfo platformInfo,
        PlatformProduct platformProduct,
        PlatformRegion platformRegion,
        PlatformService platformService,
        PlatformRate platformRate
        )
    {
        entity.PlatformInfo = platformInfo;
        entity.PlatformProduct = platformProduct;
        entity.PlatformRegion = platformRegion;
        entity.PlatformService = platformService;
        entity.PlatformRate = platformRate;

        entity.CurrencyCode = platformRate.CurrencyCode;
        entity.UnitOfMeasure = platformRate.UnitOfMeasure;
        entity.RetailPrice = platformRate.RetailPrice;
        entity.UnitPrice = platformRate.UnitPrice;
    }

    private static void CalculateEntry(ProjectSimulationEntry entity, decimal quantity)
    {
        entity.Quantity = quantity;
        entity.SalesAmount = decimal.Round(entity.Quantity * entity.SalesPrice, 4);
        entity.RetailAmount = decimal.Round(entity.Quantity * entity.RetailPrice, 4);
        entity.UnitAmount = decimal.Round(entity.Quantity * entity.UnitPrice, 4);
        entity.OwnSalesAmount = decimal.Round(entity.SalesAmount * (entity.OwnershipPercentage / 100), 4);
        entity.OwnRetailAmount = decimal.Round(entity.RetailAmount * (entity.OwnershipPercentage / 100), 4);
        entity.OwnUnitAmount = decimal.Round(entity.UnitAmount * (entity.OwnershipPercentage / 100), 4);
    }
}
