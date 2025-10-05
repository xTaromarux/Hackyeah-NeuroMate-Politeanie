using CommunityToolkit.Mvvm.Messaging;

namespace NeuroMate.Messages
{
    public class AvatarChangedMessage
    {
        public int? AvatarId { get; }

        public AvatarChangedMessage(int? avatarId = null)
        {
            AvatarId = avatarId;
        }
    }
}
