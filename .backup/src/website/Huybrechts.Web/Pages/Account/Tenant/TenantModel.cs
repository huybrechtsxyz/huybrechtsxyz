using Huybrechts.Core.Application;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.Web.Pages.Account.Tenant;

public class TenantModel
{
    [Key]
    [Required]
    [RegularExpression("^[a-z0-9]+$")]
    [StringLength(24, MinimumLength = 2)]
    [Display(Name = nameof(Id), ShortName = nameof(Id) + "_s", Description = nameof(Id) + "_d", Prompt = nameof(Id) + "_p", ResourceType = typeof(TenantViewModels))]
    public string Id { get; set; } = string.Empty;

    [Display(Name = nameof(State), ShortName = nameof(State) + "_s", Description = nameof(State) + "_d", Prompt = nameof(State) + "_p", ResourceType = typeof(TenantViewModels))]
    public ApplicationTenantState State { get; set; }

    [Required]
    [StringLength(256, MinimumLength = 2)]
    [Display(Name = nameof(Name), ShortName = nameof(Name) + "_s", Description = nameof(Name) + "_d", Prompt = nameof(Name) + "_p", ResourceType = typeof(TenantViewModels))]
    public string Name { get; set; } = string.Empty;

    [StringLength(512)]
    [Display(Name = nameof(Description), ShortName = nameof(Description) + "_s", Description = nameof(Description) + "_d", Prompt = nameof(Description) + "_p", ResourceType = typeof(TenantViewModels))]
    public string? Description { get; set; }

    [Display(Name = nameof(Remark), ShortName = nameof(Remark) + "_s", Description = nameof(Remark) + "_d", Prompt = nameof(Remark) + "_p", ResourceType = typeof(TenantViewModels))]
    public string? Remark { get; set; }

    public byte[]? Picture { get; set; }
}