using shared;
using shared.src;
using shared.src.protocol;
using shared.src.serialization;

namespace server.src
{
    public class LobbyRoom(string name) : Room(name)
    {
        readonly Random random = new();
        const float areaMinX = -17;
        const float areaMaxX = -17;
        
        const float areaMinY = 0.0f;
        const float areaMaxY = 0.01f;
        
        const float areaMinZ = -4;
        const float areaMaxZ = 18;

        public override void ProcessMessage(IMessage msg, GameClient sender)
        {
            switch (msg)
            {
                case SimpleMessage simpleMessage:
                    //echo for now
                    SafeForEach(m => m.SendMessage(simpleMessage));
                    break;
                case MoveRequest moveRequest:
                    float x = moveRequest.x;
                    float y = moveRequest.y;
                    float z = moveRequest.z;
                    if(MathF.Abs(x) < areaMaxX && InRange(y, areaMinY, areaMaxY) && InRange(z, areaMinZ, areaMaxZ))
                    {
                        MoveResponse resp = new() { x = x, y = y, z = z, moverId = sender.ID };
                        SafeForEach(m => m.SendMessage(resp));
                    }
                break;
            }
        }

        protected override void OnPlayerJoined(GameClient player)
        {
            player.Info = new PlayerInfo()
            {
                AvatarView = random.Next(0, 3),
                Pos = new NetVec3() { vector = new System.Numerics.Vector3( 
                    areaMinX + (float)(random.NextDouble() * areaMaxX), 
                    areaMinY + (float)(random.NextDouble() * areaMaxY), 
                    areaMinZ + (float)(random.NextDouble() * areaMaxZ)
                )}
            };
            var response = new JoinResponse() { playerInfo = player.Info, ClientID = player.ID };
            var responseForOthers = new PlayerJoined() { ID = player.ID, PlayerInfo = player.Info };
            player.SendMessage(response);

            SafeForEach(client => { if (client != player) client.SendMessage(responseForOthers); });
        }

        static bool InRange(float v,  float min, float max)
        {
            return (v >= min && v <= max);
        }
    }
}
