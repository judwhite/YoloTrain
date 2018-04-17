using System;
using System.Runtime.InteropServices;

namespace YoloTrain.Utils
{
    public static class YoloDll
    {
        [DllImport("yolo_cpp_dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NewDetector([In] string cfg_filename, [In] string weight_filename, [In] int gpu_id = 0);

        [DllImport("yolo_cpp_dll.dll")]
        public static extern int Detect([In] IntPtr detector, [In] string image_filename, [In] IntPtr result, [In] float thresh = 0.2f, [In] bool use_mean = false);

        [DllImport("yolo_cpp_dll.dll")]
        public static extern void FreeDetector([In] IntPtr detector);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingBoxArray
    {
        public int count;
        public IntPtr data;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingBox
    {
        // (x,y) - top-left corner, (w, h) - width & height of bounded box
        public uint x;
        public uint y;
        public uint w;
        public uint h;

        // confidence - probability that the object was found correctly
        public float prob;

        // class of object - from range [0, classes-1]
        public uint obj_id;

        // tracking id for video (0 - untracked, 1 - inf - tracked object)
        public uint track_id;
        // counter of frames on which the object was detected
        public uint frames_counter;
    }
}
