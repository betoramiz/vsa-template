using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace VsaTemplate.Tests.Common;

#if (IncludeComments)
/// <summary>
/// Emite JWTs firmados con la misma llave de desarrollo que trae
/// appsettings.json, para ejercitar endpoints protegidos con
/// RequireAuthorization() sin necesitar un IdP real en los tests.
/// </summary>
#endif
public static class TestJwt
{
    private const string Issuer = "VsaTemplate";
    private const string Audience = "VsaTemplate";
    private const string SigningKey = "dev-only-signing-key-override-with-user-secrets-or-env-vars";

    public static string CreateToken(string? userId = null, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId ?? Guid.NewGuid().ToString()) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
