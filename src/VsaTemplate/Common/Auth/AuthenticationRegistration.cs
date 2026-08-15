using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace VsaTemplate.Common.Auth;

#if (IncludeComments)
/// <summary>
/// Valida JWTs emitidos por un IdP externo (Auth0, Entra ID, Keycloak, un
/// servicio de identidad propio, etc.). Este template no emite tokens: no
/// hay tabla de usuarios ni endpoint de login, solo la validación del lado
/// de la API. Configura la sección "Jwt" (Issuer, Audience, SigningKey) en
/// appsettings — en producción, sobreescribe SigningKey vía variables de
/// entorno o un secret store, nunca la dejes en el appsettings.json commiteado.
/// </summary>
#endif
public static class AuthenticationRegistration
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Falta la sección de configuración \"{JwtOptions.SectionName}\".");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                };
            });

        services.AddAuthorization(options => options.ConfigureSlicePolicies());
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
