using UnityEngine;

namespace Assets.Contraints
{
    class FixedPointConstraint : ConstraintBase
    {
        private readonly int _index;
        public Vector3 Position;

        public FixedPointConstraint(ClothComponent cloth, int index, Vector3 position) : base(cloth)
        {
            _index = index;
            Position = position;
        }

        public void UpdatePosition(Vector3 position)
        {
            Position = position;
        }

        public override void Resolve(float dt)
        {
            ref var position = ref _cloth.Positions[_index];
            ref var newPosition = ref _cloth.NewPositions[_index];
            (newPosition.x, newPosition.y, newPosition.z) 
                = (position.x, position.y, position.z) 
                = (Position.x, Position.y, Position.z);
        }
    }
}
