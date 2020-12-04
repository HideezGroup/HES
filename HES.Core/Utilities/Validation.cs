using HES.Core.Exceptions;
using Hideez.SDK.Communication.Utils;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HES.Core.Utilities
{
    public static class Validation
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
                    throw new IncorrectUrlException($"Incorrect url.");
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
                throw new IncorrectOtpException("Incorrect OTP secret.");
            }

            return otp;
        }

        public static string GetModelStateErrors(ModelStateDictionary ModelState)
        {
            return string.Join(" ", ModelState.Values.SelectMany(s => s.Errors).Select(s => s.ErrorMessage).ToArray());
        }
    }
}