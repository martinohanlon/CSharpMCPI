using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

public class SocketListener {
    
    // Incoming data from the client.
    public string data = null;
    Socket handler;

    Dictionary<string, string> commands = new Dictionary<string, string>()
    {
        {"world.getBlock", "1\n"},
        {"world.getBlockWithData", "1,0\n"},
        {"world.setBlock", ""},
        {"world.getHeight", "10\n"},
        {"chat.post", ""},
        {"player.getPos", "10,11,12\n"}
    };

    public SocketListener() {
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];
        bool connected = false;
        int bytesRec;
        string command;
        int endOfCommand;
        byte[] msg;
        int bytesSent;

        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 4711);

        // Create a TCP/IP socket.
        Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp );

        // Bind the socket to the local endpoint and 
        // listen for incoming connections.
        try {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.
            while (true) {
                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.
                handler = listener.Accept();
                connected = true;

                // An incoming connection needs to be processed.
                while (connected) {
                    bytes = new byte[1024];
                    // manage a connection
                    try
                    {
                        bytesRec = handler.Receive(bytes);
                        // if receive stops blocking and returns 0 bytes the connection is lost.
                        if (bytesRec > 0) 
                        {
                            data += Encoding.ASCII.GetString(bytes,0,bytesRec);
                            // have we got a complete command
                            if (data.EndsWith("\n")) 
                            {
                                // Show the data on the console.
                                Console.WriteLine("Data received : {0} : {1} bytes", data, bytesRec);

                                // get the command - up to the open bracket (
                                endOfCommand = data.IndexOf("(");
                                if (endOfCommand > 0) 
                                {
                                    command = data.Substring(0, endOfCommand);
                                    
                                    // is the command in the dictionary of commands?
                                    if (commands.ContainsKey(command))
                                    { 
                                        Console.WriteLine("Command : {0}", command);
                                        if (commands[command].Length > 0)
                                        {
                                            msg = Encoding.ASCII.GetBytes(commands[command]);
                                            bytesSent = handler.Send(msg);
                                            Console.WriteLine("Data sent : {0} : {1} bytes", Encoding.ASCII.GetString(msg,0,bytesSent), bytesSent);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Command not found : {0}", command);
                                    }
                                } 
                                else
                                {
                                    Console.WriteLine("Not a valid command ( not found");
                                }
                                // processed command, clear it
                                data = null;
                            }
                        } 
                        else
                        {
                            connected = false;
                            handler.Shutdown(SocketShutdown.Both);
                            handler.Close();
                        }
                    }
                    catch (Exception e) {
                       Console.WriteLine(e.ToString());
                       connected = false;
                    }
                }
            }
        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }

    }

    ~SocketListener()
    {
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
    }
}

class MockMinecraft
{
    public static void Main() 
    {
        SocketListener listener = new SocketListener();
    }
}