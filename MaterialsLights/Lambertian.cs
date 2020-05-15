using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PathTracer.Samplers;

namespace PathTracer
{
    public class Lambertian : BxDF
    {
        private Spectrum kd;
        public Lambertian(Spectrum r)
        {
            kd = r;
        }

        public override Spectrum f(Vector3 wo, Vector3 wi)
        {
            return kd * Utils.PiInv;
        }

        public override (Spectrum, Vector3, double) Sample_f(Vector3 wo)
        {
            double theta = Math.Asin(Math.Sqrt(ThreadSafeRandom.NextDouble()));
            double phi = 2 * Math.PI * ThreadSafeRandom.NextDouble();
            Vector3 wi = Utils.SphericalDirection(Math.Sin(theta), Math.Cos(theta), phi);
            return (f(wo, wi), wi, Pdf(wo, wi));
        }

        public override double Pdf(Vector3 wo, Vector3 wi)
        {
            //return Utils.CosTheta(wi) * Utils.PiInv;
            //return !Utils.SameHemisphere(wo, wi) ? Utils.AbsCosTheta(wi) * Utils.PiInv : 0;
            return Utils.AbsCosTheta(wi) * Utils.PiInv;
        }
    }
}
