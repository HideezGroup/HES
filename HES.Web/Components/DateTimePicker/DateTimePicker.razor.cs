using Microsoft.AspNetCore.Components;
using System;

namespace HES.Web.Components
{
    public partial class DateTimePicker<TValue> : HESDomComponentBase
    {
        private const string DateMax = "9999-12-31T23:59";

        [Parameter] public TValue Value { get; set; }
        [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

        private void OnValueChanged(ChangeEventArgs args)
        {
            var value = args.Value.ToString();

            if (string.IsNullOrEmpty(value))
                return;

            TValue result;

            var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

            if (targetType == typeof(DateTime))
            {
                TryParseDateTime(value, out result);
                ValueChanged.InvokeAsync(result);
            }
            else if (targetType == typeof(DateTimeOffset))
            {
                TryParseDateTimeOffset(value, out result);
                ValueChanged.InvokeAsync(result);
            }
            else
            {
                throw new InvalidOperationException($"The type '{targetType}' is not a supported date type.");
            }
        }

        private bool TryParseDateTime(string value, out TValue result)
        {
            var success = DateTime.TryParse(value, out var parsedValue);
            if (success)
            {
                result = (TValue)(object)parsedValue;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        private bool TryParseDateTimeOffset(string value, out TValue result)
        {
            var success = DateTimeOffset.TryParse(value, out var parsedValue);
            if (success)
            {
                result = (TValue)(object)parsedValue;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}