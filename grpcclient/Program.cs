using System;
using System.Net.Http;
using System.Threading.Tasks;
using grpcserver;
using Grpc.Net.Client;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

namespace grpcclient
{
    class Program
    {
        static void Main(string[] args)
        {
           var handler = new HttpClientHandler()
            {
                SslProtocols = SslProtocols.Tls12,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (message, cer, chain, errors) =>
                {
                    return chain.Build(cer);
                }
            };
            var path = AppDomain.CurrentDomain.BaseDirectory + "cert\\client.pfx";
            var crt = new X509Certificate2(path, "123456789");
            handler.ClientCertificates.Add(crt);

             var channel = GrpcChannel.ForAddress("https://localhost:5001",new GrpcChannelOptions{HttpHandler=handler});
            var client =  new Greeter.GreeterClient(channel);
            var reply =  client.SayHello( new HelloRequest { Name = "GreeterClient" });
            Console.WriteLine("Greeting: " + reply.Message);
            ///
            Console.WriteLine("http start................");
             var httphandler = new HttpClientHandler();
             httphandler.ServerCertificateCustomValidationCallback=HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
             var httpclient=new HttpClient(httphandler);
            var ret= httpclient.GetStringAsync("http://localhost:5000/v1/greeter/gavin").Result;
            Console.WriteLine(ret);
        }
    }
}
