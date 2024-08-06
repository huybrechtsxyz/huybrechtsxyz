using AutoMapper;
using FluentValidation;
using Huybrechts.Core.Platform;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Huybrechts.App.Features.Platform;

public static class PlatformInfoFlow
{
    public record Model
    {
        public Ulid Id { get; init; }

        [Display(Name = nameof(Name), ResourceType = typeof(PlatformLocalization))]
        public string Name { get; set; } = string.Empty;

        [Display(Name = nameof(Description), ResourceType = typeof(PlatformLocalization))]
        public string? Description { get; set; }

        [Display(Name = nameof(Remark), ResourceType = typeof(PlatformLocalization))]
        public string? Remark { get; set; }
    }

    //
    // LIST
    //

    public sealed record ListModel : Model
    {
    }

    public sealed class ListMapping : Profile
    {
        public ListMapping() => CreateProjection<PlatformInfo, ListModel>();
    }

    public sealed class ListQuery : EntityListFlow.Query, IRequest<ListResult> 
    {
    }

    public sealed class ListValidator : AbstractValidator<ListQuery> { public ListValidator() { } }

    public sealed class ListResult : EntityListFlow.Result<ListModel>
    {
    }

    public sealed class ListHandler : 
        EntityListFlow.Handler<PlatformInfo, ListModel>,
        IRequestHandler<ListQuery, ListResult>
    {
        public ListHandler(PlatformContext dbcontext, IConfigurationProvider configuration)
            : base(dbcontext, configuration) 
        {
            base.SetWhere = "Name.Contains(@0) || Description.Contains(@0)";
            base.OrderBy = "Name";
        }

        public async Task<ListResult> Handle(ListQuery request, CancellationToken cancellationToken)
        {
            return (ListResult)await base.Handle(request, cancellationToken);
        }
    }
}
