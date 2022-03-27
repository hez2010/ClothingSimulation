namespace Assets.Contraints
{
    class BendingConstraint : ConstraintBase
    {
        private readonly float _stiffness;
        private readonly int _index1, _index2, _index3, _index4;
        private readonly float _length;

        public BendingConstraint(ClothComponent cloth, float stiffness, int index1, int index2, int index3, int index4) : base(cloth)
        {
            _stiffness = stiffness;
            _index1 = index1;
            _index2 = index2;
            _index3 = index3;
            _index4 = index4;
            _length = (_cloth.Positions[_index3] - ((_cloth.Positions[index1] + _cloth.Positions[index2] + _cloth.Positions[index3]) / 3.0f)).magnitude;
        }

        public override void Resolve(float dt)
        {
            var center = (_cloth.Predicts[_index1] + _cloth.Predicts[_index2] + _cloth.Predicts[_index3]) / 3.0f;
            var dir = _cloth.Predicts[_index3] - center;
            var dis = dir.magnitude;
            var diff = 1.0f - (_length / dis);
            var force = dir * diff;
            var corr1 = _stiffness / 2.0f * force * (float)dt;
            _cloth.Predicts[_index1] += corr1;
            var corr2 = _stiffness / 2.0f * force * (float)dt;
            _cloth.Predicts[_index2] += corr2;
            var corr3 = (float)dt * -_stiffness * force;
            _cloth.Predicts[_index3] += corr3;
        }
    }
}
