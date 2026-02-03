using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Abstraction.Infrastructure;
using Domain.Entities;
using Infrastructure.Settings;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Providers;

internal sealed class JwtProvider(JwtSettings jwtSetting) : IJwtProvider
{
    public string Create(User user)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.Secret));

        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier , user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                ]),
            Expires = DateTime.UtcNow.AddMinutes(jwtSetting.ExpirationInMinutes),
            SigningCredentials = signingCredentials,
            Issuer = jwtSetting.Issuer,
            Audience = jwtSetting.Audience
        };

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

        return jwtSecurityTokenHandler.WriteToken(securityToken);
    }
}
