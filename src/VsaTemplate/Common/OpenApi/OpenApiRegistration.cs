using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;

namespace VsaTemplate.Common.OpenApi;

public static class OpenApiRegistration
{
#if (IncludeComments)
    /// <summary>
    /// Contrato para el frontend: describe todos los endpoints automáticamente
    /// a partir de TypedResults (sin atributos). El documento se sirve en
    /// /openapi/v1.json y también se emite en build a openapi/ (ver csproj);
    /// desde ahí Angular genera sus tipos/servicios — comando en el README.
    /// </summary>
#endif
    public static IServiceCollection AddOpenApiDocument(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
#if (IncludeComments)
            // Los records anidados se llaman igual en todos los slices (Request,
            // Response); sin esto colisionan en el documento. Se prefija con la
            // clase contenedora: CreateProduct.Request → "CreateProductRequest".
#endif
            options.CreateSchemaReferenceId = typeInfo =>
                typeInfo.Type.DeclaringType is { } owner
                    ? $"{owner.Name}{typeInfo.Type.Name}"
                    : OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);
        });

#if (IncludeComments)
    /// <summary>
    /// Expone el documento (/openapi/v1.json) y la UI de Scalar (/scalar/v1)
    /// solo en Development y Staging: en Production no queremos publicar el
    /// contrato de la API ni una UI para explorarla/probarla.
    /// </summary>
#endif
    public static WebApplication MapOpenApiDocument(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }
}
