using Assets.Utils;
using UnityEngine;

namespace Assets.Collisions
{
    class SphereCollisionObject : CollisionObject
    {
        private readonly Vector3 _center;
        private readonly float _radius;
        public SphereCollisionObject(Vector3 center, float radius)
        {
            _center = center;
            _radius = radius;
            Bounds = new Bounds(center, new Vector3(radius * 2, radius * 2, radius * 2));
        }

        public override bool Hit(int index, Vector3 position, Vector3 velocity, float time, out CollisionResult? result)
        {
            var dis = position - _center;
            var a = velocity.normalized.Dot(velocity.normalized);
            var b = dis.Dot(velocity.normalized);
            var c = dis.Dot(dis) - _radius * _radius;
            var discriminant = b * b - a * c;
            if (discriminant >= 0)
            {
                var time1 = (-b - Mathf.Sqrt(discriminant)) / a;
                var time2 = (-b + Mathf.Sqrt(discriminant)) / a;
                var reachTime = 0f;
                var hit = false;

                if (time1 < time && time1 >= 0)
                {
                    reachTime = time1;
                    hit = true;
                }
                else if (time2 < time && time2 >= 0)
                {
                    reachTime = time2;
                    hit = true;
                }
                else if (time1 < 0 && time2 > 0)
                {
                    // inside
                    reachTime = time1;
                    hit = true;
                }
                else if (time2 < 0 && time1 > 0)
                {
                    // inside
                    reachTime = time2;
                    hit = true;
                }

                if (hit)
                {
                    var hitPosition = position + velocity * reachTime;
                    result = new CollisionResult(index, this, hitPosition, (hitPosition - _center) / _radius, reachTime);
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
