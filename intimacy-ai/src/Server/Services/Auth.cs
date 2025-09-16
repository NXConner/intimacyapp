using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace IntimacyAI.Server.Services
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; } = "IntimacyAI";
        public string Audience { get; set; } = "IntimacyAI-Clients";
        public string SigningKey { get; set; } = string.Empty;
    }

    public static class AuthEndpoints
    {
        public static void MapAuth(this WebApplication app)
        {
            app.MapPost("/api/auth/login", (LoginRequest req, IConfiguration cfg) =>
            {
                // Demo: accept any username with password "password". Replace with real user store.
                if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                    return Results.BadRequest("username and password required");
                if (!string.Equals(req.Password, "password"))
                    return Results.Unauthorized();

                var jwt = GenerateToken(req.Username, cfg);
                return Results.Ok(new { access_token = jwt });
            }).WithOpenApi();

            app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
            {
                if (user?.Identity?.IsAuthenticated != true) return Results.Unauthorized();
                return Results.Ok(new { name = user.Identity!.Name ?? "user" });
            }).WithOpenApi();
        }

        private static string GenerateToken(string username, IConfiguration cfg)
        {
            var issuer = cfg["Jwt:Issuer"] ?? "IntimacyAI";
            var audience = cfg["Jwt:Audience"] ?? "IntimacyAI-Clients";
            var key = cfg["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is required");
            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
            };
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);
            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        public sealed class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}

