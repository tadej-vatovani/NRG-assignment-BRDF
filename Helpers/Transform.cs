using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace PathTracer
{
  public class Transform
  {
    Matrix<double> m, mInv;
    public Transform()
    {
      m = Matrix<double>.Build.DenseIdentity(4, 4);
      mInv = m.Inverse();
    }
    public Transform(Matrix<double> mat)
    {
      m = mat.Clone();
      mInv = m.Inverse();
    }

    public SurfaceInteraction Apply(SurfaceInteraction si)
    {
      var p = ApplyPoint(si.Point);
      var n = ApplyNormal(si.Normal);
      var w = ApplyVector(si.Wo);
      var dpdu = ApplyVector(si.Dpdu);
      return new SurfaceInteraction(p, n, w, dpdu, si.Obj);
    }

    public static Transform Translate(double x, double y, double z)
    {
      var m = Matrix<double>.Build.DenseIdentity(4, 4);
      m[0, 3] = x;
      m[1, 3] = y;
      m[2, 3] = z;
      return new Transform(m);
    }

    /// <summary>
    /// 4x4 rotation matrix
    /// </summary>
    /// <param name="phi">angle in degrees</param>
    /// <returns></returns>
    public static Transform RotateX(double phi)
    {
      var r = Matrix3D.RotationAroundXAxis(Angle.FromDegrees(phi));

      var m = Matrix<double>.Build.DenseIdentity(4, 4);
      m.SetSubMatrix(0, 0, r);
      return new Transform(m);
    }

    /// <summary>
    /// 4x4 rotation matrix
    /// </summary>
    /// <param name="phi">angle in degrees</param>
    /// <returns></returns>
    public static Transform RotateY(double phi)
    {
      var r = Matrix3D.RotationAroundYAxis(Angle.FromDegrees(phi));

      var m = Matrix<double>.Build.DenseIdentity(4, 4);
      m.SetSubMatrix(0, 0, r);
      return new Transform(m);
    }

    /// <summary>
    /// 4x4 rotation matrix
    /// </summary>
    /// <param name="phi">angle in degrees</param>
    /// <returns></returns>
    public static Transform RotateZ(double phi)
    {
      var r = Matrix3D.RotationAroundZAxis(Angle.FromDegrees(phi));

      var m = Matrix<double>.Build.DenseIdentity(4, 4);
      m.SetSubMatrix(0, 0, r);
      return new Transform(m);
    }

    /// <summary>
    /// appends - multiplies this with new matrix 
    /// </summary>
    /// <param name="with">matrix to append</param>
    /// <returns>this</returns>
    public Transform A(Transform a)
    {
      m=m.Multiply(a.m);
      mInv = a.mInv.Multiply(mInv);

      return this;
    }

    public Transform Inverse()
    {
      var t = new Transform()
      {
        m = mInv,
        mInv = m
      };
      return t;
    }

    public Ray Apply(Ray r)
    {
      var o = ApplyPoint(r.o);
      var d = ApplyVector(r.d);
      return new Ray(o, d);
    }

    public Vector3 ApplyPoint(Vector3 pt)
    {
      if (pt == null)
        return null;
      double x = pt.x, y = pt.y, z = pt.z;
      double xp = m[0,0] * x + m[0,1] * y + m[0,2] * z + m[0,3];
      double yp = m[1,0] * x + m[1,1] * y + m[1,2] * z + m[1,3];
      double zp = m[2,0] * x + m[2,1] * y + m[2,2] * z + m[2,3];
      double wp = m[3,0] * x + m[3,1] * y + m[3,2] * z + m[3,3];

      return new Vector3(xp/wp, yp/wp, zp/wp);
    }

    public Vector3 ApplyVector(Vector3 v)
    {
      if (v == null)
        return null;
      double x = v.x, y = v.y, z = v.z;
      return new Vector3(m[0, 0] * x + m[0, 1] * y + m[0, 2] * z,
                        m[1, 0] * x + m[1, 1] * y + m[1, 2] * z,
                        m[2, 0] * x + m[2, 1] * y + m[2, 2] * z);
    }
    public Vector3 ApplyNormal(Vector3 n)
    {
      if (n == null)
        return null;
      double x = n.x, y = n.y, z = n.z;
      return new Vector3(mInv[0, 0] * x + mInv[1, 0] * y + mInv[2, 0] * z,
                          mInv[0, 1] * x + mInv[1, 1] * y + mInv[2, 1] * z,
                          mInv[0, 2] * x + mInv[1, 2] * y + mInv[2, 2] * z);
    }


  }
}
