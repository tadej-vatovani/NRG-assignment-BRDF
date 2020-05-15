using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PathTracer.Samplers;

namespace PathTracer
{
    class PathTracer
    {
        public Spectrum Li(Ray r, Scene s)
        {
            Spectrum L = Spectrum.ZeroSpectrum;
            Spectrum B = Spectrum.Create(1);

            int nbounces = 0;
            while (nbounces < 20)
            {
                SurfaceInteraction isect = null;

                //Get the object the ray intersected with
                isect = s.Intersect(r).Item2;

                //If the ray hasn't hit anything return 0
                if (isect == null)
                    break;

                Vector3 wo = isect.Wo;//-r.d;
                //If the ray hit a light, take its emission
                if (isect.Obj is Light)
                {
                    if (nbounces == 0)
                        L = B * isect.Le(wo);
                    break;
                }

                //Create a light ray from the intersection point and add its emission
                Spectrum Ld = Light.UniformSampleOneLight(isect, s);
                L = L.AddTo(B * Ld);


                //Get the materials value at this point
                (Spectrum f, Vector3 wi, double pr, bool specular) = (isect.Obj as Shape).BSDF.Sample_f(wo, isect);

                if (!specular)
                    B = B * f * Utils.AbsCosTheta(wi) / pr;



                //Spawn a new ray from the intersection
                r = isect.SpawnRay(wi);

                if (nbounces > 3)
                {
                    double q = 1.0 - B.Max();
                    if (ThreadSafeRandom.NextDouble() < q)
                        break;
                    B = B / (1 - q);
                }
                nbounces++;
            }

            return L;
        }
    }


}
