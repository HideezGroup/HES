using HES.Core.Constants;
using HES.Core.Exceptions;
using Hideez.SDK.Communication.Utils;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HES.Core.Helpers
{
    public static class ValidationHelper
    {
        public static string VerifyUrls(string urls)
        {
            if (string.IsNullOrEmpty(urls))
                return null;

            List<string> verifiedUrls = new List<string>();
            foreach (var url in urls.Split(";"))
            {
                if (!UrlUtils.TryGetDomain(url.Trim(), out string domain))
                {
                    throw new HESException(HESCode.IncorrectUrl);
                }

                verifiedUrls.Add(domain);
            }

            var result = string.Join("; ", verifiedUrls.ToArray());
            return result;
        }

        public static string VerifyOtpSecret(string otp)
        {
            if (string.IsNullOrEmpty(otp))
                return null;

            var valid = Regex.IsMatch(otp.Replace(" ", ""), @"^[a-zA-Z0-9]+$");

            if (!valid)
            {
                throw new HESException(HESCode.IncorrectOtp);
            }

            return otp;
        }
         
        public static string VerifyReturnUrl(string url)
        {
            if (IsLocalUrl(url))
            {
                return url;
            }
            else
            {
                return Routes.Dashboard;
            }
        }

        public static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            // Allows "/" or "/foo" but not "//" or "/\".
            if (url[0] == '/')
            {
                // url is exactly "/"
                if (url.Length == 1)
                {
                    return true;
                }

                // url doesn't start with "//" or "/\"
                if (url[1] != '/' && url[1] != '\\')
                {
                    return true;
                }

                return false;
            }

            // Allows "~/" or "~/foo" but not "~//" or "~/\".
            if (url[0] == '~' && url.Length > 1 && url[1] == '/')
            {
                // url is exactly "~/"
                if (url.Length == 2)
                {
                    return true;
                }

                // url doesn't start with "~//" or "~/\"
                if (url[2] != '/' && url[2] != '\\')
                {
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}