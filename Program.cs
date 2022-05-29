using Grpc.Net.Client;
using GrpcEchoClient;
using Grpc.Core;

using var channel = GrpcChannel.ForAddress("http://localhost:5000");
var client = new Echo.EchoClient(channel);
var reply = await client.UnaryEchoAsync(new EchoRequest { Message = "test" });
Console.WriteLine("UnaryEcho: " + reply.Message);

CancellationTokenSource source = new CancellationTokenSource();
CancellationToken token = source.Token;

using var call = client.BidirectionalStreamingEcho(null, null, token);
// using var call = client.BidirectionalStreamingEcho(null, null, token);

var readTask = Task.Run(async () =>
{
    await foreach (var response in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine(response.Message);
    }
});

await call.RequestStream.WriteAsync(new EchoRequest
{
    Message = "hello"
});

await call.RequestStream.WriteAsync(new EchoRequest
{
    Message = "world"
});

// await Task.Delay(100);

// await call.RequestStream.WriteAsync(new EchoRequest
// {
//     Message = "closed"
// });

// source.Cancel();

Console.WriteLine("Disconnecting");
await call.RequestStream.CompleteAsync();
await readTask;
