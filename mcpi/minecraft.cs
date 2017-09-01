using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace MinecraftPiAPI
{
    public class Server
    {
        // Data buffer for incoming data.  
        private byte[] bytes = new byte[1024];
        private Socket sender;

        public Server(String address, Int32 port)
        {
            // Establish the remote endpoint for the socket.
            IPAddress ipAddress = IPAddress.Parse(address);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP  socket.  
            sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint
            sender.Connect(remoteEP);

            Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
        }

        ~Server()
        {
            // Release the socket.  
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        public void Send(String message)
        {
            // Encode the data string into a byte array.  
            byte[] msg = Encoding.ASCII.GetBytes(message);

            // Send the data through the socket.  
            Console.WriteLine("sending data: {0}", message);
            int bytesSent = sender.Send(msg);
            Console.WriteLine("data sent: {0}", message);
        }

        private String Receive()
        {
            // Receive the response from the remote device.  
            int bytesRec = sender.Receive(bytes);
            string data = Encoding.ASCII.GetString(bytes,0,bytesRec);
            Console.WriteLine("data received: {0}", data);
            return data;
        }

        private String SendReceive(String message)
        {
            Send(message);
            return Receive();
        }

        public void PostToChat(String message)
        {
            String command = String.Format("chat.post({0})\n", message);
            Send(command);
        }

        public Int32 GetHeight(Int32 x, Int32 z)
        {
            String command = String.Format("world.getHeight({0},{1})\n", x, z);
            String response = SendReceive(command);
            return Int32.Parse(response);
        }

        public Int32 GetBlock(Int32 x, Int32 y, Int32 z)
        {
            String command = String.Format("world.getBlock({0},{1},{2})\n", x, y, z);
            String response = SendReceive(command);
            return Int32.Parse(response);
        }

        public List<Int32> GetBlockWithData(Int32 x, Int32 y, Int32 z)
        {
            String command = String.Format("world.getBlockWithData({0},{1},{2})\n", x, y, z);
            String response = SendReceive(command);
            return ParseResponseToInt(response);
        }

        public void SetBlock(Int32 x, Int32 y, Int32 z, Int32 id)
        {
            SetBlock(x, y, z, id, 0);
        }

        public void SetBlock(Int32 x, Int32 y, Int32 z, Int32 id, Int32 data)
        {
            String command = String.Format("world.setBlock({0},{1},{2},{3},{4})\n", x, y, z, id, data);
            Send(command);
        }

        public List<Double> GetPlayerPos()
        {
            String response = SendReceive("player.getPos()\n");
            return ParseResponseToDouble(response);
        }

        public void SetPlayerPos(Double x, Double y, Double z)
        {
            String command = String.Format("player.setPos({0}, {1}, {2})\n", x, y, z);
            Send(command);
        }

        private List<Int32> ParseResponseToInt(String response)
        {
            String[] elements = response.Split(',');
            List<Int32> parsedResponse = new List<Int32>();
            foreach (String element in elements)
            {
                parsedResponse.Add(Int32.Parse(element));
            }
            return parsedResponse;
        }

        private List<Double> ParseResponseToDouble(String response)
        {
            String[] elements = response.Split(',');
            List<Double> parsedResponse = new List<Double>();
            foreach (String element in elements)
            {
                parsedResponse.Add(Double.Parse(element));
            }
            return parsedResponse;
        }
    }

    class Minecraft
    {
        private Server connection;

        public Minecraft() 
        {
            Connect("127.0.0.1", 4711);
        }

        public Minecraft(String address, Int32 port) 
        {
            Connect(address, port);
        }

        public Server Connection
        { 
            get
            {
                return connection;
            }
        }

        public Chat Chat
        {
            get 
            {
                return new Chat(Connection);
            }
        }
        
        public World World
        {
            get 
            {
                return new World(Connection);
            }
        }

        public Player Player
        {
            get 
            {
                return new Player(Connection);
            }
        }

        private void Connect(String address, Int32 port)
        {
            connection = new Server(address, port);
        }

    }

    class World
    {
        private Server connection;

        public World(Server connection)
        {
            this.connection = connection;
        }

        public Block Block(Double x, Double y, Double z)
        {
            return new Block(connection, Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(z));
        }

        public Block Block(Int32 x, Int32 y, Int32 z)
        {
            return new Block(connection, x, y, z);
        }

        public Block Block(Position position)
        {
            return new Block(connection, position.BlockX, position.BlockY, position.BlockZ);
        }

        public Block Block(PlayerPosition position)
        {
            Position playerPosition = position.Get();
            return new Block(connection, playerPosition.BlockX, playerPosition.BlockY, playerPosition.BlockZ);
        }

        public Int32 Height(Double x, Double z)
        {
            return connection.GetHeight(Convert.ToInt32(x), Convert.ToInt32(z));
        }

        public Int32 Height(Int32 x, Int32 z)
        {
            return connection.GetHeight(x, z);
        }
    }

    class Player
    {
        private Server connection;
        /*private bool hostPlayer;
        private Int32 entityId;*/

        public Player(Server connection)
        {
            this.connection = connection;
            //hostPlayer = true;
        }

        /*public Player(Server connection, Int32 entityId)
        {
            this.connection = connection;
            hostPlayer = false;
            this.entityId = entityId
        }*/

        public PlayerPosition Position
        {
            get
            {
                return new PlayerPosition(connection);
            }
        }
    }

    class PlayerPosition
    {
        private Server connection;
        /*private bool hostPlayer;
        private Int32 entityId;*/

        public PlayerPosition(Server connection)
        {
            this.connection = connection;
            //hostPlayer = true;
        }

        public PlayerPosition(Server connection, Position newPosition)
        {
            this.connection = connection;
            //hostPlayer = true;
        }

        /*public PlayerPosition(Server connection, Int32 entityId)
        {
            this.connection = connection;
            hostPlayer = false;
            this.entityId = entityId
        }*/

        public Double X 
        { 
            get 
            {
                List<Double> currentPosition = connection.GetPlayerPos();
                return currentPosition[0]; 
            }
            set 
            {
                List<Double> currentPosition = connection.GetPlayerPos();
                if (value != currentPosition[0]) 
                {
                    Set(value, currentPosition[1], currentPosition[2]);
                }
            }
        }

        public Double Y 
        { 
            get 
            {
                List<Double> currentPosition = connection.GetPlayerPos();
                return currentPosition[1]; 
            }
            set 
            {
                List<Double> currentPosition = connection.GetPlayerPos();
                if (value != currentPosition[1]) 
                {
                    Set(currentPosition[0], value, currentPosition[2]);
                }
            }
        }

        public Double Z 
        { 
            get 
            {
                List<Double> currentPosition = connection.GetPlayerPos();
                return currentPosition[2]; 
            }
            set 
            {
                List<Double> currentPosition = connection.GetPlayerPos();
                if (value != currentPosition[2]) 
                {
                    Set(currentPosition[0], currentPosition[1], value);
                }
            }
        }

        public Position Get()
        {
            List<Double> currentPosition = connection.GetPlayerPos();
            return new Position(currentPosition[0], currentPosition[1], currentPosition[2]);
        }

        public void Set(Position position)
        {
            Set(position.X, position.Y, position.Z);
        }

        public void Set(Int32 x, Int32 y, Int32 z)
        {
            Set(Convert.ToDouble(x), Convert.ToDouble(y), Convert.ToDouble(z));
        }

        public void Set(Double x, Double y, Double z)
        {
            connection.SetPlayerPos(x, y, z);
        }
    }

    class Position
    {
        private Double _x, _y, _z;

        public Position(Double x, Double y, Double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public Double X { get { return _x; } set { _x = value; } }
        public Double Y { get { return _y; } set { _y = value; } }
        public Double Z { get { return _z; } set { _z = value; } }
        public Int32 BlockX { get { return Convert.ToInt32(Math.Floor(X)); } }
        public Int32 BlockY { get { return Convert.ToInt32(Math.Floor(Y)); } }
        public Int32 BlockZ { get { return Convert.ToInt32(Math.Floor(Z)); } }
    }

    class Chat
    {
        private Server connection;

        public Chat(Server connection)
        {
            this.connection = connection;
        }

        public void Post(String message)
        {
            connection.PostToChat(message);
        }
    }

    class Block
    {
        private Server connection;
        private Int32 _x, _y, _z;

        public Block(Server connection, Int32 x, Int32 y, Int32 z)
        {
            this.connection = connection;
            _x = x;
            _y = y;
            _z = z;
        }

        public Int32 X { get { return _x; } }
        public Int32 Y { get { return _y; } }
        public Int32 Z { get { return _z; } }

        public Int32 Id 
        { 
            get
            {
                List<Int32> currentBlockValues = connection.GetBlockWithData(_x, _y, _z);
                return currentBlockValues[0];
            }
            set
            {
                List<Int32> currentBlockValues = connection.GetBlockWithData(_x, _y, _z);
                if (value != currentBlockValues[0]) 
                { 
                    connection.SetBlock(_x, _y, _z, value, currentBlockValues[1]);
                }
            }
        }

        public Int32 Data
        { 
            get
            {
                List<Int32> currentBlockValues = connection.GetBlockWithData(_x, _y, _z);
                return currentBlockValues[1];
            }
            set
            {
                List<Int32> currentBlockValues = connection.GetBlockWithData(_x, _y, _z);
                if (value != currentBlockValues[1]) 
                { 
                    connection.SetBlock(_x, _y, _z, currentBlockValues[0], value);
                }
            }
        }

        public void Set(Int32 id)
        {
            Id = id;
        }

        public void Set(BlockValue value)
        {
            Set(value.Id, value.Data);
        }

        public void Set(Int32 id, Int32 data)
        {
            connection.SetBlock(_x, _y, _z, id, data);
        }

        public BlockValue Get()
        {
            List<Int32> currentBlockValues = connection.GetBlockWithData(_x, _y, _z);
            return new BlockValue(currentBlockValues[0], currentBlockValues[1]);
        }

        public override string ToString()
        {
            return String.Format("Block : X={0}, Y={1}, Z={2}, Id={3}, Data={4}", X, Y, Z, Id, Data);
        }
    }

    class Blocks
    {
        public static BlockValue Air = new BlockValue(0);
        public static BlockValue Stone = new BlockValue(1);

        public static BlockValue Tnt = new BlockValue(46);
        public static BlockValue TntInactive = new BlockValue(46);
        public static BlockValue TntActive = new BlockValue(46, 1);
    }

    class BlockValue
    {
        private Int32 _id;
        private Int32 _data;

        public BlockValue() { }

        public BlockValue(Int32 id)
        {
            _id = id;
            _data = 0;
        }

        public BlockValue(Int32 id, Int32 data)
        {
            _id = id;
            _data = data;
        }

        public Int32 Id { get { return _id; } }
        public Int32 Data { get { return _data; } }
    } 

    
}