using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using TestuKurimoSistema.Controllers;
using Microsoft.AspNetCore.Http;

namespace TestuKurimoSistema.OAuth2
{
    public static class TokenValidator
    {
        public static string Validate(HttpRequest request)
        {
            var TokenString = request.Headers.Where(h => h.Key == "Authorization").First().Value.ToString();
            if (TokenString == "")
            {
                return null;
            }
            var Token = TokenString.Split(' ')[1];
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = OAuthController.Secret.Select(s => Convert.ToByte(s)).ToArray();
            var validationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidIssuer = "veryveryverysecret",
                ValidAudience = "TestuKurimoSistema"
            };
            SecurityToken validatedToken = null;
            var claims = tokenHandler.ValidateToken(Token, validationParameters, out validatedToken);
            string claim = null;
            if (claims.FindFirst(ClaimsIdentity.DefaultRoleClaimType) != null)
            claim = claims.FindFirst(ClaimsIdentity.DefaultRoleClaimType).ToString();
            if ((claim == null || claim == "") && claims.FindFirst(ClaimTypes.NameIdentifier)!= null)
                claim = claims.FindFirst(ClaimTypes.NameIdentifier).ToString();
            return claim;
        }
    }
}
