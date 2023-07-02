using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Maestro;

internal class LocalSecurityTokenValidator : JwtSecurityTokenHandler
{
    private readonly byte[]? secretKeyBytes;
   public LocalSecurityTokenValidator() {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET");
        if(!string.IsNullOrEmpty(secretKey)) {
            secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
        }
   }

    public override ClaimsPrincipal ValidateToken( string token, TokenValidationParameters validationParameters, out SecurityToken validatedToken ) {
        if(secretKeyBytes == null) {
            return base.ValidateToken( token, validationParameters, out validatedToken );
        }
        SecurityKey securityKey = new SymmetricSecurityKey(secretKeyBytes);
        
        validationParameters.ValidateIssuer = false;
        validationParameters.ValidateAudience = false;
        validationParameters.IssuerSigningKey = securityKey;
        return base.ValidateToken( token, validationParameters, out validatedToken );
    }
}