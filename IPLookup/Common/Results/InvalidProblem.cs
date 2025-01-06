using System.Reflection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace IPLookup.Common.Results;

public sealed class InvalidProblem : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IContentTypeHttpResult, IValueHttpResult, IValueHttpResult<ProblemDetails>
{
    private readonly ProblemHttpResult problem;

    public InvalidProblem(string errorMessage)
    {
        problem = TypedResults.Problem
        (
            statusCode: StatusCode,
            title: "Bad Request",
            detail: errorMessage
        );
    }

    public int? StatusCode => StatusCodes.Status400BadRequest;
    public string? ContentType => problem.ContentType;
    public object? Value => problem.ProblemDetails;
    ProblemDetails? IValueHttpResult<ProblemDetails>.Value => problem.ProblemDetails;

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        builder.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status404NotFound, typeof(ProblemDetails), ["application/problem+json"]));
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await problem.ExecuteAsync(httpContext);
    }
}
