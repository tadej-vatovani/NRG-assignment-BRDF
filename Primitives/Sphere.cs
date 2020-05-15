using System;

namespace PathTracer
{
  class Sphere : Shape
  {
    public double Radius { get; set; }
    public Sphere(double radius, Transform objectToWorld)
    {
      Radius = radius;
      ObjectToWorld = objectToWorld;
    }

    public override (double?, SurfaceInteraction) Intersect(Ray ray)
    {
      Ray r = WorldToObject.Apply(ray);

      // Compute quadratic sphere coefficients

      // Initialize _double_ ray coordinate values
      double a = r.d.x * r.d.x + r.d.y * r.d.y + r.d.z * r.d.z;
      double b = 2 * (r.d.x * r.o.x + r.d.y * r.o.y + r.d.z * r.o.z);
      double c = r.o.x * r.o.x + r.o.y * r.o.y + r.o.z * r.o.z - Radius * Radius;

      // Solve quadratic equation for _t_ values
      (bool s, double t0, double t1) = Utils.Quadratic(a, b, c);

      if (!s) return (null, null);

      // Check quadric shape _t0_ and _t1_ for nearest intersection
      if (t1 <= 0)
        return (null, null);

      double tShapeHit = t0;
      if (tShapeHit <= Renderer.Epsilon)
      {
        tShapeHit = t1;
      }

      // Compute sphere hit position and $\phi$
      var pHit = r.Point(tShapeHit);
      var dpdu = new Vector3(-pHit.y, pHit.x, 0);
      var si = new SurfaceInteraction(pHit, pHit.Clone().Normalize(), -r.d, dpdu, this);

      return (tShapeHit, ObjectToWorld.Apply(si));
    }

    public override (SurfaceInteraction, double) Sample()
    {
      var pObj = Samplers.UniformSampleSphere() * Radius;
      pObj *= Radius / pObj.Length(); // refine
      var n = pObj.Normalize();
      var dpdu = new Vector3(-pObj.y, pObj.x, 0);
      var pdf = 1 / Area();
      return (ObjectToWorld.Apply(new SurfaceInteraction(pObj, n, Vector3.ZeroVector, dpdu, this)), pdf);
    }

    public override double Area() { return 4 * Math.PI * Radius * Radius; }

    public override double Pdf(SurfaceInteraction si, Vector3 wi)
    {
      throw new NotImplementedException();
    }

  }
}
