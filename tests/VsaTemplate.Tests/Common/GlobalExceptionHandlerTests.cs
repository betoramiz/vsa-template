using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VsaTemplate.Common.ErrorHandling;

namespace VsaTemplate.Tests.Common;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task Writes_a_500_problem_details_response_for_an_unhandled_exception()
    {
        var problemDetailsService = new Mock<IProblemDetailsService>();
        problemDetailsService
            .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
            .ReturnsAsync(true);

        var handler = new GlobalExceptionHandler(problemDetailsService.Object, NullLogger<GlobalExceptionHandler>.Instance);
        var httpContext = new DefaultHttpContext();

        var handled = await handler.TryHandleAsync(httpContext, new InvalidOperationException("boom"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        problemDetailsService.Verify(s => s.TryWriteAsync(It.Is<ProblemDetailsContext>(context =>
            context.ProblemDetails.Status == StatusCodes.Status500InternalServerError &&
            context.Exception!.Message == "boom")), Times.Once);
    }
}
