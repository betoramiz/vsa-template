using FluentValidation;

namespace VsaTemplate.Common.Endpoints;

#if (IncludeComments)
/// <summary>
/// Cross-cutting de validación como endpoint filter (la alternativa de Minimal
/// APIs a los "behaviors" de MediatR). Se aplica por endpoint con
/// <c>.WithValidation&lt;TRequest&gt;()</c>: resuelve el IValidator del request,
/// y si falla corta con 400 ValidationProblem antes de llegar al handler — el
/// handler solo recibe requests válidos y no carga con código de validación.
/// </summary>
#endif
public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder)
        where TRequest : class =>
        builder
            .AddEndpointFilter(async (context, next) =>
            {
#if (IncludeComments)
                // GetRequiredService a propósito: si marcaste el endpoint con
                // WithValidation y no existe el validator, es un bug de
                // configuración y debe explotar, no pasar en silencio.
#endif
                var validator = context.HttpContext.RequestServices
                    .GetRequiredService<IValidator<TRequest>>();

                var request = context.Arguments.OfType<TRequest>().First();
                var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

                return result.IsValid
                    ? await next(context)
                    : TypedResults.ValidationProblem(result.ToDictionary());
            })
#if (IncludeComments)
            // Documenta el 400 en OpenAPI: el handler ya no lo declara porque
            // nunca lo produce — lo produce este filtro.
#endif
            .ProducesValidationProblem();
}
