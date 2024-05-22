using Huybrechts.App.Identity;
using Huybrechts.Core.Application; 
using Microsoft.AspNetCore.Mvc;

namespace Huybrechts.Web.Controllers.Application;

[Route("api/[controller]")]
[ApiController]
public class TenantController : ControllerBase
{
    private readonly ApplicationTenantManager _tenantManager;

    public TenantController(ApplicationTenantManager tenantManager)
    {
        _tenantManager = tenantManager;
    }

    // GET: api/<Controller>
    [HttpGet]
    public async Task<IEnumerable<ApplicationTenantInfo>> Get()
    {
        var list = await _tenantManager.GetTenantsAsync();
        return list;
    }

    // GET api/<Controller>/tenant1
    [HttpGet("{id}")]
    public async Task<ApplicationTenantInfo?> GetAsync(string id)
    {
        var item = await _tenantManager.GetTenantAsync(id);
        return item;
    }

    // POST api/<Controller>
    [HttpPost]
    public async Task PostAsync([FromBody] ApplicationTenantInfo tenantInfo)
    {
        await _tenantManager.AddTenantAsync((App.Identity.Entities.ApplicationTenant)tenantInfo);
    }

    // PUT api/<Controller>/tenant1
    [HttpPut("{id}")]
    public async void Put(string id, [FromBody] ApplicationTenantInfo tenantInfo)
    {
        tenantInfo.Id = id;
        await _tenantManager.UpdateTenantAsync((App.Identity.Entities.ApplicationTenant)tenantInfo);
    }

    // DELETE api/<Controller>/tenant1
    [HttpDelete("{id}")]
    public async void Delete(string id)
    {
        var item = await _tenantManager.GetTenantAsync(id);
        if (item is not null)
            await _tenantManager.DeleteTenantAsync(item);
    }
}
