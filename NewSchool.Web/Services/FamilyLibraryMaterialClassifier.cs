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
        "marque",
        "matematica",
        "matemática",
        "complete",
        "conte",
        "corresponda",
        "cubra",
        "circule",
        "pintar",
        "portugues",
        "português",
        "quiz",
        "quantidade",
        "escreva",
        "seu corpo",
        "timeline",
        "trace",
        "tracado",
        "traçado"
    ];

    private static readonly string[] StoryCues =
    [
        "historia",
        "história",
        "conto",
        "aventura",
        "fabula",
        "fábula",
        "narrativa",
        "personagem",
        "selva",
        "floresta",
        "menino",
        "menina",
        "esquilo",
        "gato",
        "perdao",
        "perdão"
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
        var normalizedTitle = NormalizeText(title);
        var normalizedCategory = NormalizeText(category);
        var normalizedSourcePath = NormalizeText(sourceRelativePath);

        if (normalizedTitle.StartsWith("leituras de ", StringComparison.Ordinal)
            || normalizedCategory.Contains("leituras-base do curriculo", StringComparison.Ordinal)
            || normalizedSourcePath.StartsWith("author/leituras-", StringComparison.Ordinal))
        {
            return false;
        }

        if (normalizedTitle.StartsWith("apostila de ", StringComparison.Ordinal)
            || normalizedTitle.StartsWith("caderno de avaliacao de ", StringComparison.Ordinal)
            || normalizedCategory.Contains("apostila do curriculo", StringComparison.Ordinal)
            || normalizedCategory.Contains("avaliacao por unidade", StringComparison.Ordinal)
            || normalizedSourcePath.StartsWith("author/apostila-", StringComparison.Ordinal)
            || normalizedSourcePath.StartsWith("author/avaliacao-", StringComparison.Ordinal))
        {
            return true;
        }

        var printableScore = ScoreMatches(searchable, PrintableCues);
        var bookScore = ScoreMatches(searchable, BookCues);
        var storyScore = ScoreMatches(searchable, StoryCues);

        if (normalizedCategory.Contains("livrinhos", StringComparison.Ordinal)
            && printableScore >= 2
            && storyScore == 0)
        {
            return true;
        }

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
