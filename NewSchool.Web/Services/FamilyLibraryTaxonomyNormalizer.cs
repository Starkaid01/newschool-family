using System.Globalization;
using System.Text;

namespace NewSchool.Web.Services;

internal static class FamilyLibraryTaxonomyNormalizer
{
    public static string NormalizeCategory(string? value)
    {
        var normalized = NormalizeKey(value);
        return normalized switch
        {
            "ciencias" => "Ciências",
            "matematica" => "Matemática",
            "portugues" => "Português",
            "fabulas" => "Fábulas",
            "atividade de matematica" => "Atividade de matemática",
            "atividade de mundo" => "Atividade integrada de mundo",
            "ebooks" => "eBooks",
            _ => value?.Trim() ?? string.Empty
        };
    }

    public static string NormalizeEducationStage(string? value)
    {
        var normalized = NormalizeKey(value);
        return normalized switch
        {
            "educacao infantil" => "Educação Infantil",
            "ensino fundamental" => "Ensino Fundamental",
            _ => value?.Trim() ?? string.Empty
        };
    }

    private static string NormalizeKey(string? value)
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
            .ToLowerInvariant()
            .Trim();
    }
}
