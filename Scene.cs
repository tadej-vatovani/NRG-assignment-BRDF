using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
    public class Scene
    {

        /// <summary>
        /// Elements contains all scene elements (lights and shapes)
        /// </summary>
        public List<Primitive> Elements { get; set; } = new List<Primitive>();
        public List<Light> Lights => Elements.Where(x => x is Light).Cast<Light>().ToList();
        public Vector3 CameraOrigin { get; set; } = new Vector3(0, 0, 0);
        public double AspectRatio { get; set; } = 16.0 / 9;

        public double ImagePlaneWidth { get; set; } = 16;
        public double ImagePlaneHeight => ImagePlaneWidth / AspectRatio;
        public double ImagePlaneDistance { get; set; } = 8;
        public double ImagePlaneVerticalOffset { get; set; } = 0;

        /// <summary>
        /// Finds closest intersection of ray with scene
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public (double?, SurfaceInteraction) Intersect(Ray r)
        {
            double? mint = null;
            SurfaceInteraction si = null;
            foreach (var sh in Elements)
            {
                (double? t, SurfaceInteraction shi) = sh.Intersect(r);
                if (t.HasValue && t > Renderer.Epsilon && (!mint.HasValue || t < mint))
                {
                    mint = t.Value;
                    si = shi;
                }
            }

            return (mint, si);
        }
        /// <summary>
        /// Returns true if points are not ocludded (no element is between them)
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public bool Unoccluded(Vector3 p1, Vector3 p2)
        {
            Ray r = new Ray(p1, p2 - p1);
            (double? t, SurfaceInteraction it) = Intersect(r);

            if (!t.HasValue || (p2 - it.Point).Length() < Renderer.Epsilon)
                return true;

            return false;
        }


        /// <summary>
        /// Generate Cornell Box Geometry
        /// </summary>
        /// <returns></returns>
        public static Scene CornellBox()
        {
            var s = new Scene()
            {
                CameraOrigin = new Vector3(278, 274.4, -800),
                AspectRatio = 1.0 / 1.0,
                ImagePlaneWidth = 5.5
            };

            Shape el;

            // floor
            el = new Quad(556.0, 559.2, Transform.Translate(556.0 / 2, 0, 559.2 / 2).A(Transform.RotateX(-90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);

            // celing
            el = new Quad(556.0, 559.2, Transform.Translate(556.0 / 2, 548.8, 559.2 / 2).A(Transform.RotateX(90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);

            // back
            el = new Quad(556.0, 548.8, Transform.Translate(556.0 / 2, 548.8 / 2, 559.2).A(Transform.RotateX(180)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.White)));
            s.Elements.Add(el);

            //right
            el = new Quad(559.2, 548.8, Transform.Translate(556.0, 548.8 / 2, 559.2 / 2).A(Transform.RotateY(90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Green)));
            s.Elements.Add(el);

            //left
            el = new Quad(559.2, 548.8, Transform.Translate(0, 548.8 / 2, 559.2 / 2).A(Transform.RotateY(-90)));
            el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Red)));
            s.Elements.Add(el);

            s.Elements.Add(new DiffuseAreaLight(new Disk(80, 0.1, Transform.Translate(278, 548, 280).A(Transform.RotateX(90))), Spectrum.Create(1), 20));
            //s.Elements.Add(new DiffuseAreaLight(new Disk(80, 0.1, Transform.Translate(278, 0, 280).A(Transform.RotateX(-90))), Spectrum.Create(1), 20));
            //s.Elements.Add(new RectangleLight(new Quad(160, 80, Transform.Translate(278, 278, 350).A(Transform.RotateX(135))), Spectrum.Create(1), 20));

            el = new Sphere(100, Transform.Translate(150, 100, 420));
            //el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Blue)));
            el.BSDF.Add(new SpecularReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White), 1, 1.5));
            el.BSDF.Add(new MERL("pink-fabric", true));
            s.Elements.Add(el);

            el = new Sphere(100, Transform.Translate(300, 100, 300));
            //el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Blue)));
            el.BSDF.Add(new SpecularReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White), 1, 1.5));
            el.BSDF.Add(new MERL("beige-fabric", true));
            s.Elements.Add(el);

            el = new Sphere(100, Transform.Translate(400, 100, 230));
            //el.BSDF.Add(new MicrofacetReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White), 1.5, 1, 0.05));
            //el.BSDF.Add(new SpecularReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White),0,0));

            //el.BSDF.Add(new SpecularTransmission(Spectrum.ZeroSpectrum.FromRGB(Color.White), 1, 1.5));
            //el.BSDF.Add(new Lambertian(Spectrum.ZeroSpectrum.FromRGB(Color.Yellow)));
            el.BSDF.Add(new SpecularReflection(Spectrum.ZeroSpectrum.FromRGB(Color.White), 1, 1.5));
            el.BSDF.Add(new MERL("yellow-matte-plastic", true));
            s.Elements.Add(el);


            return s;

        }
    }
}
