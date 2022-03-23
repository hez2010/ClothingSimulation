using System;
using UnityEngine;

namespace Assets.Utils
{
    class HitDetector
    {
        public static bool HitSphere(Vector3 center, float radius, Vector3 position, Vector3 velocity, float time, out float reachTime)
        {
            reachTime = 0f;

            var dis = position - center;
            var a = velocity.Dot(velocity);
            var b = dis.Dot(velocity);
            var c = dis.Dot(dis) - (radius * radius);
            var discriminant = (b * b) - (a * c);
            if (discriminant >= 0)
            {
                var time1 = (-b - Mathf.Sqrt(discriminant)) / a;
                var time2 = (-b + Mathf.Sqrt(discriminant)) / a;
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

                return hit;
            }

            return false;
        }

        public static bool HitBox(Vector3 minPoint, Vector3 maxPoint, Vector3 position, Vector3 velocity, float time, out int axis, out float reachTime)
        {
            var Tmin = float.MaxValue;
            axis = 0;
            reachTime = 0f;
            for (var a = 0; a < 3; a++)
            {
                var invV = 1.0f / velocity[a];
                var t0 = (minPoint[a] - position[a]) * invV;
                var t1 = (maxPoint[a] - position[a]) * invV;

                if (invV < 0f)
                {
                    (t0, t1) = (t1, t0);
                }

                var tmin = Mathf.Max(0, t0);
                var tmax = Mathf.Min(time, t1);
                if (tmax <= tmin)
                {
                    return false;
                }

                if (Tmin > tmin)
                {
                    Tmin = reachTime = tmin;
                    axis = a;
                }
            }

            return true;
        }

        public static bool HitCapsule(Vector3 pA, Vector3 pB, float radius, Vector3 position, Vector3 velocity, float time, out Vector3 normal, out float reachTime)
        {
            var ab = pB - pA;
            var ao = position - pA;
            var ab_d = ab.Dot(velocity);
            var ab_ao = ab.Dot(ao);
            var ab_ab = ab.Dot(ab);
            var m = ab_d / ab_ab;
            var n = ab_ao / ab_ab;

            var q = velocity - (ab * m);
            var r = ao - (ab * n);

            var a = q.Dot(q);
            var b = 2f * q.Dot(r);
            var c = r.Dot(r) - (radius * radius);

            if (Mathf.Abs(a) < float.Epsilon)
            {
                if (!HitSphere(pA, radius, position, velocity, time, out var atime) ||
                    !HitSphere(pB, radius, position, velocity, time, out var btime))
                {
                    normal = default;
                    reachTime = default;
                    return false;
                }

                if (atime < btime)
                {
                    var p = position + (velocity * atime);
                    normal = (p - pA).normalized;
                    reachTime = atime;
                }
                else
                {
                    var p = position + (velocity * btime);
                    normal = (p - pB).normalized;
                    reachTime = btime;
                }

                return true;
            }

            var discriminant = (b * b) - (4.0f * a * c);
            if (discriminant < 0.0f)
            {
                normal = default;
                reachTime = default;
                return false;
            }

            var tmin = (-b - Mathf.Sqrt(discriminant)) / (2.0f * a);
            var tmax = (-b + Mathf.Sqrt(discriminant)) / (2.0f * a);
            var t = tmin;
            if (Mathf.Abs(tmax) < Mathf.Abs(tmin))
            {
                t = tmax;
            }

            // Check if K1 and K2 are inside the line segment defined by AB
            var tK = (t * m) + n;
            if (tK < 0.0f)
            {
                // On sphere (A, r)
                if (HitSphere(pA, radius, position, velocity, time, out var aReachTime))
                {
                    var p = position + (velocity * aReachTime);
                    reachTime = aReachTime;
                    normal = (p - pA).normalized;
                }
                else
                {
                    normal = default;
                    reachTime = default;
                    return false;
                }
            }
            else if (tK > 1.0f)
            {
                // On sphere (B, r)
                if (HitSphere(pB, radius, position, velocity, time, out var bReachTime))
                {
                    var p = position + (velocity * bReachTime);
                    reachTime = bReachTime;
                    normal = (p - pB).normalized;
                }
                else
                {
                    normal = default;
                    reachTime = default;
                    return false;
                }
            }
            else
            {
                // On the cylinder
                var p = position + (velocity * t);
                var k1 = pA + (ab * tK);
                reachTime = t;
                normal = (p - k1).normalized;
            }

            return true;
        }
    }
}
