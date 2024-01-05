# ServerSocket library

The ServerSocket library provides an implementation of a server socket in C#. It allows accepting client connections, receiving and sending data, and handling connection and disconnection events.

## Usage

The following is an example of how to use the ServerSocket library to accept client connections:
```csharp
var serverSocket = new ServerSocket(PortListening: 4567, SocketBufferSize: 1024)
{
    ServerIP = "127.0.0.1",
    EncodingType= DataEncodingType.ASCII
};
serverSocket.ConnectedClient += (client) => Console.WriteLine($"Client connected: {client}");
serverSocket.DisconnectedClient += (client) => Console.WriteLine($"Client disconnected: {client}");
serverSocket.DataReceived += (data, clientId) => Console.WriteLine($"Data received: {data} from client: {clientId}");
await serverSocket.Listen(new CancellationToken());
```

## License 

The ServerSocket library is distributed under the MIT license. See the LICENSE file for more information.
