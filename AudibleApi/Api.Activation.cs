using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AudibleApi
{
    public partial class Api
    {
        static Api()
        {
            //Device license response encoded in ISO-8859-15. Register encoding providers once. 
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        const int ACTIVATION_BLOB_SZ = 0x238;
        public async Task<string> GetActivationBytesAsync()
        {
            // note: this call uses the audible login uri, NOT api
            var client = Sharer.GetSharedHttpClient(Locale.AudibleLoginUri());

            var response = await AdHocAuthenticatedGetAsync($"/license/token?action=register&player_manuf=Audible,iPhone&player_model=iPhone", client);

            var deviceLicense = await response.Content.ReadAsByteArrayAsync();

            if (deviceLicense.Length < ACTIVATION_BLOB_SZ)
                throw new ApiErrorException(response.Headers.Location, new JObject { { "error", "Unexpected activation response: " + await response.Content.ReadAsStringAsync() } });

            //activation bytes are at beginning of activation blob
            uint actBytes = BitConverter.ToUInt32(deviceLicense, deviceLicense.Length - ACTIVATION_BLOB_SZ);

            return actBytes.ToString("x8");
        }
    }
}
