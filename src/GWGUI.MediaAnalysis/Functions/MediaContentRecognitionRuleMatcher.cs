using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Functions;

internal static class MediaContentRecognitionRuleMatcher
{
    public static bool Matches(MediaContentRecognitionRule rule, IReadOnlyList<byte>? content)
    {
        if (content is null || !HasContentConditions(rule)) return false;
        if (rule.ContentLengths is { Count: > 0 } lengths && !lengths.Contains(content.Count)) return false;
        return rule.Signatures.All(signature => MatchesSignature(content, signature));
    }

    public static bool HasContentConditions(MediaContentRecognitionRule rule) =>
        rule.Signatures.Count > 0 || rule.ContentLengths is { Count: > 0 };

    private static bool MatchesSignature(IReadOnlyList<byte> content, MediaContentSignature signature)
    {
        if (signature.ByteGroups.Count == 0 || signature.ByteGroups.Any(group => group.Count == 0)) return false;

        return signature.Direction switch
        {
            MediaContentSearchDirection.Start => MatchesFixed(content, signature, fromEnd: false),
            MediaContentSearchDirection.End => MatchesFixed(content, signature, fromEnd: true),
            MediaContentSearchDirection.SearchStart => MatchesInOrder(content, signature, fromEnd: false),
            MediaContentSearchDirection.SearchEnd => MatchesInOrder(content, signature, fromEnd: true),
            _ => false
        };
    }

    private static bool MatchesFixed(
        IReadOnlyList<byte> content,
        MediaContentSignature signature,
        bool fromEnd)
    {
        if (signature.Position is not int position || signature.ByteGroups.Count != 1) return false;
        var group = signature.ByteGroups[0];
        var start = fromEnd ? content.Count - 1 - position : position;
        return MatchesAt(content, group, start);
    }

    private static bool MatchesInOrder(
        IReadOnlyList<byte> content,
        MediaContentSignature signature,
        bool fromEnd)
    {
        if (signature.Position is not null) return false;

        var boundary = fromEnd ? content.Count : 0;
        foreach (var group in signature.ByteGroups)
        {
            var foundAt = Find(content, group, boundary, fromEnd);
            if (foundAt < 0) return false;
            boundary = fromEnd ? foundAt : foundAt + group.Count;
        }
        return true;
    }

    private static int Find(
        IReadOnlyList<byte> content,
        IReadOnlyList<byte> group,
        int boundary,
        bool fromEnd)
    {
        if (fromEnd)
        {
            for (var start = boundary - group.Count; start >= 0; start--)
                if (MatchesAt(content, group, start)) return start;
            return -1;
        }

        for (var start = boundary; start <= content.Count - group.Count; start++)
            if (MatchesAt(content, group, start)) return start;
        return -1;
    }

    private static bool MatchesAt(
        IReadOnlyList<byte> content,
        IReadOnlyList<byte> group,
        int start)
    {
        if (start < 0 || start > content.Count - group.Count) return false;
        for (var index = 0; index < group.Count; index++)
            if (content[start + index] != group[index]) return false;
        return true;
    }
}
