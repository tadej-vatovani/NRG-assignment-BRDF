using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
  class Disk : Shape
  {
    private double radius;
    private double height;

    public Disk(double r, double h, Transform objectToWorld)
    {
      radius = r;
      height = h;
      ObjectToWorld = objectToWorld;
    }

    public override  (SurfaceInteraction, double) Sample()
    {
      (double x, double y) = Samplers.UniformSampleDisk();

      var pObj = new Vector3(x* radius, y* radius, height);

      var n =new Vector3(0, 0, 1);
      var dpdu = new Vector3(-pObj.y, pObj.x, 0);
      double pdf = 1 / Area();
      return (ObjectToWorld.Apply(new SurfaceInteraction(pObj, n, Vector3.ZeroVector, dpdu, this)), pdf);
    }

    public override double Area()
    {
      return Math.PI * (radius * radius);
    }

  public override (double?, SurfaceInteraction) Intersect(Ray r)
  {
      var ray = WorldToObject.Apply(r);

      // Compute plane intersection for disk

      // Reject disk intersections for rays parallel to the disk's plane
      if (ray.d.z == 0)
        return (null, null);

      double tShapeHit = (height - ray.o.z) / ray.d.z;
      if (tShapeHit <= Renderer.Epsilon)
        return (null, null);

      // See if hit point is inside disk radii and $\phimax$
      var pHit = ray.Point(tShapeHit);
      var dist2 = pHit.x * pHit.x + pHit.y * pHit.y;
      if (dist2 > radius * radius + Renderer.Epsilon)
        return (null, null);

      // Refine disk intersection point
      pHit.z = height;

      var dpdu = new Vector3(-pHit.y, pHit.x, 0);

      var si = new SurfaceInteraction(pHit, new Vector3(0, 0, 1), -ray.d,dpdu, this);
      return (tShapeHit, ObjectToWorld.Apply(si));
    }
  }


}
