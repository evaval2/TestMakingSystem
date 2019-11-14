using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Newtonsoft.Json;
using TestuKurimoSistema.DB.Entities;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using TestuKurimoSistema.OAuth2;
using Newtonsoft.Json;

namespace TestuKurimoSistema.Controllers
{    
    public class OAuthController : Controller
    {
        /*
         * Get /authorize&client_id=tinklalapis.lt&
scope=resource&
redirect_uri=https://tinklalapis.lt/callback&
response_type=code&state=123

         * oauth/authorize?client_id=....
         * &redirectUri=client.com/accept?
         * <- code=token
         * 
         * oauth/access_token?
         * client_id&
         * client_secret=secretCode&
         * code=token
         * <- access_token=someToken
         * 
         * /temos?access_token=someToken
         * 
         * ?response_type=code&state=st123&client_id=id123&scope=read%20delete%20write&redirect_uri=http%3A%2F%2Ftestai.lt%2Ftemos
         * 
         * 
         * grant_type client_credentials
         * client_id
         * client_secret
         * 
         * 
         * grant type password
         * client id
         * client secret
         * username
         * password
         * 
         * 
         * grant type refresh token
         * client id
         * client secret
         * refresh token 
         * client id = 0oaofza260XVHw7yZ0h7
         * client_secret = s0bQtpDTtpaicnUND8SVz4ZmIjRFIjQqJc86WsfS
         * registered redirect uris https://www.oauth.com/playground/authorization-code.html
         * https://www.oauth.com/playground/authorization-code-with-pkce.html
         *  authorization_code
            refresh_token
            implicit
         * */
        public static string Secret = "veryveryverysecret";
        [Route("/authorize")]
        [HttpGet]        
        public async void Authorize(string client_id, string state, string response_type, string redirect_uri, string scope)
        {
            byte[] buff = null;
            var scopes = scope.Split(' ');
            int errorCode;
            string error;
            string redirect = "{0}?error={1}&error_description={2}&state={3}";
            string body = null;
            if (response_type != "code")
            {
                body = JsonConvert.SerializeObject(new Dictionary<string, string> { { "error", "invalid_request" }, { "error_description", "The response type \"" + response_type + "\" is not supported by the authorization server." } });
                Response.StatusCode = 400;
                buff = body.ToCharArray().Select((c) => Convert.ToByte(c)).ToArray();
                Response.Body.Write(buff, 0, buff.Length);
                
                return;
            }
            if (CheckClient(client_id, redirect_uri, state, out error, out errorCode))
            {
                return;
            }
            
            string code = GetRandomCode();
            body = JsonConvert.SerializeObject(new ClientData(client_id, state, response_type, redirect_uri, scope));
            
            HttpContext.Session.SetString(code, body);
            Response.Headers.Add("redirect_uri", new Microsoft.Extensions.Primitives.StringValues(redirect_uri));
            body = JsonConvert.SerializeObject(new Dictionary<string, string> { { "code", code }, { "state", state } });
            buff = body.ToCharArray().Select((c) => Convert.ToByte(c)).ToArray();
            Response.StatusCode = 200;
         //   Response.Redirect(redirect_uri, true);
            Response.ContentType = "application/json";
            Response.Body.Write(buff, 0, buff.Length);
    //        return Json(body);
        }

