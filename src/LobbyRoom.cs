using shared;
using shared.src;
using shared.src.protocol;
using shared.src.serialization;
using System.Numerics;

namespace server.src
{
    public class LobbyRoom(string name) : Room(name)
    {
        readonly Random random = new();
        const float areaMinX = -17;
        const float areaMaxX = 17;
        
        const float areaMinY = 0.0f;
        const float areaMaxY = 0.01f;
        
        const float areaMinZ = -4;
        const float areaMaxZ = 18;

        const int SKIN_COUNT = 4;

        public override void ProcessMessage(IMessage msg, GameClient sender)
        {
            switch (msg)
            {
                case SimpleMessage simpleMessage:
                    //echo for now
                    ProcessChatMessage(simpleMessage, sender);
                    break;
                case MoveRequest moveRequest:
                    Console.WriteLine("move request");
                    float x = moveRequest.x;
                    float y = moveRequest.y;
                    float z = moveRequest.z;
                    if(MathF.Abs(x) < areaMaxX && InRange(y, areaMinY, areaMaxY) && InRange(z, areaMinZ, areaMaxZ))
                    {
                        MoveResponse resp = new() { x = x, y = y, z = z, moverId = sender.Info.ID };
                        sender.Info.Pos.vector = new Vector3(x, y, z);

                        SafeForEach(m => m.SendMessage(resp));
                    }
                break;
            }
        }

        void ProcessChatMessage(SimpleMessage msg, GameClient sender)
        {
            string input = msg.Text;
            if (input[0] == '/')
            {
                int commandIndex = input.IndexOf(' ');
                string commandName = input[1..commandIndex];
                string rest = input[input.IndexOf(' ')..];
                switch (commandName)
                {
                    case "whisper":
                        IEnumerable<GameClient> targets = _clients.Where(x=> x != sender && Vector3.Distance(x.Info.Pos.vector, sender.Info.Pos.vector) <= 2);

                        foreach (var x in _clients)
                        {
                            var dist = Vector3.Distance(x.Info.Pos.vector, sender.Info.Pos.vector);
                        }
                        
                        SimpleMessage whisper = new() 
                        {
                            SenderID = sender.ID,
                            Text = sender.Info.Name + " whispers: " + rest
                        };
                        SimpleMessage responseToSelf = new()
                        {
                            SenderID = sender.ID,
                            Text = !targets.Any() ? "no one around" : "You whisper: " + input
                        };
                        foreach (var t in targets) t.SendMessage(whisper);
                        sender.SendMessage(responseToSelf);
                    break;
                    case "changeSkin":
                        if(int.TryParse(rest.Trim(), out int index) && index > -1 && index < SKIN_COUNT)
                        {
                            var changeSkin = new ChangeSkin() { SkinID = index, OwnerID = sender.ID };
                            SafeForEach(m=> m.SendMessage(changeSkin));
                        }
                        else
                        {
                            SimpleMessage wrong = new()
                            {
                                SenderID = sender.ID,
                                Text = "Cannot change sking, bad input"
                            };
                            sender.SendMessage(wrong);
                        }
                        break;
                }
            }
            else
            {
                SimpleMessage response = new() 
                {
                    SenderID = sender.ID, 
                    Text = sender.Info.Name + ": " + input 
                };
                SimpleMessage responseToSelf = new()
                {
                    SenderID = sender.ID,
                    Text = "You" + ": " + input
                };
                SafeForEach(m => m.SendMessage(m == sender ? responseToSelf : response ));

            }
        }

        protected override void OnPlayerJoined(GameClient player)
        {
            player.Info = new PlayerInfo()
            {
                AvatarView = random.Next(0, SKIN_COUNT),
                Pos = new NetVec3() { vector = new Vector3( 
                    areaMinX + (float)(random.NextDouble() * areaMaxX), 
                    areaMinY + (float)(random.NextDouble() * areaMaxY), 
                    areaMinZ + (float)(random.NextDouble() * areaMaxZ)
                )},
                Name = "Guest " + player.Info.ID,
                ID = player.ID,
                
            };
            var response = new JoinResponse() { 
                playerInfo = player.Info, 
                PlayerInfos = new() 
                { 
                    List = _clients.Where(x=> x != player).Select(x=> x.Info).ToList()
                } 
            };
            Console.WriteLine(player.Info.Name);
            var responseForOthers = new PlayerJoined() { PlayerInfo = player.Info };
            player.SendMessage(response);

            SafeForEach(client => { if (client != player) client.SendMessage(responseForOthers); });
        }

        static bool InRange(float v,  float min, float max)
        {
            return (v >= min && v <= max);
        }
    }
}
