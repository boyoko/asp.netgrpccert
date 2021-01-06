using System;
using System.Net.Http;
using System.Threading.Tasks;
using grpcserver;
using Grpc.Net.Client;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using Grpc.Core;
using Newtonsoft.Json;

namespace grpcclient
{
    class Program
    {
        const string url = "https://localhost:5001";
        const string tokenSchema = "Bearer";
        static void Main(string[] args)
        {
            GrpcCall();
            Console.WriteLine("http start................");
            ///
            HttpCall();
        }
        static void GrpcCall() {
            var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions { HttpHandler = GetHttpHandler() });
            var client = new Greeter.GreeterClient(channel);
            var loginReplay = client.Login(new LoginRequest {  Username="gavin", Password="gavin"});
            string token = loginReplay.Token;
            //Console.WriteLine("get token:" + token);
         
            var headers = new Metadata();
            headers.Add("Authorization", $"{tokenSchema} {token}");
            var reply = client.SayHello(new HelloRequest { Name = "GreeterClient" },headers);
            Console.WriteLine("SayHello 1: " + reply.Message);
            ///

            client = new Greeter.GreeterClient(GetChannel(url, token));
            reply = client.SayHello(new HelloRequest { Name = "GreeterClient2" });
            Console.WriteLine("SayHello 2: " + reply.Message);
        }
        static void HttpCall() {
            var httpclient = new HttpClient(GetHttpHandler());
            var loginRequest = "{\"username\":\"gavin\",\"password\":\"gavin\"}";
            HttpContent content = new StringContent(loginRequest);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response=  httpclient.PostAsync(url + "/v1/greeter/login", content).Result;
            response.EnsureSuccessStatusCode();//用来抛异常的
            var loginResponseStr=  response.Content.ReadAsStringAsync().Result;
            var token=  JsonConvert.DeserializeObject<LoginReply>(loginResponseStr).Token;
            //
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(tokenSchema, token);
            request.RequestUri = new Uri(url + "/v1/greeter/gavin");

         
            var helloResponse = httpclient.Send(request);
            var helloResponseStr = helloResponse.Content.ReadAsStringAsync().Result;
           Console.WriteLine(helloResponseStr);
        }
        static HttpClientHandler GetHttpHandler() {
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
            return handler;
        }

      static  GrpcChannel GetChannel(string address, string token) {
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    metadata.Add("Authorization", $"{tokenSchema} {token}"); 
                }
                return Task.CompletedTask;
            });
            
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
                HttpHandler=GetHttpHandler()
            });
            return channel;
        }
    }
}
