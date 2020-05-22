using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PathTracer.Samplers;


namespace PathTracer
{
    class MERL : BxDF
    {
        const int BRDF_SAMPLING_RES_THETA_H = 90;
        const int BRDF_SAMPLING_RES_THETA_D = 90;
        const int BRDF_SAMPLING_RES_PHI_D = 360;

        const double RED_SCALE = (1.0 / 1500.0);
        const double GREEN_SCALE = (1.15 / 1500.0);
        const double BLUE_SCALE = (1.66 / 1500.0);

        double[] brdf;

        /// <summary>
        /// Uses the supplied filepath to build MERL matrix, if relativePath is set to true, expects MERL binary files to be in BRDF folder in project root(when using debugging) 
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="absolutePath"></param>
        public MERL(String filepath, Boolean relativePath = false)
        {
            Stream f;
            if (relativePath)
                f = new FileStream("..\\\\..\\\\BRDF\\" + filepath + ".binary", FileMode.Open);
            else
                f = new FileStream(filepath + ".binary", FileMode.Open);


            int[] dims = new int[3];

            byte[] buffer = new byte[sizeof(int)];
            for (int i = 0; i < 3; i++)
            {
                if (f.Read(buffer, 0, buffer.Length) != buffer.Length)
                    throw new InvalidDataException();
                dims[i] = BitConverter.ToInt32(buffer, 0);
            }
            int n = dims[0] * dims[1] * dims[2];

            if (n != BRDF_SAMPLING_RES_THETA_H *
                 BRDF_SAMPLING_RES_THETA_D *
                 BRDF_SAMPLING_RES_PHI_D / 2)
            {
                Console.WriteLine("Dimensions don't match\n");
                f.Close();

            }
            brdf = new double[n * 3];
            buffer = new byte[n * 3 * sizeof(double)];
            if (f.Read(buffer, 0, buffer.Length) != buffer.Length)
                throw new InvalidDataException();

            for (int i = 0; i < n * 3; i++)
            {
                brdf[i] = BitConverter.ToDouble(buffer, i * sizeof(double));
            }
            f.Close();
        }

        public override Spectrum f(Vector3 wo, Vector3 wi)
        {
            (double r, double g, double b) = LookUpBRDFValue(wo, wi);
            //Console.WriteLine("{0} {1} {2}", (int)(r / RED_SCALE), (int)(g / GREEN_SCALE), (int)(b / BLUE_SCALE));
            return Spectrum.Create(Vector<double>.Build.Dense(new[] { r, g, b }));
        }

        public override double Pdf(Vector3 wo, Vector3 wi)
        {
            return 1 / (Math.PI * 4);
        }

        public override (Spectrum, Vector3, double) Sample_f(Vector3 wo)
        {
            Vector3 wi = CosineSampleHemisphere();
            return (f(wo, wi), wi, Pdf(wo, wi));

        }

        (double red_val, double green_val, double blue_val) LookUpBRDFValue(Vector3 wo, Vector3 wi)
        {

            (double thetaHalf, double fiHalf, double thetaDiff, double fiDiff) = StdCoordsToHalfDiffCoords(wo, wi);


            // Find index.
            // Note that phi_half is ignored, since isotropic BRDFs are assumed
            int ind = PhiDiffIndex(fiDiff) +
                  ThetaDiffIndex(thetaDiff) * BRDF_SAMPLING_RES_PHI_D / 2 +
                  ThetaHalfIndex(thetaHalf) * BRDF_SAMPLING_RES_PHI_D / 2 *
                                     BRDF_SAMPLING_RES_THETA_D;

            double redVal = Math.Max(brdf[ind] * RED_SCALE, 0);
            double greenVal = Math.Max(brdf[ind + BRDF_SAMPLING_RES_THETA_H * BRDF_SAMPLING_RES_THETA_D * BRDF_SAMPLING_RES_PHI_D / 2] * GREEN_SCALE, 0);
            double blueVal = Math.Max(brdf[ind + BRDF_SAMPLING_RES_THETA_H * BRDF_SAMPLING_RES_THETA_D * BRDF_SAMPLING_RES_PHI_D] * BLUE_SCALE, 0);


            if (redVal < 0.0 || greenVal < 0.0 || blueVal < 0.0)
            {
                Console.WriteLine("Below horizon.\n");
            }

            return (redVal, greenVal, blueVal);

        }

