using FluentValidation;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Huybrechts.App.Features.Platform;

public sealed record GetPlatformListQuery : IRequest<GetPlatformListResponse>
{

}

public sealed record GetPlatformListResponse : IRequest
{

}

public sealed class GetPlatformListValidator : AbstractValidator<Query>
{
    public GetPlatformListValidator()
    {
        
    }
}

public sealed class GetPlatformListHandler : IRequestHandler<GetPlatformListQuery, GetPlatformListResponse>
{
    private readonly PlatformContext _dbcontext;

    public GetPlatformListHandler(PlatformContext context)
    {
        _dbcontext = context;
    }

    public Task<GetPlatformListResponse> Handle(GetPlatformListQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

