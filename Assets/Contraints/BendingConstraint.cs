namespace Assets.Contraints
{
    class BendingConstraint : ConstraintBase
    {
        private readonly float _stiffness;
        private readonly int _index1, _index2, _index3;
        private readonly float _length;

        public BendingConstraint(ClothComponent cloth, float stiffness, int index1, int index2, int index3) : base(cloth)
        {
            _stiffness = stiffness;
            _index1 = index1;
            _index2 = index2;
            _index3 = index3;
            _length = (_cloth.Positions[_index3] - ((_cloth.Positions[index1] + _cloth.Positions[index2] + _cloth.Positions[index3]) / 3.0f)).magnitude;
        }

        public override void Resolve(float dt)
        {
            var center = (_cloth.NewPositions[_index1] + _cloth.NewPositions[_index2] + _cloth.NewPositions[_index3]) / 3.0f;
            var dir = _cloth.NewPositions[_index3] - center;
            var dis = dir.magnitude;
            var diff = 1.0f - (_length / dis);
            var force = dir * diff;
            var corr1 = _stiffness / 2.0f * force * (float)dt;
            _cloth.Delta[_index1].Vec += corr1;
            _cloth.Delta[_index1].Count++;
            var corr2 = _stiffness / 2.0f * force * (float)dt;
            _cloth.Delta[_index2].Vec += corr2;
            _cloth.Delta[_index2].Count++;
            var corr3 = (float)dt * -_stiffness * force;
            _cloth.Delta[_index3].Vec += corr3;
            _cloth.Delta[_index3].Count++;
        }
    }
}
