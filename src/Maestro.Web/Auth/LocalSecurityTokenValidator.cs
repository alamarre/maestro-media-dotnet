using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Maestro;

internal class LocalSecurityTokenValidator : JwtSecurityTokenHandler
{
    private readonly byte[]? secretKeyBytes;

    public LocalSecurityTokenValidator()
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (!string.IsNullOrEmpty(secretKey))
        {
            secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        }
    }

    public override ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters,
        out SecurityToken validatedToken)
    {
        if (secretKeyBytes == null)
        {
            return base.ValidateToken(token, validationParameters, out validatedToken);
        }

        SecurityKey securityKey = new SymmetricSecurityKey(secretKeyBytes);

        validationParameters.ValidateIssuer = false;
        validationParameters.ValidateAudience = false;
        validationParameters.IssuerSigningKey = securityKey;
        return base.ValidateToken(token, validationParameters, out validatedToken);
    }

    public string CreateToken(Guid userId, Dictionary<string, string>? additionalClaims = null,
        string? secretKey = null)
    {
        SecurityKey securityKey = new SymmetricSecurityKey(secretKeyBytes);
        if (secretKey != null)
        {
            securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }),
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            Expires = DateTime.UtcNow.AddYears(1),
        };

        if (additionalClaims != null)
        {
            foreach (var claim in additionalClaims)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(claim.Key, claim.Value));
            }
        }

        JwtSecurityToken token = base.CreateJwtSecurityToken(tokenDescriptor);
        return WriteToken(token);
    }
}
