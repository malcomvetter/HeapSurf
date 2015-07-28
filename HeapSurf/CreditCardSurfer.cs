using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace HeapSurf
{
    public class CreditCardSurfer
    {
        const string pattern16numeric = @"[0-9]{16}";
        const string patternExpiration = @"[0-9]{4}";
        const string patternTrack1and2 = @"(((%%?[Bb`]?)[0-9]{13,19}\^[A-Za-z\s]{0,26}\/[A-Za-z\s]{0,26}\^(1[2-9]|2[0-9])(0[1-9]|1[0-2])[0-9\s]{3,50}\?)[;\s]{1,3}([0-9]{13,19}=(1[2-9]|2[0-9])(0[1-9]|1[0-2])[0-9]{3,50}\?))";
        const string patternTrack1 = @"((%%?[Bb`]?)[0-9]{13,19}\^[A-Za-z\s]{0,26}\/[A-Za-z\s]{0,26}\^(1[2-9]|2[0-9])(0[1-9]|1[0-2])[0-9\s]{3,50}\?)";
        const string patternTrack2 = @"([0-9]{13,19}=(1[2-9]|2[0-9])(0[1-9]|1[0-2])[0-9]{3,50})";
        const string patternName = @"[a-zA-Z -',.]{3,30}"; // @"[a-z ,.'-]+$/i";

        private Config _config;

        public CreditCardSurfer() : this ("config.xml")
        { }

        public CreditCardSurfer(string file)
        {
            _config = Config.Load(file);
        }

        public bool FindCC(byte[] bytes, string ProcessName, long offset)
        {
            string s = System.Text.Encoding.UTF8.GetString(bytes);
            var cc = FindPAN(s, ProcessName, offset);
            if (!string.IsNullOrEmpty(cc.Number))
            {
                cc.Track1 = FindTrack1Data(s);
                cc.Track2 = FindTrack2Data(s);
                Save(cc);
                return true;
            }
            return false;
        }

        string FindTrack1Data(string s)
        {
            var match = Regex.Match(s, patternTrack1);
            if (match.Success)
            {
                return match.Value;
            }
            return null;
        }

        string FindTrack2Data(string s)
        {
            var match = Regex.Match(s, patternTrack2);
            if (match.Success)
            {
                return match.Value;
            }
            return null;
        }

        CreditCard FindPAN(string s, string ProcessName, long offset)
        {
            var cc = new CreditCard();
            var match = Regex.Match(s, pattern16numeric);
            if (match.Success && match.Value != "0000000000000000" && LuhnCheck(match.Value))
            {
                cc.Number = match.Value;
                cc.Expiration = FindExpiration(s, cc.Number);
                cc.CardholderName = FindName(s);
                cc.Literal = s;
                cc.FirstDiscovered = DateTime.Now;
                cc.LastDiscovered = DateTime.Now;
                cc.MemoryAddresses.Add(offset + match.Index);
                cc.ProcessNames.Add(ProcessName);
                if (Config.Default.ConsoleOutput)
                {
                    Console.WriteLine(" + Found PAN ({0}) in Process: {1}", cc.Number, ProcessName);
                }
            }
            return cc;
        }

        string FindExpiration(string literal, string cc)
        {
            literal = literal.Replace(cc, "");
            var match = Regex.Match(literal, patternExpiration);
            if (match.Success)
            {
                return match.Value;
            }
            return "";
        }

        string FindName(string s)
        {
            var match = Regex.Match(s, patternName);
            if (match.Success)
            {
                return match.Value;
            }
            return "";
        }

        bool LuhnCheck(string s)
        {
            int[] DELTAS = new int[] { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };
            int checksum = 0;
            char[] chars = s.ToCharArray();
            for (int i = chars.Length - 1; i > -1; i--)
            {
                int j = ((int)chars[i]) - 48;
                checksum += j;
                if (((i - chars.Length) % 2) == 0)
                    checksum += DELTAS[j];
            }

            return ((checksum % 10) == 0);
        }

        void Save(CreditCard CC)
        {
            if (CC == null)
            {
                return;
            }
            var CCs = new List<CreditCard>();
            if (_config != null && !string.IsNullOrEmpty(_config.FilePath))
            {
                CCs = FileHelper.Load<List<CreditCard>>(_config.FilePath);
            }
            CCs.Add(CC);
            var sorted = Unique(CCs);
            try
            {
                FileHelper.Save<List<CreditCard>>(sorted, _config.FilePath);
            }
            catch (Exception)
            {
                FileHelper.Save<List<CreditCard>>(CCs, Config.Default.FilePath);
            }
        }

        List<CreditCard> Unique(List<CreditCard> unsorted)
        {
            var sorted = new List<CreditCard>();
//            unsorted.Sort();
            foreach (var u in unsorted)
            {
                var found = false;
                foreach (var s in sorted)
                {
                    if (u.Number == s.Number)
                    {
                        s.FirstDiscovered = GetOldestDate(u.FirstDiscovered, s.FirstDiscovered);
                        s.LastDiscovered = GetNewestDate(u.LastDiscovered, s.LastDiscovered);
                        s.Expiration = GetExpiration(u.Expiration, s.Expiration);
                        s.Track1 = GetTrack1(s.Track1, u.Track1);
                        s.Track2 = GetTrack2(s.Track2, u.Track2);
                        s.CardholderName = GetCardholderName(u.CardholderName, s.CardholderName);
                        s.ProcessNames.AddRange(u.ProcessNames);
                        s.ProcessNames = Unique(s.ProcessNames);
                        s.MemoryAddresses.AddRange(u.MemoryAddresses);
                        s.MemoryAddresses = Unique(s.MemoryAddresses);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
//                    u.FirstDiscovered = u.LastDiscovered;
                    sorted.Add(u);
                }
            }
            return sorted;
        }

        List<string> Unique(List<string> unsorted)
        {
            var sorted = new List<string>();
            foreach (var u in unsorted)
            {
                var found = false;
                foreach (var s in sorted)
                {
                    if (u == s)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    sorted.Add(u);
                }
            }
            return sorted;
        }

        List<long> Unique(List<long> unsorted)
        {
            var sorted = new List<long>();
            foreach (var u in unsorted)
            {
                var found = false;
                foreach (var s in sorted)
                {
                    if (u == s)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    sorted.Add(u);
                }
            }
            return sorted;
        }

        DateTime GetOldestDate(DateTime d1, DateTime d2)
        {
            if (d1 < d2)
            {
                return d1;
            }
            return d2;
        }

        DateTime GetNewestDate(DateTime d1, DateTime d2)
        {
            if (d1 > d2)
            {
                return d1;
            }
            return d2;
        }

        string GetExpiration(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
            {
                return s2;
            }
            if (string.IsNullOrEmpty(s2))
            {
                return s1;
            }
            //TODO: comparison?
            return s1;
        }

        string GetCardholderName(string s1, string s2)
        {
            //TODO: update this if needed
            return GetExpiration(s1, s2);
        }

        string GetTrack1(string s1, string s2)
        {
            //TODO: update this if needed
            return GetExpiration(s1, s2);
        }

        string GetTrack2(string s1, string s2)
        {
            //TODO: update this if needed
            return GetExpiration(s1, s2);
        }
    }
}
