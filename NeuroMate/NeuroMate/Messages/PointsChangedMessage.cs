using CommunityToolkit.Mvvm.Messaging;

namespace NeuroMate.Messages
{
    public class PointsChangedMessage
    {
        public int NewPoints { get; }

        public PointsChangedMessage(int newPoints)
        {
            NewPoints = newPoints;
        }
    }
}
