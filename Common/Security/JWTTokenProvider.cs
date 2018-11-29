using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Common.Security
{
    public class JwtTokenProvider
    {
        private readonly SigningCredentials _credentials;
        private readonly JwtSecurityTokenHandler _handler;
        private readonly JwtTokenSettings _settings;
        private readonly SymmetricSecurityKey _signingKey;

        public JwtTokenProvider(JwtTokenSettings settings)
        {
            _settings = settings;
            _handler = new JwtSecurityTokenHandler();
            var securityKey = settings.SigningKey;
            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            _credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature);
        }

        public async Task<JwtToken> SignAsync(string userName, IEnumerable<Claim> userClaims, int expiration = 20)
        {
            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer64)
            };
            var claims2 = userClaims as Claim[] ?? new Claim[0];
            if (claims2.Any())
                claims.AddRange(claims2);
            var expire = now.AddMinutes(expiration);
            var header = new JwtHeader(_credentials);
            var payload = new JwtPayload(_settings.Issuer, _settings.Audience, claims, now, expire);
            var token = new JwtSecurityToken(header, payload);
            var accessToken = _handler.WriteToken(token);
            var rs = new JwtToken
            {
                access_token = accessToken,
                expires_in = (int) TimeSpan.FromMinutes(expiration).TotalSeconds
            };
            return await Task.FromResult(rs);
        }

        public async Task<ClaimsPrincipal> VerifyAsync(string token)
        {
            var parameters = GetTokenValidationParameters();
            var principal = _handler.ValidateToken(token, parameters, out _);
            return await Task.FromResult(principal);
        }

        public TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters
            {
                AuthenticationType = JwtBearerDefaults.AuthenticationScheme,
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}