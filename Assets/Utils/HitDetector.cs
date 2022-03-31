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
            var dir = velocity.normalized;
            var a = dir.Dot(dir);
            var b = dis.Dot(dir);
            var c = dis.Dot(dis) - (radius * radius);
            var discriminant = (b * b) - (a * c);
            if (discriminant >= 0)
            {
                var time1 = (-b - Mathf.Sqrt(discriminant)) / a;
                var time2 = (-b + Mathf.Sqrt(discriminant)) / a;
                var hit = false;

                time1 = (dir * time1).magnitude / velocity.magnitude;
                time2 = (dir * time2).magnitude / velocity.magnitude;

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
                //else if (time1 < 0 && time2 > 0)
                //{
                //    // inside
                //    reachTime = time1;
                //    hit = true;
                //}
                //else if (time2 < 0 && time1 > 0)
                //{
                //    // inside
                //    reachTime = time2;
                //    hit = true;
                //}

                return hit;
            }

            return false;
        }

        public static bool InsideSphere(Vector3 center, float radius, Vector3 position)
        {
            var d = center - position;
            return d.Dot(d) <= radius * radius;
        }

        public static bool GetSphereClosestSurfacePoint(Vector3 center, float radius, Vector3 position, out Vector3 normal, out Vector3 closestPosition)
        {
            var c2p = position - center;
            float d2 = c2p.Dot(c2p);
            float r2 = radius * radius;
            if (d2 < r2)
            {
                normal = c2p.normalized;
                closestPosition = center + normal * radius;
                return true;
            }
            else
            {
                normal = closestPosition = default;
                return false;
            }
        }

        public static bool HitBox(Vector3 center, Vector4[] size, Vector3 position, Vector3 velocity, float time, out float reachTime)
        {
            var t0 = float.NegativeInfinity;
            var t1 = float.PositiveInfinity;
            var d = velocity.normalized;

            for (var i = 0; i < 3; i++)
            {
                var u = size[i].GetDirection();
                var r = u.Dot(d);
                var s = u.Dot(center - position);
                if (Mathf.Abs(r) < float.Epsilon)
                {
                    if (-s - (size[i].w / 2) > 0 || -s + (size[i].w / 2) < 0)
                    {
                        reachTime = default;
                        return false;
                    }
                }
                else
                {
                    var t0Tmp = (s - (size[i].w / 2)) / r;
                    var t1Tmp = (s + (size[i].w / 2)) / r;
                    if (t0Tmp > t1Tmp)
                    {
                        (t0Tmp, t1Tmp) = (t1Tmp, t0Tmp);
                    }
                    t0 = Mathf.Max(t0Tmp, t0);
                    t1 = Mathf.Min(t1Tmp, t1);

                    if (t0 > t1 || t1 < 0)
                    {
                        reachTime = default;
                        return false;
                    }
                }
            }

            reachTime = t0 > 0 ? t0 : t1;
            return reachTime <= time;
        }

        public static bool HitAABB(Vector3 minPoint, Vector3 maxPoint, Vector3 position, Vector3 velocity, float time, out Vector3 normal, out float reachTime)
        {
            normal = default;
            reachTime = default;
            var tmin = float.NegativeInfinity;
            var tmax = time;

            for (var a = 0; a < 3; a++)
            {
                var invV = 1.0f / velocity[a];
                var t0 = (minPoint[a] - position[a]) * invV;
                var t1 = (maxPoint[a] - position[a]) * invV;

                if (invV < 0f)
                {
                    (t0, t1) = (t1, t0);
                }

                if (tmin < t0)
                {
                    tmin = t0;
                    reachTime = tmin;
                }

                tmax = Mathf.Min(t1, tmax);
                if (tmax <= tmin)
                {
                    reachTime = default;
                    return false;
                }
            }

            return true;
        }

        public static bool InsideAABB(Vector3 min, Vector4 ax, Vector4 ay, Vector4 az, Vector3 position)
        {
            var o = min;
            position = position - o;
            var projOnAxis = new Vector3(
                position.Dot(ax.GetDirection()),
                position.Dot(ay.GetDirection()),
                position.Dot(az.GetDirection())
            );
            var axisLength = new Vector3(ax.w, ay.w, az.w);
            return projOnAxis is { x: > 0, y: > 0, z: > 0 } && projOnAxis.x < axisLength.x && projOnAxis.y < axisLength.y && projOnAxis.z < axisLength.z;
        }

        static Vector3 select(Vector3 a, Vector3 b, Vector3 c) { return new Vector3(c.x > 0 ? b.x : a.x, c.y > 0 ? b.y : a.y, c.z > 0 ? b.z : a.z); }
        static Vector3 step(Vector3 y, Vector3 x) { return select(new Vector3(0, 0, 0), new Vector3(1, 1, 1), new Vector3(x.x > y.x ? 1 : 0, x.y > y.y ? 1 : 0, x.z > y.z ? 1 : 0)); }

        public static bool GetAABBClosestSurfacePoint(Vector3 min, Vector4 ax, Vector4 ay, Vector4 az, Vector3 position, out Vector3 normal, out Vector3 closestPosition)
        {
            var o = min;
            position = position - o;
            var xyz = new Vector3(
                position.Dot(ax.GetDirection()),
                position.Dot(ay.GetDirection()),
                position.Dot(az.GetDirection())
            );
            var projOnAxis = xyz;
            var axisLength = new Vector3(ax.w, ay.w, az.w);
            var side = step(axisLength * 0.5f, projOnAxis); // >0.5 => 1 | <0.5 => 0
            var left = new Vector3(1, 1, 1) - side * 2;
            var right = projOnAxis - new Vector3(side.x * axisLength.x, side.y * axisLength.y, side.z * axisLength.z);
            var signedDist = new Vector3(left.x * right.x, left.y * right.y, left.z * right.z);
            bool inside = signedDist is { x: > 0, y: > 0, z: > 0 };
            if (inside)
            {
                var dst = signedDist.x;
                var axis = ax;
                var sideFlag = side.x;
                var axisIndex = 0;
                if (signedDist.y < dst)
                {
                    dst = signedDist.y;
                    axisIndex = 1;
                    sideFlag = side.y;
                    axis = ay;
                }
                if (signedDist.z < dst)
                {
                    dst = signedDist.z;
                    sideFlag = side.z;
                    axisIndex = 2;
                    axis = az;
                }

                normal = sideFlag == 1 ? xyz : -xyz;
                var offset = (projOnAxis[axisIndex] - sideFlag * axis.w);
                closestPosition = o + position - xyz * offset;
                return true;
            }
            else
            {
                normal = closestPosition = default;
                return false;
            }
        }

        public static bool HitCapsule(Vector3 pA, Vector3 pB, float radius, Vector3 position, Vector3 velocity, float time, out Vector3 normal, out float reachTime)
        {
            var ab = pB - pA;
            var ao = position - pA;
            var d = velocity.normalized;
            var vScale = velocity.magnitude / d.magnitude;
            var ab_d = ab.Dot(d);
            var ab_ao = ab.Dot(ao);
            var ab_ab = ab.Dot(ab);
            var m = ab_d / ab_ab;
            var n = ab_ao / ab_ab;

            var q = d - (ab * m);
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
                    var p = position + (d * atime);
                    normal = (p - pA).normalized;
                    reachTime = atime / vScale;
                }
                else
                {
                    var p = position + (d * btime);
                    normal = (p - pB).normalized;
                    reachTime = btime / vScale;
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
                var p = position + (d * t);
                var k1 = pA + (ab * tK);
                reachTime = t / vScale;
                normal = (p - k1).normalized;
                return false; // fix me: disabled for now
            }

            return true;
        }

        public static bool InsideCapsule(Vector3 pA, Vector3 pB, float radius, Vector3 position)
        {
            var o = pA;
            var d = (pB - pA).normalized;
            var w = (pB - pA).magnitude;
            position = position - o;
            var proj = position.Dot(d); // p project on AB
            if (proj < -radius || proj > w + radius)
            {
                return false;
            }
            var r2 = radius * radius;
            if (proj >= 0 && proj <= w)
            {
                // on cylinder
                var dist2 = position.Dot(position) - proj * proj;
                return dist2 < r2;
            }
            if (proj >= -radius && proj < 0)
            {
                // on pA
                return position.Dot(position) < r2;
            }
            if (proj <= w + radius)
            {
                // on pB
                var v = position - (d * w);
                return v.Dot(v) < r2;
            }
            return false;
        }

        public static bool GetCapsuleClosestSurfacePoint(Vector3 pA, Vector3 pB, float radius, Vector3 position, out Vector3 normal, out Vector3 closestPosition)
        {
            var o = pA;
            var d = (pB - pA).normalized;
            var w = (pB - pA).magnitude;
            position = position - o;
            var proj = position.Dot(d); // p -> AB
            if (proj < -radius || proj > w + radius)
            {
                normal = closestPosition = default;
                return false;
            }
            var r2 = radius * radius;
            if (proj >= 0 && proj <= w)
            {
                // cylinder
                var dist2 = position.Dot(position) - proj * proj;
                if (dist2 < r2)
                {
                    var q = d * proj;
                    normal = (position - q).normalized;
                    closestPosition = o + q + normal * radius;
                    return true;
                }
                else
                {
                    normal = closestPosition = default;
                    return false;
                }
            }
            if (proj >= -radius && proj < 0)
            {
                // sphere pA
                var c2p = position;
                if (c2p.Dot(c2p) < r2)
                {
                    normal = c2p.normalized;
                    closestPosition = o + radius * normal;
                    return true;
                }
                else
                {
                    normal = closestPosition = default;
                    return false;
                }
            }
            if (proj <= w + radius)
            {
                // sphere pB
                var c = (d * w);
                var c2p = position - c;
                if (c2p.Dot(c2p) < r2)
                {
                    normal = c2p.normalized;
                    closestPosition = o + c + radius * normal;
                    return true;
                }
                else
                {
                    normal = closestPosition = default;
                    return false;
                }
            }

            normal = closestPosition = default;
            return false;
        }
    }
}
