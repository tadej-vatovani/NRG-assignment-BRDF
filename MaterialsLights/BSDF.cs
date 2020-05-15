using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PathTracer.Samplers;

namespace PathTracer
{
  public class BSDF
  {
    private List<BxDF> bxdfs = new List<BxDF>(5);

    public bool IsSpecular => bxdfs.All(x => x.IsSpecular);

    public BSDF Add(BxDF b)
    {
      bxdfs.Add(b);
      return this;
    }

    public Spectrum f(Vector3 woW, Vector3 wiW, SurfaceInteraction si)
    {

      var wi = WorldToLocal(wiW, si);
      var wo = WorldToLocal(woW, si);
      var f = Spectrum.ZeroSpectrum;
      if (Math.Abs(wo.z) < Renderer.Epsilon)
        return f;

      foreach(var bx in bxdfs)
      {
        f.AddTo(bx.f(wo, wi));
      }
      return f;
    }

    public (Spectrum, Vector3, double, bool) Sample_f(Vector3 woW, SurfaceInteraction si)
    {

      var woL = WorldToLocal(woW, si);
      if (Math.Abs(woL.z) < Renderer.Epsilon) 
        return (Spectrum.ZeroSpectrum, Vector3.ZeroVector, 0, false);

      // randomly choose bxdf 
      int comp = (int)Math.Floor(ThreadSafeRandom.NextDouble() * bxdfs.Count);
      var bxdf = bxdfs[comp];

      // Sample chosen _BxDF_
      (Spectrum f, Vector3 wiL, double pdf) = bxdf.Sample_f(woL);

      if (pdf < Renderer.Epsilon) {
                Console.WriteLine("Zero pfd");
        return (Spectrum.ZeroSpectrum, Vector3.ZeroVector, 0, false);
      }

      // Compute overall PDF & f with all BxDFs
      foreach (var bx in bxdfs.Where(x => x != bxdf))
      {
        pdf += bx.Pdf(woL, wiL);
        f.AddTo(bx.f(woL, wiL));
      }
      pdf /= bxdfs.Count;

      var wiW = LocalToWorld(wiL, si);
      return (f, wiW, pdf, bxdf.IsSpecular);
    }

    public double Pdf(Vector3 woW, Vector3 wiW, SurfaceInteraction si)
    {
      Vector3 wo = WorldToLocal(woW, si);
      Vector3 wi = WorldToLocal(wiW, si);
      if (wo.z == 0)
        return 0;
      return bxdfs.Average(x => x.Pdf(wo, wi));
    }

    private Vector3 WorldToLocal(Vector3 v, SurfaceInteraction si)
    {
      return new Vector3(Vector3.Dot(v, si.Dpdu), Vector3.Dot(v, si.Dpdv), Vector3.Dot(v, si.Normal));
    }
    private Vector3 LocalToWorld(Vector3 v, SurfaceInteraction si)
    {
      return new Vector3(si.Dpdu.x * v.x + si.Dpdv.x * v.y + si.Normal.x * v.z,
                      si.Dpdu.y * v.x + si.Dpdv.y * v.y + si.Normal.y * v.z,
                      si.Dpdu.z * v.x + si.Dpdv.z * v.y + si.Normal.z * v.z);
    }

  }
}
