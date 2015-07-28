using System;
using System.Collections.Generic;
using System.Text;

namespace HeapSurf
{
    public class CreditCard
    {
        public string Number { get; set; }
        public string Expiration { get; set; }
        public string CardholderName { get; set; }
        public string Track1 { get; set; }
        public string Track2 { get; set; }
        public DateTime FirstDiscovered { get; set; }
        public DateTime LastDiscovered { get; set; }
        public string Literal { get; set; }
        public List<string> ProcessNames { get; set; }
        public List<long> MemoryAddresses { get; set; }

        public CreditCard()
        {
            FirstDiscovered = DateTime.MaxValue;
            LastDiscovered = DateTime.MinValue;
            ProcessNames = new List<string>();
            MemoryAddresses = new List<long>();
        }
    }
}
