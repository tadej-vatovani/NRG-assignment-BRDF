using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathTracer
{
  public abstract class BxDF
  {
    public virtual bool IsSpecular => false;

    /// <summary>
    /// f(wo,wi) in local coords
    /// </summary>
    /// <param name="wo"></param>
    /// <param name="wi"></param>
    /// <returns></returns>
    public abstract Spectrum f(Vector3 wo, Vector3 wi) ;

    /// <summary>
    /// Sample wi direction according to wo in local coords
    /// </summary>
    /// <param name="woL"></param>
    /// <returns></returns>
    public abstract (Spectrum, Vector3, double) Sample_f(Vector3 woL);

    /// <summary>
    /// pdf(wo,wi) in local coords
    /// </summary>
    /// <param name="wo"></param>
    /// <param name="wi"></param>
    /// <returns></returns>
    public abstract double Pdf(Vector3 wo, Vector3 wi);

  }

}