        public async Task<User> Authenticate(string username, string password)
        {
            var user = await GetUser(username);
            if (user == null)
                return null;
            if (!user.Password.Equals(password))
            {
                return null;
            }
            return user;
        }
        [Route("/token")]
        [HttpPost]        
        public async Task<IActionResult> Access_token()
        {
            var jsonSerializer = new JsonSerializer();
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            using (var streamReader = new StreamReader(Request.Body))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                parameters = jsonSerializer.Deserialize<Dictionary<string, string>>(jsonTextReader);
            }
            string accessToken = null;
            string refreshToken = null;
            if (!parameters.ContainsKey("client_id"))
                parameters.Add("client_id", "TestuKurimoSistema");
            if (!parameters.ContainsKey("grant_type"))
                return BadRequest();
            if (parameters["grant_type"] == "password")
            {
                if (!parameters.ContainsKey("password") || !parameters.ContainsKey("username"))
                {
                    return BadRequest();
                }
                var user = await Authenticate(parameters["username"], parameters["password"]);
                if (user != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Secret.Select(s=> Convert.ToByte(s)).ToArray();
                    var accesstokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                    new Claim(ClaimTypes.Role, user.UserRole.ToString())
                        }),
                        Expires = DateTime.UtcNow.AddMinutes(5),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        Audience = parameters["client_id"],
                        Issuer = Secret
                    };
                    var refreshtokenDescriptor = new SecurityTokenDescriptor
                    {

                        Subject = new ClaimsIdentity(new Claim[]
                        {
                    new Claim(ClaimTypes.NameIdentifier, user.Username.ToString())
                        }),
                        Expires = DateTime.UtcNow.AddMonths(12),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        Audience = parameters["client_id"],
                        Issuer = Secret
                    };
                    var accesstoken = tokenHandler.CreateToken(accesstokenDescriptor);
                    accessToken = tokenHandler.WriteToken(accesstoken);                    
                    var refreshtoken = tokenHandler.CreateToken(refreshtokenDescriptor);
                    refreshToken = tokenHandler.WriteToken(refreshtoken);
                    user.Token = refreshToken;
                    await user.Update(user.Username, user);
                    return Json(new Dictionary<string, string>() { { "access_token", accessToken }, { "expires", "300" }, { "state", parameters["state"] }, { "refresh_token", refreshToken } });
                }
                else
                {
                    // failed to authenticate
                    return StatusCode(401);
                }
                //return access token and refresh token
            }
            else if (parameters["grant_type"] == "authorization_code")
            {
                return StatusCode(400);
            }
            else if (parameters["grant_type"] == "refresh_token")
            {
                if (!parameters.ContainsKey("refresh_token"))
                    return BadRequest();
                var name = TokenValidator.Validate(parameters["refresh_token"]);
                if (name != null)
                {
                    name = name.Split(' ')[1];
                    var TokenString = parameters["refresh_token"];
                    var users = await GetUsers();
                    User user = null;
                    user = users.Where(u => u.Token == TokenString).First();
                    if (user != null && user.Username == name)
                    {
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var key = Secret.Select(s => Convert.ToByte(s)).ToArray();
                        var accesstokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new Claim[]
                            {
                    new Claim(ClaimTypes.Role, user.UserRole.ToString())
                            }),
                            Expires = DateTime.UtcNow.AddMinutes(5),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                            Audience = parameters["client_id"],
                            Issuer = Secret
                        };
                        var refreshtokenDescriptor = new SecurityTokenDescriptor
                        {

                            Subject = new ClaimsIdentity(new Claim[]
                            {
                    new Claim(ClaimTypes.NameIdentifier, user.Username.ToString())
                            }),
                            Expires = DateTime.UtcNow.AddMonths(12),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                            Audience = parameters["client_id"],
                            Issuer = Secret
                        };
                        var accesstoken = tokenHandler.CreateToken(accesstokenDescriptor);
                        accessToken = tokenHandler.WriteToken(accesstoken);
                        var refreshtoken = tokenHandler.CreateToken(refreshtokenDescriptor);
                        refreshToken = tokenHandler.WriteToken(refreshtoken);
                        user.Token = refreshToken;
                        await user.Update(user.Username, user);
                        return Json(new Dictionary<string, string>() { { "access_token", accessToken }, { "expires", "300" }, { "token_type", "JWTbearer" }, { "refresh_token", refreshToken } });
                    }
                }
            }            
            return StatusCode(400);
        }
        private static string GetRandomCode()
        {
            string rndStr = "";
            for (int i = 0; i < 20; i++)
            {
                string rndTmp = (new Random()).Next(9).ToString();
                rndStr = rndStr + rndTmp;
            }
            return rndStr;
        }
        private static bool CheckClient(string client_id, string redirect_uri, string state, out string error, out int errorCode)
        {
            error = "";
            errorCode = 0;
            bool result = false;
            if (client_id != "TestuKurimoSistema")
                result = true;
            else
            {
                error = "invalid_client";
                errorCode = 401;
            }
            if (redirect_uri != "http://testai.lt/")
                result = true;
            else
            {
                error = "invalid_request";
                errorCode = 400;
            }
            return result;
        }
        private static async Task<User> GetUser(string username)
        {
            var user = new User();
            return await user.Select(username);
        }
        private static async Task<List<User>> GetUsers()
        {
            var user = new User();
            return await user.Select();
        }
        private class ClientData
        {
            protected string client_id { get; set; }
            protected string state { get; set; }
            protected string response_type { get; set; }
            protected string redirect_uri { get; set; }
            protected string scope { get; set; }
            public ClientData(string c_id, string st, string r_type, string r_uri, string sc)
            {
                client_id = c_id; state = st; response_type = r_type; redirect_uri = r_uri; scope = sc;
            }
        }
        
    }    
}
