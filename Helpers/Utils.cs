using System;

namespace PathTracer
{
  public static class Utils
  {
    public const double PiInv = 1.0 / Math.PI;
    /// <summary>
    /// Solve quadratic equation
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <param name="C"></param>
    /// <returns></returns>
    public static (bool, double, double) Quadratic(double A, double B, double C)
    {
      // Find quadratic discriminant
      double discrim = B * B - 4.0 * A * C;
      if (discrim < 0.0)
        return (false, 0, 0);
      double rootDiscrim = Math.Sqrt(discrim);

      // Compute quadratic _t_ values
      double q;
      if (B < 0)
        q = -.5 * (B - rootDiscrim);
      else
        q = -.5 * (B + rootDiscrim);
      double t0 = q / A;
      double t1 = C / q;
      if (t0 > t1)
      {
        var x = t0;
        t0 = t1;
        t1 = x;
      }
      return (true, t0, t1);
    }

    /// <summary>
    /// Degrees to radians
    /// </summary>
    /// <param name="deg"></param>
    /// <returns></returns>
    static double Radians(double deg) { return (Math.PI / 180) * deg; }

    /// <summary>
    /// Parametric linear interpolation
    /// </summary>
    /// <param name="t"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static double Lerp(double t, double v1, double v2) { return (1 - t) * v1 + t * v2; }

    /// <summary>
    /// Linear interpolation within array of x and vals
    /// </summary>
    /// <param name="w"></param>
    /// <param name="i"></param>
    /// <param name="x"></param>
    /// <param name="vals"></param>
    /// <returns></returns>
    public static double Lerp(double w, int i, double[] x, double[] vals)
    {
      return Lerp((w - x[i]) / (x[i + 1] - x[i]), vals[i], vals[i + 1]);
    }

    /// <summary>
    /// MIS power heuristic
    /// </summary>
    /// <param name="nf"></param>
    /// <param name="fPdf"></param>
    /// <param name="ng"></param>
    /// <param name="gPdf"></param>
    /// <returns></returns>
    public static double PowerHeuristic(int nf, double fPdf, int ng, double gPdf)
    {
      double f = nf * fPdf, g = ng * gPdf;
      return (f * f) / (f * f + g * g);
    }

    /// <summary>
    /// Vectors (in local coords) are on the same hemisphere
    /// </summary>
    /// <param name="w"></param>
    /// <param name="wp"></param>
    /// <returns></returns>
    public static bool SameHemisphere(Vector3 w, Vector3 wp)
    {
      return w.z * wp.z > 0;
    }

    /// <summary>
    /// Clamp value between low and high
    /// </summary>
    /// <param name="val"></param>
    /// <param name="low"></param>
    /// <param name="high"></param>
    /// <returns></returns>
    public static double Clamp(this double val, double low, double high)
    {
      if (val < low)
        return low;
      else if (val > high)
        return high;
      else
        return val;
    }

    /// <summary>
    /// Reflect vector around normal
    /// </summary>
    /// <param name="wo"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public static Vector3 Reflect(Vector3 wo, Vector3 n)
    {
      return -wo + n * (2 * Vector3.Dot(wo, n));
    }

    public static (bool, Vector3) Refract(Vector3 wi, Vector3 n, double eta)
    {
        // Compute $\cos \theta_\roman{t}$ using Snell's law
        var cosThetaI = Vector3.Dot(n, wi);
        var sin2ThetaI = Math.Max(0, 1 - cosThetaI * cosThetaI);
        var sin2ThetaT = eta * eta * sin2ThetaI;

        // Handle total internal reflection for transmission
        if (sin2ThetaT >= 1)
          return (false, Vector3.ZeroVector);
        var cosThetaT = Math.Sqrt(1 - sin2ThetaT);
        Vector3 r = eta * -wi + (eta * cosThetaI - cosThetaT) * n;
        return (true, r);
    }

    #region Angle functions of vectors with respect to spherical coordinates

    /// <summary>
    /// Cosine theta in local coords (is z)
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double CosTheta(Vector3 w) { return w.z; }
    /// <summary>
    /// Cosine theta squared in local coords (is z^2)
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double Cos2Theta(Vector3 w) { return w.z * w.z; }
    /// <summary>
    /// Abs. value of cosine theta in local coords
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double AbsCosTheta(Vector3 w) { return Math.Abs(w.z); }

    /// <summary>
    /// Sin theta squared in local coords (1-cos^2)
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double Sin2Theta(Vector3 w) { return Math.Max(0, 1 - Cos2Theta(w)); }
    /// <summary>
    /// Sin theta in local coords
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double SinTheta(Vector3 w) { return Math.Sqrt(Sin2Theta(w)); }

    /// <summary>
    /// Tan theta in local coords
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double TanTheta(Vector3 w) { return SinTheta(w) / CosTheta(w); }

    /// <summary>
    /// Tan theta squared in local coords
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double Tan2Theta(Vector3 w) { return Sin2Theta(w) / Cos2Theta(w); }

    /// <summary>
    /// Cos phi in local coords
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double CosPhi(Vector3 w)
    {
      double sinTheta = SinTheta(w);
      return (sinTheta == 0) ? 1 : (w.x / sinTheta).Clamp(-1, 1);
    }
    /// <summary>
    /// Sin phi in local coords
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double SinPhi(Vector3 w)
    {
      double sinTheta = SinTheta(w);
      return (sinTheta == 0) ? 0 : (w.y / sinTheta).Clamp(-1, 1);
    }
    /// <summary>
    /// Cos phi squared in local coords
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double Cos2Phi(Vector3 w) { return CosPhi(w) * CosPhi(w); }

    /// <summary>
    /// Sin phi squared in local coords
    /// </summary>
    /// <param name="w"></param>
    /// <returns></returns>
    public static double Sin2Phi(Vector3 w) { return SinPhi(w) * SinPhi(w); }

    /// <summary>
    /// Spherical to cartesian coordinates
    /// </summary>
    /// <param name="sinTheta"></param>
    /// <param name="cosTheta"></param>
    /// <param name="phi"></param>
    /// <returns></returns>
    public static Vector3 SphericalDirection(double sinTheta, double cosTheta, double phi)
    {
      return new Vector3(sinTheta * Math.Cos(phi), sinTheta * Math.Sin(phi), cosTheta);
    }

    #endregion
  }

}
