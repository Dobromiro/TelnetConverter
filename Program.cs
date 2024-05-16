using System;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string configFilePath = "config.txt";
        string ipAddress = null;
        string comPortName = null;
        int baudRate = 9600;
        int port = 23;

        try
        {
            // Wczytywanie konfiguracji z pliku
            using (StreamReader configReader = new StreamReader(configFilePath))
            {
                string line;
                while ((line = configReader.ReadLine()) != null)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length != 2)
                        continue;

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    switch (key)
                    {
                        case "ipAddress":
                            ipAddress = value;
                            break;
                        case "comPortName":
                            comPortName = value;
                            break;
                        case "baudRate":
                            baudRate = int.Parse(value);
                            break;
                        case "port":
                            port = int.Parse(value);
                            break;
                    }
                }
            }

            if (ipAddress == null || comPortName == null)
            {
                Console.WriteLine("Błąd: Brakujące informacje w pliku konfiguracyjnym.");
                return;
            }

            using (TcpClient client = new TcpClient())
            {
                Console.WriteLine("Connecting to server...");
                await client.ConnectAsync(ipAddress, port);
                Console.WriteLine("Connected to server.");

                using (NetworkStream networkStream = client.GetStream())
                using (StreamReader reader = new StreamReader(networkStream, Encoding.ASCII))
                using (StreamWriter writer = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true })
                using (SerialPort serialPort = new SerialPort(comPortName, baudRate))
                {
                    serialPort.Open();
                    Console.WriteLine($"Serial port {comPortName} opened.");

                    string welcomeMessage = await reader.ReadLineAsync();
                    Console.WriteLine("Server: " + welcomeMessage);
                    serialPort.WriteLine(welcomeMessage);

                    while (true)
                    {
                        string serverResponse = await reader.ReadLineAsync();
                        if (serverResponse == null) break;
                        Console.WriteLine("Server: " + serverResponse);
                        serialPort.WriteLine(serverResponse + "\r");
                        //serialPort.WriteLine(serverResponse + "\r\n");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
