using CommunityToolkit.Mvvm.Messaging;

namespace NeuroMate.Messages
{
    public class PointsChangedMessage
    {
        public int Points { get; }

        public PointsChangedMessage(int points)
        {
            Points = points;
        }
    }
}
