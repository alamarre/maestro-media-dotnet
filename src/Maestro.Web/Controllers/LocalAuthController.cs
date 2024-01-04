using System.ComponentModel.DataAnnotations;
using Maestro.Auth;
using Maestro.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maestro.Controllers;

public record Credentials(string Username, string Password, Guid? tenantId);

public record TenantUserData(string Username, string Password, string Email);

public record UserToken(string Token, Guid? TenantId, Guid UserId);

public record TokenInfo(Guid tenantId, Guid userId, DateTime expires);

public class LocalAuthController(IDbContextFactory<MediaDbContext> dbContextFactory) : IController
{
    [AllowAnonymous]
    private async Task<IResult> CreateTenantWithAdminUser(string domain, [FromBody] TenantUserData credentials)
    {
        using var db = dbContextFactory.CreateDbContext();

        Guid tenantId = Guid.NewGuid();

        var hashedPassword = GetHashedPassword(credentials.Password, 12);
        var domainCreated = new TenantDomain {
            TenantId = tenantId,
            DomainName = domain
        };
        var user = new AccountUser
        {
            UserId = Guid.NewGuid(),
            TenantId = tenantId
            
        };
        var emailRecord = new AccountEmail
        {
            TenantId = tenantId,
            UserId = user.UserId,
            EmailAddress = credentials.Email
        };
        var login = new AccountLogin
        {
            TenantId = tenantId,
            UserId = user.UserId,
            Username = credentials.Username,
            HashedPasswordPasses = 12,
            HashedPassword = hashedPassword
        };
        
        db.TenantDomain.Add(domainCreated);
        db.AccountUser.Add(user);
        db.AccountEmail.Add(emailRecord);
        db.AccountLogin.Add(login);
        int affectedRows = await db.SaveChangesAsync();
        if(affectedRows == 0) {
            return Results.Problem();
        }
        return Results.Ok();
    }

    [AllowAnonymous]
    private async Task<IResult> Login([FromBody] Credentials credentials, LocalSecurityTokenValidator tokenValidator)
    {
        
        using var db = await dbContextFactory.CreateDbContextAsync();
        var loginInfo = await db.AccountLogin.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Username == credentials.Username);
        if(loginInfo == null) {
            return Results.Unauthorized();
        }
        if(!BCrypt.Net.BCrypt.Verify(credentials.Password, loginInfo.HashedPassword)) {
            return Results.Unauthorized();
        }

        if(credentials.tenantId != null && loginInfo.TenantId != credentials.tenantId) {
            return Results.Unauthorized();
        }

        Dictionary<string, string> additionalClaims = new Dictionary<string, string>();
        if(loginInfo.TenantId != null) {
            additionalClaims.Add("tenantId", loginInfo.TenantId.ToString()!);
        }
        var token = tokenValidator.CreateToken(loginInfo.UserId, additionalClaims);
        var responseObject = new UserToken(token, loginInfo.TenantId, loginInfo.UserId);
        return Results.Ok(responseObject);
    }

    [AllowAnonymous]
    private IResult CreateToken(Guid userId, string? secretKey, [FromBody] Dictionary<string, string> claims, IUserContextProvider userContextProvider, LocalSecurityTokenValidator tokenValidator)
    {
        var user = userContextProvider.GetUserContext();
        if(secretKey == null && user?.IsGlobalAdmin == false) {
            return Results.BadRequest();
        }

        var token = tokenValidator.CreateToken(userId, claims, secretKey);
        return Results.Ok(new UserToken(token, null, userId));
    }

    public void MapRoutes(IEndpointRouteBuilder routes)
    {
        routes.MapPost("/tenant/{domain}", CreateTenantWithAdminUser);
        routes.MapPost("/login", Login);
        routes.MapPost("/token/{userId}/{tenantId?}/{secretKey?}/", CreateToken);
    }

    private string GetHashedPassword(string password, int workFactor = 12)
	{
		return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
	}
}