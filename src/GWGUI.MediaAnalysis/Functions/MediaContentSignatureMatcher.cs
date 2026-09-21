using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Functions;

internal static class MediaContentSignatureMatcher
{
    public static bool Matches(MediaContentRecognitionRule rule, IReadOnlyList<byte>? content)
    {
        if (content is null || !HasSignatures(rule)) return false;
        return rule.Signatures.All(signature => MatchesSignature(content, signature));
    }

    public static bool HasSignatures(MediaContentRecognitionRule rule) =>
        rule.Signatures.Count > 0;

    private static bool MatchesSignature(IReadOnlyList<byte> content, MediaContentSignature signature)
    {
        if (signature.Bytes.Count == 0 || signature.Bytes.Count > content.Count) return false;

        var lastStart = content.Count - signature.Bytes.Count;
        var first = signature.Position is int position
            ? signature.Direction == MediaContentSearchDirection.Start
                ? position
                : content.Count - 1 - position
            : signature.Direction == MediaContentSearchDirection.Start
                ? 0
                : lastStart;
        var last = signature.Position is not null
            ? first
            : signature.Direction == MediaContentSearchDirection.Start
                ? lastStart
                : 0;
        var step = first <= last ? 1 : -1;

        for (var start = first; step > 0 ? start <= last : start >= last; start += step)
        {
            if (start < 0 || start > lastStart) continue;
            var matches = true;
            for (var index = 0; index < signature.Bytes.Count; index++)
            {
                if (content[start + index] == signature.Bytes[index]) continue;
                matches = false;
                break;
            }
            if (matches) return true;
        }
        return false;
    }
}
