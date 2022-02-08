// See https://aka.ms/new-console-template for more information
using System.Net.Sockets;
using System.Text;

namespace TcpClientProgram;


public class Program
{
    static TcpClient _client;
    const int DATA_SIZE_LENGTH = 4;
    public static void Main(string[] args)
    {
       
        _client = new TcpClient();
        Task.Run(Run);
         
        Console.WriteLine("Press enter to exit...");
        Console.ReadLine();
    }

    static void Run()
    {
        try {
        _client.Connect(System.Net.IPAddress.Loopback, 8081);
        Console.WriteLine("connected");
        Task.Run(Receive);
        Send("Hello");
        }
        catch (Exception e)
        {
            Console.WriteLine("<!>Error: " +e.Message);
        }
    }

    static void Receive()
    {
        bool readData = false;
        int dataLength = 0;
        string data = "";
        while(_client.Connected)
        {
            if (!readData)
            {
                byte[] dataLengthBytes = new byte[DATA_SIZE_LENGTH];
                _client.GetStream().Read(dataLengthBytes, 0, dataLengthBytes.Length);
                dataLength = BitConverter.ToInt32(dataLengthBytes);
                Console.WriteLine($"Read data length: " +dataLength);
                readData = true;
            }
            else
            {
                byte[] dataBytes = new byte[dataLength];
                _client.GetStream().Read(dataBytes, 0, dataLength);
                data = Encoding.ASCII.GetString(dataBytes);
                Console.WriteLine($"Read data: " + data);
                readData = false;
            }
        }
    }

    static void Send(string payload)
    {
        Console.WriteLine("Send: " + payload);
        byte[] bytes = GetBytes( payload );
        Console.WriteLine($"Send bytes {string.Join(",", bytes)}");
        _client.GetStream().Write( bytes );
    }

    static byte[] GetBytes( string payload )
    {
        
        byte[] stringBytes  = Encoding.ASCII.GetBytes( payload );
        Console.WriteLine($"string bytes: {string.Join(",", stringBytes)}");
        byte[] sizeBytes    = BitConverter.GetBytes(stringBytes.Length);
        Console.WriteLine($"size bytes: {string.Join(",", sizeBytes)}");
        byte[] bytes        = new byte[sizeBytes.Length + stringBytes.Length];
        for (var i = 0; i < sizeBytes.Length; i++)
        {
            bytes[i] =sizeBytes[i];
        }
        for (var i = sizeBytes.Length  ; i < bytes.Length; i++)
        {
            int stringBytesIndex = i - sizeBytes.Length  ;
            bytes[i] = stringBytes[stringBytesIndex];
        }
        return bytes;
    }

    // private static string GetResponse(NetworkStream stream)
    // {
    //     byte[] data = new byte[DataSize];
    //     stream.Read( data, 0, data.Length );
    //     return Encoding.ASCII.GetString( data, 0, data.Length );
    // }
}