        /// <summary>
        /// Convert standard coordinates to half vector/difference vector coordinates
        /// </summary>
        /// <param name="theta_in"></param>
        /// <param name="fi_in"></param>
        /// <param name="theta_out"></param>
        /// <param name="fi_out"></param>
        /// <returns></returns>
        (double theta_half, double fi_half, double theta_diff, double fi_diff) StdCoordsToHalfDiffCoords(Vector3 wo, Vector3 wi)
        {
            wi = wi.Normalize();
            wo = wo.Normalize();

            Vector3 wh = (wo + wi).Normalize();
            // compute  theta_half, fi_half
            double thetaHalf = Math.Acos(wh.z);
            double fiHalf = Math.Atan2(wh.y, wh.x);


            Vector3 normal = new Vector3(0.0, 0.0, 1.0);
            Vector3 biNormal = new Vector3(0.0, 1.0, 0.0);

            // compute diff vector
            Vector3 temp = Vector3.Rotate(wi, normal, -fiHalf);
            Vector3 diff = Vector3.Rotate(temp, biNormal, -thetaHalf);

            // compute  theta_diff, fi_diff	
            double thetaDiff = Math.Acos(diff.z);
            double fiDiff = Math.Atan2(diff.y, diff.x);
            return (thetaHalf, fiHalf, thetaDiff, fiDiff);
        }

        /// <summary>
        /// Lookup theta_half index
        /// This is a non-linear mapping!
        /// In:  [0 .. pi/2]
        /// Out: [0 .. 89]
        /// </summary>
        /// <param name="thetaHalf"></param>
        /// <returns></returns>
        int ThetaHalfIndex(double thetaHalf)
        {
            if (thetaHalf <= 0.0)
                return 0;
            double theta_half_deg = ((thetaHalf / (Math.PI / 2.0)) * BRDF_SAMPLING_RES_THETA_H);
            double temp = theta_half_deg * BRDF_SAMPLING_RES_THETA_H;
            temp = Math.Sqrt(temp);
            int ret_val = (int)temp;
            if (ret_val < 0) ret_val = 0;
            if (ret_val >= BRDF_SAMPLING_RES_THETA_H)
                ret_val = BRDF_SAMPLING_RES_THETA_H - 1;
            return ret_val;
        }

        /// <summary>
        /// Lookup theta_diff index
        /// In:  [0 .. pi/2]
        /// Out: [0 .. 89]
        /// </summary>
        /// <param name="thetaDiff"></param>
        /// <returns></returns>
        int ThetaDiffIndex(double thetaDiff)
        {
            int tmp = (int)(thetaDiff / (Math.PI * 0.5) * BRDF_SAMPLING_RES_THETA_D);
            if (tmp < 0)
                return 0;
            else if (tmp < BRDF_SAMPLING_RES_THETA_D - 1)
                return tmp;
            else
                return BRDF_SAMPLING_RES_THETA_D - 1;
        }

        /// <summary>
        /// Lookup phi_diff index
        /// </summary>
        /// <param name="phiDiff"></param>
        /// <returns></returns>
        int PhiDiffIndex(double phiDiff)
        {
            // Because of reciprocity, the BRDF is unchanged under
            // phi_diff -> phi_diff + Math.PI
            if (phiDiff < 0.0)
                phiDiff += Math.PI;

            // In: phi_diff in [0 .. pi]
            // Out: tmp in [0 .. 179]
            int tmp = (int)(phiDiff / Math.PI * BRDF_SAMPLING_RES_PHI_D / 2);
            if (tmp < 0)
                return 0;
            else if (tmp < BRDF_SAMPLING_RES_PHI_D / 2 - 1)
                return tmp;
            else
                return BRDF_SAMPLING_RES_PHI_D / 2 - 1;
        }
    }
}
