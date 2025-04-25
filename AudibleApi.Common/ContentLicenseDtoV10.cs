using System;
using System.Collections.Generic;
using System.Linq;

namespace AudibleApi.Common
{
    public partial class ContentLicenseDtoV10 : V10Base<ContentLicenseDtoV10>
    {
        public static ContentLicenseDtoV10 FromJson(Newtonsoft.Json.Linq.JObject json, string deviceType, string deviceSerialNumber, string amazonAccountId)
        {
            var license = json.ToObject<ContentLicenseDtoV10>();

            if (license.ContentLicense.DrmType == DrmType.Adrm && license.ContentLicense?.LicenseResponse is not null)
                license.ContentLicense.Voucher = DecryptLicenseResponse(license, deviceType, deviceSerialNumber, amazonAccountId);

            return license;
        }

        private static VoucherDtoV10 DecryptLicenseResponse(ContentLicenseDtoV10 contentLicense, string deviceType, string deviceSerialNumber, string amazonAccountId)
        {
            //AAXC scheme described in:
            //https://patchwork.ffmpeg.org/project/ffmpeg/patch/17559601585196510@sas2-2fa759678732.qloud-c.yandex.net/

            byte[] keyComponents = System.Text.Encoding.ASCII.GetBytes(
                deviceType +
                deviceSerialNumber +
                amazonAccountId +
                contentLicense.ContentLicense.Asin
                );

            byte[] key = new byte[16];
            byte[] iv = new byte[16];

            var hash = System.Security.Cryptography.SHA256.HashData(keyComponents);
            Array.Copy(hash, 0, key, 0, 16);
            Array.Copy(hash, 16, iv, 0, 16);

            var cipherText = Convert.FromBase64String(contentLicense.ContentLicense.LicenseResponse);

            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = key;

            var plainTextBts = aes.DecryptCbc(cipherText, iv, System.Security.Cryptography.PaddingMode.None);
            var plainText = System.Text.Encoding.ASCII.GetString(plainTextBts.TakeWhile(b => b != 0).ToArray());

            return VoucherDtoV10.FromJson(plainText);
        }
    }
}
