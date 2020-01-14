using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using TestuKurimoSistema.Constants;
using Microsoft.AspNetCore.Http;

namespace TestuKurimoSistema.OAuth2
{
    public static class TokenValidator
    {
        private static string username;
        public static string Validate(string token, out string exception, out string name)
        {
            name = "";
            string ret = Validate(token, out exception);
            name = username;
            return ret;
        }
        public static string Validate(string token, out string exception)
        {
            exception = "";
            username = "";
            var TokenString = token;
            if (TokenString == "")
            {
                return null;
            }
            
            var Token = TokenString;
            var tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken check = null;
            try
            {
                check = tokenHandler.ReadJwtToken(Token);
            }
            catch (ArgumentException e)
            {
                exception = e.Message;
            }
            
            if (check == null)
                return null;
            var key = Secret.getSecret().Select(s => Convert.ToByte(s)).ToArray();
            var validationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidIssuer = "veryveryverysecret",
                ValidAudience = "TestuKurimoSistema"
            };
            SecurityToken validatedToken = null;
            ClaimsPrincipal claims = null;
            try
            {
                claims = tokenHandler.ValidateToken(Token, validationParameters, out validatedToken);
            }
            catch (SecurityTokenException e)
            {
                exception = e.Message;
            }
            if (claims == null)
            {
                return null;
            }
            string claim = null;
            if (claims.FindFirst(ClaimsIdentity.DefaultRoleClaimType) != null)
            claim = claims.FindFirst(ClaimsIdentity.DefaultRoleClaimType).ToString().Split(' ')[1];
            if ((claim == null || claim == "") && claims.FindFirst(ClaimTypes.NameIdentifier)!= null)
                claim = claims.FindFirst(ClaimTypes.NameIdentifier).ToString().Split(' ')[1];
            if (claims.FindFirst(ClaimTypes.NameIdentifier) != null)
                username = claims.FindFirst(ClaimTypes.NameIdentifier).ToString().Split(' ')[1];
            return claim;
        }
    }
}
