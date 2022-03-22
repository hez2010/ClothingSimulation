namespace Assets.Contraints
{
    abstract class ConstraintBase
    {
        protected readonly ClothComponent _cloth;
        public ConstraintBase(ClothComponent cloth) => _cloth = cloth;

        public abstract void Resolve(float dt);
    }
}
