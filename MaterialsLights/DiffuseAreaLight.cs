using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
  class DiffuseAreaLight : Light
  {
    Shape shape;
    Spectrum Lemit;

    public DiffuseAreaLight(Shape s, Spectrum l, double intensity = 1)
    {
      shape = s;
      Lemit = l * intensity;
    }

    public override (double?, SurfaceInteraction) Intersect(Ray r)
    {
      (double ?t, SurfaceInteraction si) = shape.Intersect(r);
      if (si != null)
        si.Obj = this;
      return (t, si);
    }

    public override (SurfaceInteraction, double) Sample()
    {
      return shape.Sample();
    }

    /// <summary>
    /// Samples light ray at source point
    /// </summary>
    /// <param name="source"></param>
    /// <returns>Spectrum, wi, pdf, point on light</returns>
    public override (Spectrum, Vector3, double, Vector3) Sample_Li(SurfaceInteraction source)
    {
      (SurfaceInteraction pShape, double pdf) = shape.Sample(source);

      if (pdf == 0 || (pShape.Point - source.Point).LengthSquared() < Renderer.Epsilon)
      {
        return (Spectrum.ZeroSpectrum, Vector3.ZeroVector, 0, Vector3.ZeroVector);
      }

      var wi = (pShape.Point - source.Point).Normalize();
      var Li = L(pShape, -wi);
      return (Li, wi, pdf, pShape.Point);
    }


    public override Spectrum L(SurfaceInteraction intr, Vector3 w)
    {
      return (Vector3.Dot(intr.Normal, w) > 0) ? Lemit : Spectrum.ZeroSpectrum;
    }


    public override double Pdf_Li(SurfaceInteraction si, Vector3 wi)
    {
      return shape.Pdf(si, wi);
    }

  }
}
