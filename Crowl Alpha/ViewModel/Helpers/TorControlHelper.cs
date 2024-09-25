using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Crowl_Alpha.ViewModel.Helpers
{
    public class TorControlHelper
    {
        private static string controlAddress = "127.0.0.1"; // Localhost
        private static int controlPort = 19051; // ControlPort (default: 9051)
        private static string torPassword = "developmentpassword";
        public static async Task ChangeTorNode()
        {
            try
            {
                // Establish TCP connection to ControlPort
                using (TcpClient client = new TcpClient(controlAddress, controlPort))
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    // Authenticate with the plain-text password
                    writer.WriteLine($"AUTHENTICATE \"{torPassword}\"");
                    writer.Flush();

                    // Read authentication response
                    string authResponse = await reader.ReadLineAsync();
                    if (!authResponse.StartsWith("250"))
                    {
                        throw new Exception("Authentication failed. Response: " + authResponse);
                    }
                    Console.WriteLine("Authentication succeeded.");

                    // Send the NEWNYM signal to change the Tor circuit
                    writer.WriteLine("SIGNAL NEWNYM");
                    writer.Flush();

                    // Read response from the Tor control port
                    string response = await reader.ReadLineAsync();
                    if (!response.StartsWith("250"))
                    {
                        throw new Exception("Failed to signal new Tor circuit. Response: " + response);
                    }

                    Console.WriteLine("Successfully switched to a new Tor circuit.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
