namespace Watermelon
{
    public class HealDropBehaviour : ItemDropBehaviour
    {
        public override bool IsPickable(CharacterBehavior characterBehaviour)
        {
            return true;
        }
    }
}