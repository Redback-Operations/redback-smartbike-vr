using System;
using System.Globalization;

/// <summary>
/// Shared helper for reading values out of the simple text messages published
/// on the bike/controller MQTT topics. Two shapes show up in this project:
/// a flat "{'key': value, 'key2': value2}"-style message (used by the combined
/// control topic), and a single bare number (a topic that only ever carries
/// one metric, e.g. "72"). ReadFloat/ReadBool handle the first; ReadNumericOrField
/// handles either, which is what the single-metric bike topics (heartrate,
/// cadence, speed, power) need since their exact payload shape isn't nailed
/// down in code anywhere yet.
///
/// Not real JSON parsing - just enough substring work to pull one named field
/// out of a flat, single-level message, tolerant of single or double quotes
/// around keys. Extracted from SpeedListener so every listener parses the
/// same way instead of each re-implementing it slightly differently.
/// </summary>
public static class MqttFieldParser
{
    public static float ReadFloat(string msg, string fieldName)
    {
        int keyIndex = msg.IndexOf($"'{fieldName}'");
        if (keyIndex == -1)
            keyIndex = msg.IndexOf($"\"{fieldName}\"");

        if (keyIndex == -1)
            return 0f;

        int colonIndex = msg.IndexOf(':', keyIndex);
        int endIndex = msg.IndexOfAny(new char[] { ',', '}' }, colonIndex + 1);

        if (colonIndex == -1 || endIndex == -1)
            return 0f;

        string rawValue = msg.Substring(colonIndex + 1, endIndex - colonIndex - 1).Trim().Trim('\'', '"');

        if (float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
        {
            return value;
        }

        return 0f;
    }

    public static bool ReadBool(string msg, string fieldName)
    {
        int keyIndex = msg.IndexOf($"'{fieldName}'");
        if (keyIndex == -1)
            keyIndex = msg.IndexOf($"\"{fieldName}\"");

        if (keyIndex == -1)
            return false;

        int colonIndex = msg.IndexOf(':', keyIndex);
        int endIndex = msg.IndexOfAny(new char[] { ',', '}' }, colonIndex + 1);

        if (colonIndex == -1 || endIndex == -1)
            return false;

        string rawValue = msg.Substring(colonIndex + 1, endIndex - colonIndex - 1).Trim().Trim('\'', '"');

        return rawValue.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// True if the field is present in the message at all, as opposed to
    /// present but unparsable (which ReadFloat/ReadBool silently treat as
    /// 0/false either way). Use this to tell "not published yet" apart from
    /// "published as zero".
    /// </summary>
    public static bool HasField(string msg, string fieldName)
    {
        return msg.Contains($"'{fieldName}'") || msg.Contains($"\"{fieldName}\"");
    }

    /// <summary>
    /// For topics that only ever carry one metric, where the payload might be
    /// either a bare number ("72") or a "{'fieldName': 72}" style object.
    /// Tries the bare-number reading first, falls back to ReadFloat.
    /// TODO: once the bike firmware's actual payload format for this topic is
    /// confirmed, this can likely be simplified to just one of the two.
    /// </summary>
    public static float ReadNumericOrField(string msg, string fieldName)
    {
        if (msg != null && float.TryParse(msg.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float bare))
        {
            return bare;
        }

        return ReadFloat(msg, fieldName);
    }
}
