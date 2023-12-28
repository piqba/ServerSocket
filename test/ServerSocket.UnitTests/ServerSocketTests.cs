using System.Net.Sockets;
using System.Text;

namespace ServerSocketUnitTest;
public class ServerSocketTests
{
    [Fact]
    public void Listen_ShouldAcceptClientConnection()
    {
        // Arrange
        var contConnections = 0;
        var cancellationTokenSource = new CancellationTokenSource();
        var serverSocket = new ServerSocket(4567, 1024)
        {
            ServerIP = "127.0.0.1"
        };
        serverSocket.ConnectedClient += (client) => contConnections++;

        // Act
        Task.Run(() => serverSocket.Listen(cancellationTokenSource.Token));
        // Simulate a client connection
        var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect("127.0.0.1", 4567);
        Task.Delay(200).Wait();

        // Assert
        // Verify that the ConnectedClient event is raised
        Assert.Equal(1, contConnections);

        // Clean up
        cancellationTokenSource.Cancel();
        clientSocket.Close();
        clientSocket.Dispose();
    }

    [Fact(Skip = "Test")]
    public void ServerSocket_ShouldReceiveDataFromClient()
    {
        // Arrange
        var contMessages = 0;
        var cancellationTokenSource = new CancellationTokenSource();
        var serverSocket = new ServerSocket(4570, 1024)
        {
            ServerIP = "127.0.0.1"
        };
        serverSocket.DataReceived += (client) => contMessages++;

        // Act
        Task.Run(() => serverSocket.Listen(cancellationTokenSource.Token));
        // Simulate a client connection
        var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect("127.0.0.1", 4570);
        clientSocket.Send(Encoding.ASCII.GetBytes("Hello World"));
        Task.Delay(200).Wait();

        // Assert
        // Verify that the DataReceived event is raised
        Assert.Equal(1, contMessages);

        // Clean up
        cancellationTokenSource.Cancel();
        clientSocket.Close();
        clientSocket.Dispose();
    }

    [Fact(Skip = "Test")]
    public void ServerSocket_ShouldRaiseDisconnectEvent()
    {
        // Arrange
        var contDisconnections = 0;
        var cancellationTokenSource = new CancellationTokenSource();
        var serverSocket = new ServerSocket(4569, 1024)
        {
            ServerIP = "127.0.0.1"
        };
        serverSocket.DisconnectedClient += (client) => contDisconnections++;

        // Act
        Task.Run(() => serverSocket.Listen(cancellationTokenSource.Token));
        // Simulate a client connection
        var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect("127.0.0.1", 4569);
        clientSocket.Disconnect(true);
        // Clean up
        cancellationTokenSource.Cancel();
        clientSocket.Close();
        clientSocket.Dispose();

        Task.Delay(200).Wait();

        // Assert
        // Verify that the DisconnectedClient event is raised
        Assert.Equal(1, contDisconnections);


    }

}