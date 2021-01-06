using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace grpcserver
{
    public class AuthInterceptor:Interceptor
    {
        const string tokenSchema = "Bearer";
        const string tokenIssuer = "https://localhost:5001";
        const string tokenAudience = "grpc";
        const string tokenSecurityKey = "asp.netcore5.0grpcjwt";
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var userContext = context.GetHttpContext().User;
            string userName = userContext.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(userName)) {
                string tokenStr = context.GetHttpContext().Request?.Headers["Authorization"];
                if (!string.IsNullOrEmpty(tokenStr))
                {

                    if (tokenStr.StartsWith(tokenSchema))
                    {
                        tokenStr = tokenStr.Split(' ')[1];
                    }
                    SecurityToken token;
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecurityKey));
                    ClaimsPrincipal claims = new JwtSecurityTokenHandler().ValidateToken(tokenStr, new TokenValidationParameters
                    {
                        ValidAudience = tokenAudience,
                        ValidIssuer = tokenIssuer,
                        IssuerSigningKey = key

                    }, out token);
                    context.GetHttpContext().User = claims;
                }
               
            }
            return continuation(request, context);
        }
    }
}
