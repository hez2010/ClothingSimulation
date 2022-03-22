namespace Assets.Forces
{
    abstract class ForceBase
    {
        public abstract void Apply(ClothComponent cloth, float dt);
    }
}
