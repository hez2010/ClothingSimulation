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
            var dx = _cloth.Predicts[_index2] - _cloth.Predicts[_index1];
            var correction = (dx.magnitude - _distance) * dx.normalized / 2;
            _cloth.Predicts[_index1] += _stiffness * dt * correction;
            _cloth.Predicts[_index2] -= _stiffness * dt * correction;
        }
    }
}
