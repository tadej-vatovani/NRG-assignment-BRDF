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
        const double M_PI = 3.1415926535897932384626433832795;

        double[] brdf;
        /// <summary>
        /// Uses the supplied filepath to build MERL matrix, if relativePath is set to true, expects MERL binary files to be in BRDF folder(when using debugging) 
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


            //if (filepath == "pink-fabric")
            //{
            //    string path = "D:\\FRI\\NRG\\Seminar\\cpp\\out_cpp_pink-fabric.txt";


            //    string[] readText = File.ReadAllLines(path);
            //    Console.WriteLine(readText.Length + " " + n);
            //    for (int i = 0; i < readText.Length; i++)
            //    {
            //        var vals = readText[i].Split(' ');
            //        brdf[i * 3] = Double.Parse(vals[0]);
            //        brdf[i * 3 + 1] = Double.Parse(vals[1]);
            //        brdf[i * 3 + 2] = Double.Parse(vals[2]);
            //        if (i < 20)
            //            Console.WriteLine(Double.Parse(vals[0]));
            //    }
            //}
            //n = 16;

            //StreamWriter f2 = new StreamWriter("d:\\fri\\nrg\\seminar\\cpp\\out_c#_" + filename + ".txt");
            //for (int i = 0; i < n; i++)
            //{
            //    double theta_in = i * 0.5 * M_PI / n;
            //    for (int j = 0; j < 4 * n; j++)
            //    {
            //        double phi_in = j * 2.0 * M_PI / (4 * n);
            //        for (int k = 0; k < n; k++)
            //        {
            //            double theta_out = k * 0.5 * M_PI / n;
            //            for (int l = 0; l < 4 * n; l++)
            //            {
            //                double phi_out = l * 2.0 * M_PI / (4 * n);
            //                (double red, double green, double blue) = Lookup_brdf_val(theta_in, phi_in, theta_out, phi_out);
            //                f2.WriteLine("{0:0.0000} {1:0.0000} {2:0.0000}", red, green, blue);
            //            }
            //        }
            //    }
            //}
        }

        public override Spectrum f(Vector3 wo, Vector3 wi)
        {
            double theta_in = Math.Acos(wi.z);
            double fi_in = Math.Atan2(wi.y, wi.x);
            double theta_out = Math.Acos(wo.z);
            double fi_out = Math.Atan2(wo.y, wo.x);

            (double r, double g, double b) = Lookup_brdf_val(theta_in, fi_in, theta_out, fi_out);
            //Console.WriteLine("{0} {1} {2}", (int)(r / RED_SCALE), (int)(g / GREEN_SCALE), (int)(b / BLUE_SCALE));
            Color output = Color.FromArgb((int)Math.Min(255, r * 1500), (int)Math.Min(255, g * 1500), (int)Math.Min(255, b * 1500));
            return Spectrum.ZeroSpectrum.FromRGB(output);
        }

        public override double Pdf(Vector3 wo, Vector3 wi)
        {
            return 1 / (M_PI * 4);
        }

        public override (Spectrum, Vector3, double) Sample_f(Vector3 wo)
        {
            Vector3 wi = CosineSampleHemisphere();
            if (wo.z < 0)
                wi.z *= -1;
            return (f(wo, wi), wi, Pdf(wo, wi));

        }

        (double red_val, double green_val, double blue_val) Lookup_brdf_val(double theta_in, double fi_in, double theta_out, double fi_out)
        {

            (double theta_half, double fi_half, double theta_diff, double fi_diff) = std_coords_to_half_diff_coords(theta_in, fi_in, theta_out, fi_out);


            // Find index.
            // Note that phi_half is ignored, since isotropic BRDFs are assumed
            int ind = phi_diff_index(fi_diff) +
                  theta_diff_index(theta_diff) * BRDF_SAMPLING_RES_PHI_D / 2 +
                  theta_half_index(theta_half) * BRDF_SAMPLING_RES_PHI_D / 2 *
                                     BRDF_SAMPLING_RES_THETA_D;

            double red_val = Math.Max(brdf[ind] * RED_SCALE, 0);
            double green_val = Math.Max(brdf[ind + BRDF_SAMPLING_RES_THETA_H * BRDF_SAMPLING_RES_THETA_D * BRDF_SAMPLING_RES_PHI_D / 2] * GREEN_SCALE, 0);
            double blue_val = Math.Max(brdf[ind + BRDF_SAMPLING_RES_THETA_H * BRDF_SAMPLING_RES_THETA_D * BRDF_SAMPLING_RES_PHI_D] * BLUE_SCALE, 0);


            if (red_val < 0.0 || green_val < 0.0 || blue_val < 0.0)
            {
                Console.WriteLine("Below horizon.\n");
            }

            return (red_val, green_val, blue_val);

        }

        /// <summary>
        /// Convert standard coordinates to half vector/difference vector coordinates
        /// </summary>
        /// <param name="theta_in"></param>
        /// <param name="fi_in"></param>
        /// <param name="theta_out"></param>
        /// <param name="fi_out"></param>
        /// <returns></returns>
        (double theta_half, double fi_half, double theta_diff, double fi_diff) std_coords_to_half_diff_coords(double theta_in, double fi_in, double theta_out, double fi_out)
        {

            // compute in vector
            double in_vec_z = Math.Cos(theta_in);
            double proj_in_vec = Math.Sin(theta_in);
            double in_vec_x = proj_in_vec * Math.Cos(fi_in);
            double in_vec_y = proj_in_vec * Math.Sin(fi_in);
            Vector3 v_in = new Vector3(in_vec_x, in_vec_y, in_vec_z);
            v_in = v_in.Normalize();


            // compute out vector
            double out_vec_z = Math.Cos(theta_out);
            double proj_out_vec = Math.Sin(theta_out);
            double out_vec_x = proj_out_vec * Math.Cos(fi_out);
            double out_vec_y = proj_out_vec * Math.Sin(fi_out);
            Vector3 v_out = new Vector3(out_vec_x, out_vec_y, out_vec_z);
            v_out = v_out.Normalize();


            // compute halfway vector
            double half_x = (v_in.x + v_out.x) / 2.0f;
            double half_y = (v_in.y + v_out.y) / 2.0f;
            double half_z = (v_in.z + v_out.z) / 2.0f;
            Vector3 half = new Vector3(half_x, half_y, half_z);
            half = half.Normalize();

            // compute  theta_half, fi_half
            double theta_half = Math.Acos(half.z);
            double fi_half = Math.Atan2(half.y, half.x);


            Vector3 bi_normal = new Vector3(0.0, 1.0, 0.0);
            Vector3 normal = new Vector3(0.0, 0.0, 1.0);

            // compute diff vector
            Vector3 temp = Vector3.Rotate(v_in, normal, -fi_half);
            Vector3 diff = Vector3.Rotate(temp, bi_normal, -theta_half);

            // compute  theta_diff, fi_diff	
            double theta_diff = Math.Acos(diff.z);
            double fi_diff = Math.Atan2(diff.y, diff.x);
            return (theta_half, fi_half, theta_diff, fi_diff);
        }

        /// <summary>
        /// Lookup theta_half index
        /// This is a non-linear mapping!
        /// In:  [0 .. pi/2]
        /// Out: [0 .. 89]
        /// </summary>
        /// <param name="theta_half"></param>
        /// <returns></returns>
        int theta_half_index(double theta_half)
        {
            if (theta_half <= 0.0)
                return 0;
            double theta_half_deg = ((theta_half / (M_PI / 2.0)) * BRDF_SAMPLING_RES_THETA_H);
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
        /// <param name="theta_diff"></param>
        /// <returns></returns>
        int theta_diff_index(double theta_diff)
        {
            int tmp = (int)(theta_diff / (M_PI * 0.5) * BRDF_SAMPLING_RES_THETA_D);
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
        /// <param name="phi_diff"></param>
        /// <returns></returns>
        int phi_diff_index(double phi_diff)
        {
            // Because of reciprocity, the BRDF is unchanged under
            // phi_diff -> phi_diff + M_PI
            if (phi_diff < 0.0)
                phi_diff += M_PI;

            // In: phi_diff in [0 .. pi]
            // Out: tmp in [0 .. 179]
            int tmp = (int)(phi_diff / M_PI * BRDF_SAMPLING_RES_PHI_D / 2);
            if (tmp < 0)
                return 0;
            else if (tmp < BRDF_SAMPLING_RES_PHI_D / 2 - 1)
                return tmp;
            else
                return BRDF_SAMPLING_RES_PHI_D / 2 - 1;
        }
    }
}
