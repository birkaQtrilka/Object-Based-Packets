using server.src;
using shared.src;
using shared.src.serialization;

public abstract class Room(string name)
{
    public string Name { get; } = name;

    protected readonly List<GameClient> _clients = [];

    public void SafeForEach(Action<GameClient> method)
    {
        for (int i = _clients.Count - 1; i >= 0; i--)
        {
            if (i >= _clients.Count) continue;

            try
            {
                method(_clients[i]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                if (!_clients[i].Connected) _clients.RemoveAt(i);
            }
        }

    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }

    public abstract void ProcessMessage(IMessage msg, GameClient sender);

    public void AddMember(GameClient client)
    {
        _clients.Add(client);
        OnPlayerJoined(client);
        Console.WriteLine($"{client.Name} joined the room: {Name}");

    }

    public void RemoveMember(GameClient client)
    {
        _clients.Remove(client);
        OnPlayerLeft(client);
        Console.WriteLine($"{client.Name} left the room: {Name}");
    }

    protected virtual void OnPlayerLeft(GameClient player)
    {
        
    }

    protected virtual void OnPlayerJoined(GameClient player)
    {
        
    }
}
