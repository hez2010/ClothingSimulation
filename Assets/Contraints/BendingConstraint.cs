using System;

namespace Assets.Contraints
{
    class BendingConstraint : ConstraintBase
    {
        private readonly float _stiffness;
        public BendingConstraint(ClothComponent cloth, float stiffness) : base(cloth)
        {
            _stiffness = stiffness;
        }

        public override void Resolve(float dt)
        {
            throw new NotImplementedException();
        }
    }
}
