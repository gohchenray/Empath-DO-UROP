using System;
using UnityEngine;
using Intel.RealSense;

public class GetPixelColor : MonoBehaviour
{
    private Pipeline pipeline;
    private Colorizer colorizer;
    private Texture2D colorTexture;

    private int width;
    private int height;

    void Start()
    {
        // Initialize RealSense pipeline and colorizer
        pipeline = new Pipeline();
        colorizer = new Colorizer();

        // Configure the pipeline to stream color frames
        var cfg = new Config();
        cfg.EnableStream(Stream.Color, 640, 480, Format.Rgb8, 30);
        pipeline.Start(cfg);

        // Initialize a Texture2D to display color data
        width = 640;
        height = 480;
        colorTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    void Update()
    {
        using (var frames = pipeline.WaitForFrames())
        using (var colorFrame = frames.ColorFrame)
        {
            if (colorFrame != null)
            {
                // Create a byte array to store color data
                byte[] colorData = new byte[colorFrame.Stride * colorFrame.Height];
                colorFrame.CopyTo(colorData);

                // Load data into Texture2D
                colorTexture.LoadRawTextureData(colorData);
                colorTexture.Apply();

                // Access RGB value at a specific pixel (e.g., (x, y))
                int x = 100; // Replace with the desired X coordinate
                int y = 100; // Replace with the desired Y coordinate

                // Calculate the index of the pixel in the color data array
                int index = (y * colorFrame.Width + x) * 3;
                if (index < colorData.Length - 3)
                {
                    byte red = colorData[index];
                    byte green = colorData[index + 1];
                    byte blue = colorData[index + 2];

                    Debug.Log($"RGB at ({x}, {y}): R={red}, G={green}, B={blue}");
                }
            }
            else
            {
                Debug.Log("no data");
            }
        }
    }

    void OnDestroy()
    {
        // Cleanup RealSense resources
        if (pipeline != null)
        {
            pipeline.Stop();
            pipeline.Dispose();
        }
        if (colorizer != null)
        {
            colorizer.Dispose();
        }
    }
}
