using shared;
using shared.src;
using shared.src.protocol;
using shared.src.serialization;

namespace server.src
{
    public class LobbyRoom(string name) : Room(name)
    {
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
                    if(MathF.Abs(x) < 17 && InRange(y, 0, 0.01f) && InRange(z, -4, 18))
                    {
                        SafeForEach(m => m.SendMessage(new MoveResponse() {x = x, y = y, z = z, moverId = sender.ID}));
                    }
                break;
            }
        }

        static bool InRange(float v,  float min, float max)
        {
            return (v >= min && v <= max);
        }
    }
}
