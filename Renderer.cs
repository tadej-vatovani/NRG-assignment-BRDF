using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static PathTracer.Samplers;

namespace PathTracer
{

  class Renderer
  {
    public static double Epsilon = 1e-4;


    private Spectrum[,] finalImageSum;
    private Spectrum[,] finalImage;
    private Bitmap finalRgbImage;
    private double[,] pixelWeights;
    private long totalSamples = 0;
    private const long maxTotalSamples = 10000000;

    private Bitmap bmp;


    private int pixelWidth, pixelHeight;
    public Renderer(Bitmap b)
    {
      pixelWidth = b.Width;
      pixelHeight = b.Height;
      finalImageSum = new Spectrum[pixelWidth, pixelHeight];
      finalImage = new Spectrum[pixelWidth, pixelHeight];
      finalRgbImage = new Bitmap(b);
      pixelWeights = new double[pixelWidth, pixelHeight];
      bmp = b;
    }

    public long SPP => totalSamples / (pixelWidth * pixelHeight);

    public void CopyBitmap(Image image)
    {
      var img = finalImage;

      // convert to rgb
      for (int i = 0; i < finalImage.GetLength(0);i++)
      {
        for (int j = 0; j < finalImage.GetLength(1);j++)
        {
          var rgb = finalImage[i, j]?.ToRGB();

          if (rgb == null)
            finalRgbImage.SetPixel(i, pixelHeight - 1 - j, Color.Black);
          else
          {
            rgb[0] = rgb[0].Clamp(0, 1.0039);
            rgb[1] = rgb[1].Clamp(0, 1.0039);
            rgb[2] = rgb[2].Clamp(0, 1.0039);
            Color c = Color.FromArgb((int)Math.Floor(255 * rgb[0]), (int)Math.Floor(255 * rgb[1]), (int)Math.Floor(255 * rgb[2]));
            finalRgbImage.SetPixel(i, pixelHeight - 1 - j, c);
          }
        }
      }
      ImageAttributes imageAttributes = new ImageAttributes();
      imageAttributes.SetGamma(2.4f);
      using (Graphics grD = Graphics.FromImage(image))
      {
        grD.DrawImage(finalRgbImage, new Rectangle(Point.Empty, image.Size), 0,0,image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
      }
    }

    public void Render(Scene s, CancellationToken token)
    {
      var integrator = new PathTracer();

      // Use ParallelOptions instance to store the CancellationToken
      ParallelOptions po = new ParallelOptions()
      {
        MaxDegreeOfParallelism = 1,
        CancellationToken = token
      };
      try
      {
        while (totalSamples < maxTotalSamples)
        {
          var samples = SamplePointsOnImagePlane(s, 1);
          Parallel.ForEach(samples, po, samp =>
          {
            // if cancel requested from GUI, cancel
            token.ThrowIfCancellationRequested();

            var rayTo = new Vector3(samp.X - s.ImagePlaneWidth / 2, samp.Y - s.ImagePlaneHeight / 2 + s.ImagePlaneVerticalOffset, s.ImagePlaneDistance);
            // ray through point
            Ray r = new Ray(s.CameraOrigin, rayTo);

            // evaluate radiance
            Spectrum L = integrator.Li(r, s);

            // add radiance to image
            AddToImage(samp, L, s);

          });

        }
      }
      catch (OperationCanceledException)
      {
      }
    }
    enum FilterEnum
    {
      Box,
      Triangle,
    }

    private void AddToImage(PixelSample samp, Spectrum l, Scene s, FilterEnum filter= FilterEnum.Box)
    {
      Spectrum xyzOrRgb = l;
      double pixelWeight=1;
      double px = samp.X- (samp.I + 0.5);
      double py = samp.Y - (samp.J + 0.5);
      switch (filter)
      {
        case FilterEnum.Triangle:
          pixelWeight=Math.Max(0,1-Math.Abs(px))*Math.Max(0,1-Math.Abs(py));
          break;
      }

      lock (finalImageSum)
      {
        if (finalImageSum[samp.I, samp.J] == null)
        {
          finalImageSum[samp.I, samp.J] = xyzOrRgb * pixelWeight;
        }
        else
        {
          finalImageSum[samp.I, samp.J].AddTo(xyzOrRgb * pixelWeight);
        }
        pixelWeights[samp.I, samp.J] += pixelWeight;
        finalImage[samp.I, samp.J]= finalImageSum[samp.I, samp.J] * (1 / pixelWeights[samp.I, samp.J]);
        totalSamples++;
      }
    }


  /// <summary>
  /// Samples points on image plane, returns a list of samples (relative to imageplane 0..imageplanewidth, 0..imageplaneheight)
  /// </summary>
  /// <param name="s">scene</param>
  /// <param name="subsample">whether to perform subsample-times stratification. 1 = no stratification 2=2x2 etc.</param>
  /// <param name="oneSample">if true, just one uniformly-sampled sample is returned</param>
  /// <returns>a list of samples</returns>
  private List<PixelSample> SamplePointsOnImagePlane(Scene s, int subsample=1)
    {
      var samples = new List<PixelSample>();

      var sampleWidth = s.ImagePlaneWidth / (pixelWidth * subsample);
      var sampleHeight = s.ImagePlaneHeight / (pixelHeight * subsample);

      for (int i=0;i< pixelWidth*subsample;i++)
      {
        for (int j=0;j<pixelHeight*subsample; j++)
        {
          double rx = ThreadSafeRandom.NextDouble();
          double ry = ThreadSafeRandom.NextDouble();
          double x = (rx + i) * sampleWidth;
          double y = (ry + j) * sampleHeight;
          int pixI = i / subsample;
          int pixJ = j / subsample;

          samples.Add(new PixelSample { X = x, Y = y, I = pixI, J = pixJ });
        }
      }
      return samples;
    }

    private class PixelSample
    {
      /// <summary>
      /// image plane X
      /// </summary>
      public double X { get; set; }
      /// <summary>
      /// image plane Y
      /// </summary>
      public double Y { get; set; }
      /// <summary>
      /// Pixel I
      /// </summary>
      public int I { get; set; }
      /// <summary>
      /// Pixel J
      /// </summary>
      public int J { get; set; }
    }
  }
}
