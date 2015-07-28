using System;
using System.Collections.Generic;

namespace HeapSurf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HeapSurfer surfer;
            if (args.Length > 0)
            {
                surfer = new HeapSurfer(args[0]);
            }
            else
            {
                surfer = new HeapSurfer();
            }
            surfer.Surf();
        }
    }
}