namespace ExpenseTracker.Api.Endpoints;

internal interface IEndpointModule
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
