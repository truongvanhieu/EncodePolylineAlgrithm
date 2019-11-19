using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncodedPolylineAlgorithm
{

    public class Location
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    /// <summary>
    /// Google Polyline Converter (Encoder and Decoder)
    /// </summary>
    public static class GooglePolylineConverter
    {
        /// <summary>
        /// Decodes the specified polyline string.
        /// </summary>
        /// <param name="polylineString">The polyline string.</param>
        /// <returns>A list with Locations</returns>
        public static IEnumerable<Location> Decode(string polylineString)
        {
            if (string.IsNullOrEmpty(polylineString))
                throw new ArgumentNullException(nameof(polylineString));

            var polylineChars = polylineString.ToCharArray();
            var index = 0;

            var currentLat = 0;
            var currentLng = 0;

            while (index < polylineChars.Length)
            {
                // Next lat
                var sum = 0;
                var shifter = 0;
                int nextFiveBits;
                do
                {
                    nextFiveBits = polylineChars[index++] - 63;
                    sum |= (nextFiveBits & 31) << shifter;
                    shifter += 5;
                } while (nextFiveBits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                // Next lng
                sum = 0;
                shifter = 0;
                do
                {
                    nextFiveBits = polylineChars[index++] - 63;
                    sum |= (nextFiveBits & 31) << shifter;
                    shifter += 5;
                } while (nextFiveBits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && nextFiveBits >= 32)
                    break;

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                yield return new Location
                {
                    lat = Convert.ToDouble(currentLat) / 1E5,
                    lng = Convert.ToDouble(currentLng) / 1E5
                };
            }
        }

        /// <summary>
        /// Encodes the specified locations list.
        /// </summary>
        /// <param name="locations">The locations.</param>
        /// <returns>The polyline string.</returns>
        public static string Encode(IEnumerable<Location> locations)
        {
            var str = new StringBuilder();

            var encodeDiff = (Action<int>)(diff =>
            {
                var shifted = diff << 1;
                if (diff < 0)
                    shifted = ~shifted;

                var rem = shifted;

                while (rem >= 0x20)
                {
                    str.Append((char)((0x20 | (rem & 0x1f)) + 63));

                    rem >>= 5;
                }

                str.Append((char)(rem + 63));
            });

            var lastLat = 0;
            var lastLng = 0;

            foreach (var point in locations)
            {
                var lat = (int)Math.Round(point.lat * 1E5);
                var lng = (int)Math.Round(point.lng * 1E5);

                encodeDiff(lat - lastLat);
                encodeDiff(lng - lastLng);

                lastLat = lat;
                lastLng = lng;
            }

            return str.ToString();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int orgin = -1;
            int destination = -1;
            IEnumerable<Location> result = GooglePolylineConverter.Decode(@"_`daBampsSN?LHDF}CX\fDYHU@ONCF?PFLNJL@Zt@HlATpCVzADTHXf@fAR\v@n@p@r@h@X~@h@pAv@lBlA\RbAn@lC|AbAj@RJn@^vAz@l@^tAx@NH~BpA~@f@h@ZPHt@d@r@^FDDBVL|@b@ZTTLJF^Vn@^t@d@pBpAnAv@V^\?J?RAvAIzASf@INCn@M~AQ|AWRIXCNCpAQr@KHAd@Ib@GXGfE_@zBU~@On@OFAp@Qr@MRGdCgAvAe@v@YLEh@OHEhCaA`Ag@x@Wl@QxAk@f@UlAe@bASZfEVCSyBOcBFI@OCE??GG?]GsBG_BAeBCiCEgBImEE{CAs@AkEAyBjAI~@Bf@H~@\jAh@tC|Aj@XpBx@fAVtATt@FdABxDBnABnEJpB@t@CxAAnAGlACjDGnGEDLNLRDTATMFKDQ`EWrDOdDQHbBBbA@LLlBdCOxAKh@Gj@CN?t@EDAj@CRA|@GlAKH?`AGJAf@Cp@El@E|AKL?vDYPnDRhE`@`HLvBCLDNl@tKENFPRhDL~BH`@CLFLAT^fHNj@CF?BDHCTDdA?nADXIHEJAL@LBLHJNHRBPEFC`ETlHhA`BXnCb@|Dj@~Cd@dIrAjBb@XBT?LJLBN@TEb@BlCIzBI`@AXAnC@rBEtMYp@?nBJT???HHJ\j@rALd@`@vBjAhGH\\xB~@xE~@jFl@hDTI`Bc@jBi@XKlA_@lAc@pC_A`Cy@tBu@rBy@jFkB^lBd@|BPz@Ld@`@nAVl@hArBbErHn@jBl@rAv@`CHVRbBBj@?|@C\]xF?JUd@]Z[HKJM@W@[LKFQZKv@EpF?JGtCATzBNzB?fDEdOUt@Az@@|@NvA^lK`D`FrAtFdBl@NnFzAt@Nn@JlABf@CxAKdH}@FAjEs@|EgAfPuCpAW|Be@LFNATBd@NNLdAzAt@|@`@Zn@Vh@PnZxInNdEzCv@hF~ApMzDjAZhItBdFhAdBPz@FhALn@Lj@T\V`@p@LVNb@ZzAdAxBjAl@F@`BVhD`@dBTtBVtD`@bC`@tC`@zHxAxCh@v@NfF|@jAT^Fn@TfAf@NHlCbBDD|@j@bAf@jCh@X?v@A`@Ax@OzAW~CStBMfCOt@?u@?gCNuBL_DR{AVy@Na@@w@@Y?kCi@cAg@}@k@EEmCcBOIgAg@o@U_@GkAUgF}@w@OyCi@{HyAuCa@cCa@uDa@uBWeBUiDa@aBWGAkAm@eAyB[{AOc@MWa@q@]Wk@Uo@MiAM{@G");
            //for (int i = 0; i < result.ToArray().Count(); i++)
            //{
            //    if (result.ElementAt(i).lat == 16.082079 && result.ElementAt(i).lng == 108.223643)
            //    {
            //        orgin = i;
            //    }

            //    if (result.ElementAt(i).lat == 16.08242 && result.ElementAt(i).lng == 108.220829)
            //    {
            //        destination = i;
            //    }
            //}

            var a = result.Where(x=>x.lat == 16.082079 && x.lng == 108.223643);

            Console.ReadKey();
        }
    }
}
