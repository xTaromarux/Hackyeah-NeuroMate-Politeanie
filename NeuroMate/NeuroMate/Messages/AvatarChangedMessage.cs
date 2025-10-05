using CommunityToolkit.Mvvm.Messaging;

namespace NeuroMate.Messages
{
    public class AvatarChangedMessage
    {
        public string AvatarId { get; }

        public AvatarChangedMessage(string avatarId)
        {
            AvatarId = avatarId;
        }
    }
}
