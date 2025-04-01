using System;
using System.Net.Sockets;
using System.Net;
using shared.src;
using System.Collections.ObjectModel;
using server.src;
using shared.src.serialization;
using shared.src.protocol;
using shared;

/**
 * This class implements a simple tcp echo server.
 * Read carefully through the comments below.
 * Note that the server does not contain any sort of error handling.
 */
class TCPServerSample
{
	public static void Main()
	{
		TCPServerSample server = new();
		server.Run();
	}

	TcpListener _listener;
	int _currentClientID = 0;
    
    public   static ReadOnlyDictionary<string, Room> Rooms { get; private set; }
	readonly static Dictionary<string, Room> rooms = [];
    readonly static LobbyRoom lobbyRoom = new("Lobby");
    readonly static Stack<Action> _createRoomCallStack = new();

    void Run()
	{
		Console.WriteLine("Server started on port 55555");
		_listener = new TcpListener(IPAddress.Any, 55555);
		_listener.Start();
        rooms.Add( lobbyRoom.Name, lobbyRoom);

        //var msg = new JoinResponse()
        //{
        //    playerInfo = new PlayerInfo()
        //    {
        //        ID = 1,
        //        AvatarView = 1,
        //        Name = "fwjwf"
        //    },
        //    PlayerInfos = new()
        //    {
        //        List = new()
        //        {
        //            new PlayerInfo()
        //            {
        //                ID = 2,AvatarView = 2, Name = "wgeg"
        //            },
        //            new PlayerInfo()
        //            {
        //                ID = 3, AvatarView = 3, Name = "gge"
        //            },
        //        }
        //    }
        //};

        //var writePack = new Packet();
        //writePack.Write(msg);

        //var readPack = new Packet(writePack.GetBytes());
        //var obj = readPack.ReadObject();

		while (true)
		{
			ProcessNewClients();
			ProcessExistingClients();

            
            while (_createRoomCallStack.Count > 0)
            {
                Action newRoomCall = _createRoomCallStack.Pop();
                newRoomCall();
            }


            //Although technically not required, now that we are no longer blocking, 
            //it is good to cut your CPU some slack
            Thread.Sleep(100);
		}
	}

	void ProcessNewClients()
	{
		while (_listener.Pending())
		{
            var newClient = new GameClient(_listener.AcceptTcpClient(), _currentClientID++);

            lobbyRoom.AddMember(newClient);
            
			Console.WriteLine("Accepted new client.");
		}
	}

    

	static void ProcessExistingClients()
	{
        foreach (var currentRoom in rooms.Values)
            currentRoom.SafeForEach(gameClient =>
            {
                if (!gameClient.HasMessage()) return;
                IMessage msg = gameClient.GetMessage();
                currentRoom.ProcessMessage(msg, gameClient);

            });
        
	}

    public static void JoinOrCreateRoom(string name, GameClient sender, Room leavingRoom)
    {
        _createRoomCallStack.Push(() => {
            if (!rooms.TryGetValue(name, out Room room))
            {
                room = new LobbyRoom(name);
                rooms.Add(name, room);
            }

            leavingRoom.RemoveMember(sender);
            room.AddMember(sender);
        });

    }
}

