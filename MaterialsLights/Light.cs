using System;
using static PathTracer.Samplers;

namespace PathTracer
{
  public abstract class Light : Primitive
  {
    public abstract (Spectrum, Vector3, double, Vector3) Sample_Li(SurfaceInteraction source);

    public abstract Spectrum L(SurfaceInteraction si, Vector3 w);

    public abstract double Pdf_Li(SurfaceInteraction si, Vector3 wi);

    public static Spectrum UniformSampleOneLight(SurfaceInteraction it, Scene s)
    {
      // Randomly choose a single light to sample, _light_
      int nLights = s.Lights.Count;
      if (nLights == 0)
        return Spectrum.ZeroSpectrum;

      int lightNum = (int)Math.Floor(ThreadSafeRandom.NextDouble() * nLights);
      double lightPdf = 1 / (double)nLights;

      var light = s.Lights[lightNum];
      return light.EstimateLWithIS(it, s) / lightPdf;
    }

    private Spectrum EstimateLWithIS(SurfaceInteraction it, Scene s)
    {
      // Sample light source with importance sampling
      (Spectrum Li, Vector3 wi, double lightPdf, Vector3 lightPt) = Sample_Li(it);

      var shp = it.Obj as Shape;
      Spectrum f = shp.BSDF.f(it.Wo, wi, it) * Vector3.AbsDot(wi, it.Normal);
         
      if (!s.Unoccluded(it.Point, lightPt))
      {
        return Spectrum.ZeroSpectrum;
      }

      return f * Li / lightPdf;
    }
  }
}