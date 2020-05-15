using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using MathNet.Numerics.LinearAlgebra;

namespace PathTracer
{
  public class Ray
  {
    public Vector3 d { get; set; }
    public Vector3 o { get; set; }

    public Ray(Vector3 origin, Vector3 direction)
    {
      d = direction.Normalize();
      o = origin;
    }

    /// <summary>
    /// Returns point on ray with given parameter t
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector3 Point(double t)
    {
      return o + d * t;
    }

    /// <summary>
    /// Generates new (normalized) ray from origin towards point
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public static Ray Generate(Vector3 origin, Vector3 point)
    {
      return new Ray(origin, point - origin);
    }
  }
}
