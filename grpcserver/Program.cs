using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Authentication;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
namespace grpcserver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel(kerstrel =>
                    {
                        kerstrel.Listen(IPAddress.Any, 5000,  o => o.Protocols = HttpProtocols.Http1AndHttp2);
                        kerstrel.Listen(IPAddress.Any, 5001, listenOptions =>
                        {
                            var serverPath = AppDomain.CurrentDomain.BaseDirectory + "cert\\server.pfx";
                            var serverCertificate = new X509Certificate2(serverPath, "123456789");
                            var httpsConnectionAdapterOptions = new HttpsConnectionAdapterOptions()
                            {
                                ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                                SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                                //用chain.Build验证客户端证书
                                ClientCertificateValidation = (cer, chain, error) =>
                                {   
                                    return chain.Build(cer); 
                                },
                                ServerCertificate = serverCertificate
                            };
                            listenOptions.UseHttps(httpsConnectionAdapterOptions);
                     });
                        
                        
                    });
                    
                });
    }
}
