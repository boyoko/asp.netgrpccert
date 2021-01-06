using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace grpcserver
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        const string tokenSchema = "Bearer";
        const string tokenIssuer = "https://localhost:5001";
        const string tokenAudience = "grpc";
        const string tokenSecurityKey = "asp.netcore5.0grpcjwt";
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
           var userName= CheckAuth(context)??string.Empty;
            return Task.FromResult(new HelloReply
            {
                Message = $"Hello {request.Name } Request by:{userName}"
            });
        }

        public override Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            LoginReply reply = new LoginReply { Status = "401", Token = "用户名或者密码无效" };
           
            if (request.Username == "gavin" && request.Password == "gavin")
            {
                reply.Token = CreateToken(request.Username);
                reply.Status = "200";
            }
            Console.WriteLine($"call login: username:{request.Username} ,password:{request.Password}, return token:{reply.Token}");
            return Task.FromResult(reply);
        }

        string CreateToken(string userName) {          
            var claim = new Claim[] {
                    new Claim(ClaimTypes.Name,userName)
                };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(tokenIssuer, tokenAudience, claim, DateTime.Now,  DateTime.Now.AddMinutes(60),creds);
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenStr;
        }
        string CheckAuth(ServerCallContext context) {
          string tokenStr=  context.GetHttpContext().Request?.Headers["Authorization"];
            if (string.IsNullOrEmpty(tokenStr)) {
                return string.Empty;
            }
            if (tokenStr.StartsWith(tokenSchema)) {
                tokenStr = tokenStr.Split(' ')[1];
            }
            SecurityToken token;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecurityKey));
            ClaimsPrincipal claims= new JwtSecurityTokenHandler().ValidateToken(tokenStr, new TokenValidationParameters {
                ValidAudience=tokenAudience,
                ValidIssuer=tokenIssuer ,
                IssuerSigningKey=key

            }, out token);
            var userName = claims.FindFirstValue(ClaimTypes.Name);
            return userName;
        }
    }

}
