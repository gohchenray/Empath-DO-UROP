using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Intel.RealSense;
using UnityEngine;

[ProcessingBlockData(typeof(RsColourFilter))]
public class RsColourFilter : RsProcessingBlock
{
    public float minHue = 0.2f;  // Minimum Hue values to keep
    public float maxHue = 0.8f;  // Maximum Hue values to keep
    private byte[] colourData;


    void OnDisable()
    {
        colourData = null;
    }

    private Frame ApplyFilter(VideoFrame colourFrame, FrameSource frameSource)
    {
        int colourFrameSize = colourFrame.Width * colourFrame.Height * colourFrame.BitsPerPixel/8;
        if (colourData == null || colourData.Length != colourFrameSize)
            colourData = new byte[colourFrameSize];

        colourFrame.CopyTo(colourData);

        // byte minR = (byte)(minColour.r * 255);
        // byte minG = (byte)(minColour.g * 255);
        // byte minB = (byte)(minColour.b * 255);
        // byte maxR = (byte)(maxColour.r * 255);
        // byte maxG = (byte)(maxColour.g * 255);
        // byte maxB = (byte)(maxColour.b * 255);


        // for (int i = 0; i < colourData.Length; i+= 3)
        // {
        //     byte r = colourData[i];
        //     byte g = colourData[i + 1];
        //     byte b = colourData[i + 2];


        //     if(r < minR || r > maxR || g < minG || g > maxG || b < minB || b > maxB)
        //     {
        //         colourData[i] = 0;
        //         colourData[i + 1] = 0;
        //         colourData[i + 2] = 0;
        //     }

        // }

        for (int i = 0; i < colourData.Length; i += 3)
        {
            byte r = colourData[i];
            byte g = colourData[i + 1];
            byte b = colourData[i + 2];

            // Convert RGB to HSV
            float h, s, v;
            Color.RGBToHSV(new Color(r / 255f, g / 255f, b / 255f), out h, out s, out v);

            // Filter by Hue
            if (h < minHue || h > maxHue)
            {
                // Set the pixel to black
                colourData[i] = 0;
                colourData[i + 1] = 0;
                colourData[i + 2] = 0;
            }
        }

        var modifiedFrame = frameSource.AllocateVideoFrame<VideoFrame>(colourFrame.Profile, colourFrame, colourFrame.BitsPerPixel, colourFrame.Width, colourFrame.Height, colourFrame.Stride, Extension.VideoFrame);
        modifiedFrame.CopyFrom(colourData);

        return modifiedFrame;        
    }

    public override Frame Process(Frame frame, FrameSource frameSource)
    {
        if (frame.IsComposite)
        {
            using (var fs = FrameSet.FromFrame(frame))
            using (var colourFrameInner = fs.ColorFrame)
            {
                var v = ApplyFilter(colourFrameInner, frameSource);
                // return v;

                // find and remove the original depth frame
                var frames = new List<Frame>();
                foreach (var f in fs)
                {
                    if (f.Profile.Stream == Stream.Color && f.Profile.Format == Format.Rgb8)
                    {
                        frames.Add(v);  // Add modified color frame
                    }
                    else
                    {
                        frames.Add(f);  // Add other frames unmodified
                    }
                }

                var res = frameSource.AllocateCompositeFrame(frames);
                frames.ForEach(f => f.Dispose());
                return res.AsFrame();
            }
        }

        if (frame is VideoFrame colourFrame && colourFrame.Profile.Format == Format.Rgb8)
            return ApplyFilter(colourFrame, frameSource);

        return frame;
    }

}
