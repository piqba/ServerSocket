using System.Net;
using System.Net.Sockets;
using System.Text;

public class ServerSocket(
        int PortListening,
        int SocketBufferSize)
{
    private readonly int _portListening = PortListening;
    private readonly int _socketBufferSize = SocketBufferSize;
    /// <summary>
    /// Event raised when data is received
    /// </summary>
    public event Action<string> DataReceived;
    /// <summary>
    /// Event raised when a client is disconnected, return the client IP address
    /// </summary>
    public event Action<string> DisconnectedClient;
    /// <summary>
    /// Event raised when a client is connected, return the client IP address
    /// </summary>
    public event Action<string> ConnectedClient;
    /// <summary>
    /// Encoding type for the socket, default is ASCII
    /// </summary>
    public DataEncodingType EncodingType { get; set; } = DataEncodingType.ASCII;
    /// <summary>
    /// Server IP address, its optional, default is any local IP address
    /// </summary>
    public string ServerIP { get; set; } = string.Empty;

    /// <summary>
    /// Listen for incoming connections
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task Listen(CancellationToken cancellationToken)
    {
        var localAddress = CheckAndReturnServerAddress();
        // Create socket
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(localAddress, _portListening));
        socket.Listen(0);

        // Accept connections
        while (!cancellationToken.IsCancellationRequested)
        {
            Socket clientSocket = await socket.AcceptAsync(cancellationToken);
            string clientIP = clientSocket.RemoteEndPoint!.ToString()!;
            ConnectedClient?.Invoke(clientIP);
            _ = Task.Run(async () => await HandleClient(clientSocket, cancellationToken), cancellationToken);
        }
    }

    #region Private Methods

    private IPAddress CheckAndReturnServerAddress()
    {
        if (!IsPortAvailable(_portListening))
            throw new Exception($"Port {_portListening} is not available");

        if (!string.IsNullOrEmpty(ServerIP))
        {
            try
            {
                return IPAddress.Parse(ServerIP);
            }
            catch (FormatException)
            {
                throw new Exception($"Server ip {ServerIP} not in the correct format."); ;
            }
        }
        else
        {
            return GetLocalIP()
            ?? throw new Exception("Local IP address not found");
        }
    }
    private static bool IsPortAvailable(int port)
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }
    private async Task HandleClient(Socket clientSocket, CancellationToken cancellationToken)
    {
        try
        {
            byte[] buffer = new byte[_socketBufferSize];
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = await clientSocket.ReceiveAsync(buffer);

                    if (bytesRead == 0)
                    {
                        DisconnectedClient?.Invoke(clientSocket.RemoteEndPoint!.ToString()!);
                        clientSocket.Close();
                        clientSocket.Dispose();
                        break;
                    }

                    var clientData = DecodeMessage(buffer, bytesRead, EncodingType);
                    DataReceived?.Invoke(clientData);
                }
                catch (SocketException)
                {
                    DisconnectedClient?.Invoke(clientSocket.RemoteEndPoint!.ToString()!);
                }

            }

        }
        catch (Exception ex)
        {
            // Handle the exception here
            Console.WriteLine($"An error occurred while handling the client: {ex.Message}");
        }
    }
    private static IPAddress? GetLocalIP()
    {
        // Obtain all local ip addresses
        IPAddress[] localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
        IPAddress? localAddress = null;
        if (localAddresses is not null)
        {
            localAddress = localAddresses
                .Where(address => address.AddressFamily == AddressFamily.InterNetwork) // Only IPv4
                .Where(address => address.GetAddressBytes()[0] != 169) // Exclude link-local addresses (169.254.*.*
                .Where(address => address.GetAddressBytes()[0] != 10) // 
                .FirstOrDefault(address => !IPAddress.IsLoopback(address)); // Exclude loopback
        }
        return localAddress;
    }
    private static string DecodeMessage(byte[] buffer, int bytesRead, DataEncodingType encodingType = DataEncodingType.ASCII)
    {
        string clientData = encodingType switch
        {
            DataEncodingType.ASCII => Encoding.ASCII.GetString(buffer, 0, bytesRead),
            DataEncodingType.UTF8 => Encoding.UTF8.GetString(buffer, 0, bytesRead),
            DataEncodingType.UTF32 => Encoding.UTF32.GetString(buffer, 0, bytesRead),
            DataEncodingType.Unicode => Encoding.Unicode.GetString(buffer, 0, bytesRead),
            _ => throw new Exception("Encoding type not supported")
        };
        return clientData;
    }
    #endregion
    public enum DataEncodingType
    {
        ASCII,
        UTF8,
        UTF32,
        Unicode
    }
}