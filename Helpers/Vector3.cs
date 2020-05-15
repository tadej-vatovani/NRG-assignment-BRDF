using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    public class Vector3
    {
        Vector<double> inner;

        public static Vector3 ZeroVector => new Vector3(0, 0, 0);


        public Vector3(double x, double y, double z)
        {
            inner = Vector<double>.Build.Dense(new[] { x, y, z });
        }
        private Vector3(Vector<double> i)
        {
            inner = i;
        }

        public double x { get { return inner[0]; } set { inner[0] = value; } }
        public double y { get { return inner[1]; } set { inner[1] = value; } }
        public double z { get { return inner[2]; } set { inner[2] = value; } }

        public static Vector3 operator *(Vector3 v, double d)
        {
            return new Vector3(d * v.inner);
        }
        public static Vector3 operator *(double d, Vector3 v)
        {
            return new Vector3(d * v.inner);
        }
        /// <summary>
        /// Returns new vector as Cross product of v1 and v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 Cross(Vector3 v1, Vector3 v2)
        {
            return new Vector3(Vector3D.OfVector(v1.inner).CrossProduct(Vector3D.OfVector(v2.inner)).ToVector());
        }
        /// <summary>
        /// Rotate vector v along axis
        /// </summary>
        /// <param name="v"></param>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Vector3 Rotate(Vector3 v, Vector3 axis, double angle)
        {
            double temp;
            double cos_ang = Math.Cos(angle);
            double sin_ang = Math.Sin(angle);
            Vector3 result = v * cos_ang;

            temp = Dot(result, axis);
            temp = temp * (1.0 - cos_ang);

            result += axis * temp;

            Vector3 cross = Cross(axis, v);

            return result + (cross * sin_ang);
        }
        /// <summary>
        /// Returns new vector3 as sum of parameters
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.inner + v2.inner);
        }

        /// <summary>
        /// Returns new vector as sum of v1 and v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 operator +(Vector3 v1, double[] v2)
        {
            return new Vector3(v1.inner[0] + v2[0], v1.inner[1] + v2[1], v1.inner[2] + v2[2]);
        }

        /// <summary>
        /// returns new vector as v1-v2
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.inner - v2.inner);
        }

        public static double AbsDot(Vector3 v1, Vector3 v2)
        {
            return Math.Abs(v1.inner.DotProduct(v2.inner));
        }

        public static double Dot(Vector3 v1, Vector3 v2)
        {
            return v1.inner.DotProduct(v2.inner);
        }

        public double LengthSquared()
        {
            return inner.DotProduct(inner);
        }

        public double Length()
        {
            return Math.Sqrt(inner.DotProduct(inner));
        }

        public double[] ToArray()
        {
            return inner.ToArray();
        }
        public Vector3 Clone()
        {
            return new Vector3(inner.Clone());
        }

        /// <summary>
        /// Normalizes this vector!
        /// </summary>
        /// <returns>this, normalized</returns>
        public Vector3 Normalize()
        {
            var t = Length();
            if (t != 0)
                inner = inner / t;
            return this;
        }

        /// <summary>
        /// Unary negation, returns new vector
        /// </summary>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vector3 operator -(Vector3 v1)
        {
            return new Vector3(-v1.inner);
        }

        public override string ToString()
        {
            return string.Format("({0:0.0000}, {1:0.0000}, {2:0.0000}) ", inner[0], inner[1], inner[2]);
        }
    }

    public static class Vector3Extensions
    {
        /// <summary>
        /// Change normal towards v. Works for any coords
        /// </summary>
        /// <param name="n"></param>
        /// <param name="v"></param>
        /// <returns>New vector if normal needs to be reversed</returns>
        public static Vector3 Faceforward(this Vector3 n, Vector3 v)
        {
            var t = Vector3.Dot(n, v);
            if (t < 0)
                return -n;
            return n;
        }

    }
}
