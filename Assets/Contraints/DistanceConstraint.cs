namespace Assets.Contraints
{
    class DistanceConstraint : ConstraintBase
    {
        private readonly int _index1, _index2;
        private readonly float _distance, _stiffness;

        public DistanceConstraint(ClothComponent cloth, float stiffness, int index1, int index2) : base(cloth)
        {
            _index1 = index1;
            _index2 = index2;
            _distance = (cloth.Positions[index2] - cloth.Positions[index1]).magnitude;
            _stiffness = stiffness;
        }

        public override void Resolve(float dt)
        {
            var mass = _cloth.Mass * 2f;
            var dx = _cloth.NewPositions[_index2] - _cloth.NewPositions[_index1];
            var correction = (dx.magnitude - _distance) / mass * dt * _stiffness * dx.normalized;
            _cloth.Delta[_index1].Vec += correction;
            _cloth.Delta[_index1].Count++;
            _cloth.Delta[_index2].Vec -= correction;
            _cloth.Delta[_index2].Count++;
        }
    }
}
