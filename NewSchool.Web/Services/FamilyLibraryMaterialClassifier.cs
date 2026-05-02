using System.Globalization;
using System.Text;

namespace NewSchool.Web.Services;

internal static class FamilyLibraryMaterialClassifier
{
    private static readonly string[] BookCues =
    [
        "ebooks",
        "leitura em voz alta",
        "fabulas",
        "fábulas",
        "livrinhos",
        "literatura",
        "historias biblicas",
        "histórias bíblicas",
        "teologia"
    ];

    private static readonly string[] PrintableCues =
    [
        "alfabeto",
        "atividade",
        "atividades",
        "brincadeiras",
        "caligrafia",
        "ciencias",
        "ciências",
        "colorir",
        "contorne",
        "coordenacao motora",
        "coordenação motora",
        "desenvolvimento motor",
        "flashcard",
        "flashcards",
        "ligue",
        "logica",
        "lógica",
        "matematica",
        "matemática",
        "pintar",
        "portugues",
        "português",
        "quiz",
        "seu corpo",
        "timeline",
        "tracado",
        "traçado"
    ];

    public static bool IsPrintable(
        bool sourceFlag,
        string title,
        string category,
        string sourceRelativePath,
        string skillFocus,
        string description)
    {
        var searchable = NormalizeText($"{title} {category} {sourceRelativePath} {skillFocus} {description}");
        var printableScore = ScoreMatches(searchable, PrintableCues);
        var bookScore = ScoreMatches(searchable, BookCues);

        if (bookScore >= 2 && printableScore == 0)
        {
            return false;
        }

        if (printableScore >= 2 && bookScore == 0)
        {
            return true;
        }

        if (printableScore > bookScore)
        {
            return true;
        }

        if (bookScore > printableScore)
        {
            return false;
        }

        return sourceFlag;
    }

    public static bool IsBook(
        bool sourceFlag,
        string title,
        string category,
        string sourceRelativePath,
        string skillFocus,
        string description)
    {
        return !IsPrintable(sourceFlag, title, category, sourceRelativePath, skillFocus, description);
    }

    private static int ScoreMatches(string searchable, IReadOnlyCollection<string> cues)
    {
        var score = 0;
        foreach (var cue in cues)
        {
            if (searchable.Contains(NormalizeText(cue), StringComparison.Ordinal))
            {
                score++;
            }
        }

        return score;
    }

    private static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }
}
