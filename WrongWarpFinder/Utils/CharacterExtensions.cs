using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace WrongWarpFinder.Utils;

public static class CharacterExtensions
{
    public static unsafe bool IsJumping(this IPlayerCharacter player)
    {
        Character* character = (Character*)player.Address;
        return character->IsJumping();
    }
}
