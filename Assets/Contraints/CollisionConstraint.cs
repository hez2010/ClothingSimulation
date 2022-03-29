using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Contraints
{
    class CollisionConstraint : ConstraintBase
    {
        private readonly int _index;
        private readonly Vector3 _position;
        private readonly Vector3 _normal;
        private Vector3 _velocity;

        public CollisionConstraint(ClothComponent cloth, int index, Vector3 position, Vector3 normal, Vector3 velocity) : base(cloth)
        {
            _index = index;
            _position = position;
            _normal = normal;
            _velocity = velocity;
        }

        public override void Resolve(float dt)
        {
            var v = _velocity - _cloth.Velocities[_index];
            _cloth.Predicts[_index] = _position;
            _cloth.Velocities[_index] = v.Dot(_normal) * _normal;
            _cloth.UpdatedVelocities[_index] = true; 
        }
    }
}
