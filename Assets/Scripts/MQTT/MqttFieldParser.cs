using System;
using System.Globalization;
using System.Text.RegularExpressions;

public static class MqttFieldParser
{
    public static bool TryReadFloat(string payload, out float value, params string[] fieldNames)
    {
        value = 0f;

        if (string.IsNullOrWhiteSpace(payload))
            return false;

        string trimmed = payload.Trim().Trim('"', '\'');
        if (float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        foreach (string fieldName in fieldNames)
        {
            if (TryReadJsonLikeField(payload, fieldName, out value))
                return true;
        }

        return false;
    }

    private static bool TryReadJsonLikeField(string payload, string fieldName, out float value)
    {
        value = 0f;

        if (string.IsNullOrWhiteSpace(fieldName))
            return false;

        string escapedFieldName = Regex.Escape(fieldName);
        string pattern = "[\"']?" + escapedFieldName + "[\"']?\\s*:\\s*[\"']?([-+]?[0-9]*\\.?[0-9]+)";
        Match match = Regex.Match(payload, pattern, RegexOptions.IgnoreCase);

        return match.Success
            && float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }
}
