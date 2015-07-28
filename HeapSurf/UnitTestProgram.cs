using System;
using System.Collections.Generic;
using System.Text;

namespace HeapSurf
{
    class UnitTestProgram
    {
        public static void Main1()
        {
            var processName = "UNIT_TEST";
            var surfer = new CreditCardSurfer();

            if (surfer.FindCC(GetAsciiBytesWithCC(), processName, 0))
            {
                Console.WriteLine("Found ASCII encoded CC as expected.");
            }
            if (!surfer.FindCC(GetAsciiBytesWithoutCC(), processName, 0))
            {
                Console.WriteLine("Did not find ASCII encoded CC as expected.");
            }
            if (surfer.FindCC(GetUnicodeBytesWithCC(), processName, 0))
            {
                Console.WriteLine("Found Unicode encoded CC as expected.");
            }
            if (!surfer.FindCC(GetUnicodeBytesWithCC(), processName, 0))
            {
                Console.WriteLine("Did not find Unicode encoded CC as expected.");
            }
            if (surfer.FindCC(GetShiftedBytes(), processName, 0))
            {
                Console.WriteLine("Found CC as expected.");
            }
            else
            {
                Console.WriteLine("Did not find CC in shifted bytes.");
            }
            if (surfer.FindCC(GetAsciiEncodedBytes(GetTestMC()), processName, 0))
            {
                Console.WriteLine("Found test MC as expected.");
            }
            if (surfer.FindCC(GetAsciiEncodedBytes(GetTestDiscover()), processName, 0))
            {
                Console.WriteLine("Found test Discover as expected.");
            }
            if (surfer.FindCC(GetAsciiEncodedBytes(GetTrack1Data()), processName, 0))
            {
                Console.WriteLine("Found Track1 Data as expected.");
            }
            if (surfer.FindCC(GetAsciiEncodedBytes(GetTrack2Data()), processName, 0))
            {
                Console.WriteLine("Found Track2 Data as expected.");
            }
        }

        static string GetTestVisa()
        {
            return "4111111111111111|0449|John Smith|87.79";
        }

        static string GetTestMC()
        {
            return "5555555555554444|0224|Jane Smith|132.98";
        }

        static string GetTestDiscover()
        {
            return "|||6011111111111117|||0321|||Leroy Smith|||213.41";
        }

        static string GetTrack2Data()
        {
            return "4111111111111111=2201777000000300001?";
        }

        static string GetTrack1Data()
        {
            return "%B4111111111111111^SMITH/JOHN^2201777000000300001?";
        }

        static string GetNonCCString()
        {
            return "Lorem Ipsum Dolor Yadda Yadda";
        }

        static byte[] GetAsciiBytesWithoutCC()
        {
            return GetAsciiEncodedBytes(GetNonCCString());
        }

        static byte[] GetAsciiBytesWithCC()
        {
            return GetAsciiEncodedBytes(GetTestVisa());
        }

        static byte[] GetUnicodeBytesWithCC()
        {
            return GetUnicodeEncodedBytes(GetTestVisa());
        }

        static byte[] GetUnicodeBytesWithoutCC()
        {
            return GetUnicodeEncodedBytes(GetNonCCString());
        }

        static byte[] GetShiftedBytes()
        {
            var b = GetUnicodeBytesWithCC();
            return RotateLeft(b, 1);
        }

        static byte[] GetAsciiEncodedBytes(string s)
        {
            return System.Text.Encoding.ASCII.GetBytes(s);
        }

        static byte[] GetUnicodeEncodedBytes(string s)
        {
            return System.Text.Encoding.UTF8.GetBytes(s);
        }

        static byte[] RotateLeft(byte[] bytes, int count)
        {
            var rotated = new List<byte>();
            foreach (var b in bytes)
            {
                rotated.Add(RotateLeft(b, count));
            }
            return rotated.ToArray();
        }

        static byte[] RotateRight(byte[] bytes, int count)
        {
            var rotated = new List<byte>();
            foreach (var b in bytes)
            {
                rotated.Add(RotateRight(b, count));
            }
            return rotated.ToArray();
        }

        static byte RotateLeft(byte value, int count)
        {
            count &= 0x07;
            return (byte)((value << count) | (value >> (8 - count)));
        }

        static byte RotateRight(byte value, int count)
        {
            count &= 0x07;
            return (byte)((value >> count) | (value << (8 - count)));
        }
    }
}
