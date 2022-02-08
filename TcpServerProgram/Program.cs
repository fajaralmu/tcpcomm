using System.Net.Sockets;
using System.Text;

public class Program
{
    
    const int DATA_SIZE_LENGTH = 4;
    static TcpListener _listener;
    public static void Main(string[] args)
    {
        _listener = new TcpListener(8081);
        
        _listener.Start();
        Console.WriteLine("Server Started");
        Task.Run(Run);
        Console.ReadLine();
    }

    static void Run()
    {
        while(true)
        {
            Console.WriteLine("accepting tcp client");
            TcpClient client = _listener.AcceptTcpClient();
            Console.WriteLine("connected to client: " + client.Client.SocketType);
            Task.Run(()=>{
                
                try {
                    while(client.Connected)
                    {
                        Process(client);
                    }
                } catch (Exception e)
                {
                    Console.WriteLine("Error process clien: " + e.Message );
                }
                
                Console.WriteLine("Client closed");
            });
        }
    }

    private static void Process(TcpClient _client)
    {
        bool readData = false;
        int dataLength = 0;
        string data = "";
        _client.GetStream().ReadTimeout = -1;
        while(_client.Connected)
        {
            Console.WriteLine("Will read");
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

                ProcessRequest(_client, data);
                Console.WriteLine("request has been proceeded");
                return;
            }
        }
    }

    private static void ProcessRequest(TcpClient client, string data)
    {
        string response = "Response from server: received \"" + data + "\"";
        client.GetStream().Write( GetBytes( response ) );
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

}
