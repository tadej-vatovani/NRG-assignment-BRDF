using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
  public abstract partial class Spectrum
  {
    public Vector<double> c;

    public static Spectrum ZeroSpectrum => Spectrum.Create(0);

    public Spectrum()
    { }
    protected Spectrum(Vector<double> v)
    {
      c = v;
    }

    public static Spectrum Create(double cValue)
    {
      return new SpectrumRGB(cValue);
    }
    public static Spectrum Create(Vector<double> v)
    {
      var s = Create(0);
      s.c = v;
      return s;
    }

    public bool IsBlack()
    {
      return c.All(x => x < Renderer.Epsilon);
    }


    public Spectrum AddTo(Spectrum v2)
    {
      c += v2.c;
      return this;
    }

    public static Spectrum operator *(Spectrum v1, Spectrum v2)
    {
      Spectrum t = (Spectrum)Activator.CreateInstance(v1.GetType());
      t.c = v1.c.PointwiseMultiply(v2.c);
      return t;
    }

    public static Spectrum operator -(Spectrum v1, Spectrum v2)
    {
      Spectrum t = (Spectrum)Activator.CreateInstance(v1.GetType());
      t.c = v1.c - v2.c;
      return t;
    }

    public static Spectrum operator *(Spectrum v1, double v2)
    {
      Spectrum t = (Spectrum)Activator.CreateInstance(v1.GetType());
      t.c = (v1.c * v2);
      return t;
    }

    public static Spectrum operator *(double v1, Spectrum v2)
    {
      Spectrum t = (Spectrum)Activator.CreateInstance(v2.GetType());
      t.c = (v1 * v2.c);
      return t;
    }

    public static Spectrum operator /(Spectrum v1, double v2)
    {
      Spectrum t = (Spectrum)Activator.CreateInstance(v1.GetType());
      t.c = v1.c / v2;
      return t;
    }

    public double Max()
    {
      return c.Max();
    }
    public abstract double[] ToRGB();
    public abstract Spectrum FromRGB(Color c);


    public Spectrum Clamp()
    {
      for (int i = 0; i < c.Count; i++)
      {
        if (c[i] < 0)
          c[i] = 0;
      }
      return this;
    }
  }
}
