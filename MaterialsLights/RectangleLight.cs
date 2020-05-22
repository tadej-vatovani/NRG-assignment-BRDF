using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    class RectangleLight : Light
    {
        Quad quad;
        Spectrum Lemit;
        bool onesided;

        public RectangleLight(Quad q, Spectrum l, double intensity = 1, bool onesided = true)
        {
            quad = q;
            Lemit = l * intensity;
            this.onesided = onesided;
        }

        public override (double?, SurfaceInteraction) Intersect(Ray r)
        {
            (double? t, SurfaceInteraction si) = quad.Intersect(r);
            if (si != null)
                si.Obj = this;
            return (t, si);
        }

        public override (SurfaceInteraction, double) Sample()
        {
            return quad.Sample();
        }

        /// <summary>
        /// Samples light ray at source point
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Spectrum, wi, pdf, point on light</returns>
        public override (Spectrum, Vector3, double, Vector3) Sample_Li(SurfaceInteraction source)
        {
            (SurfaceInteraction pShape, double pdf) = quad.Sample(source);

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
            if (onesided)
                return (Vector3.Dot(intr.Normal, w) > 0) ? Lemit : Spectrum.ZeroSpectrum;
            else
                return Lemit;
        }


        public override double Pdf_Li(SurfaceInteraction si, Vector3 wi)
        {
            return quad.Pdf(si, wi);
        }

    }
}
