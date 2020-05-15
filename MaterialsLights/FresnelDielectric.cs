using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
  public class FresnelDielectric
  {
    public double EtaI { get; }
    public double EtaT { get; }
    public FresnelDielectric(double etaI, double etaT)
    {
      EtaI = etaI;
      EtaT = etaT;
    }
    // FresnelDielectric Public Methods
    public Spectrum Evaluate(double cosThetaI)
    {
      var etaI = EtaI;
      var etaT = EtaT;
      if (etaI == 0 && etaT == 0)
      {
        // special case when we don't want Fresnel, e.g. always want perfect reflection (mirror)
        return Spectrum.Create(1);
      }

      cosThetaI = cosThetaI.Clamp(-1, 1);
      // Potentially swap indices of refraction
      bool entering = cosThetaI > 0;
      if (!entering)
      {
        var t = etaI;
        etaI = etaT;
        etaT = t;
        cosThetaI = Math.Abs(cosThetaI);
      }

      // Compute _cosThetaT_ using Snell's law
      var sinThetaI = Math.Sqrt(Math.Max(0, 1 - cosThetaI * cosThetaI));
      var sinThetaT = etaI / etaT * sinThetaI;

      // Handle total public reflection
      if (sinThetaT >= 1) return Spectrum.Create(1);
      var cosThetaT = Math.Sqrt(Math.Max(0, 1 - sinThetaT * sinThetaT));
      var Rparl = ((etaT * cosThetaI) - (etaI * cosThetaT)) /
                    ((etaT * cosThetaI) + (etaI * cosThetaT));
      var Rperp = ((etaI * cosThetaI) - (etaT * cosThetaT)) /
                    ((etaI * cosThetaI) + (etaT * cosThetaT));
      return Spectrum.Create((Rparl * Rparl + Rperp * Rperp) / 2);

    }

  }

}
