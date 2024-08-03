using Lumina.Excel;

namespace OnTheFlyTranslator.Translation
{
    public class TranslationResult(string name, string translatedName)
    {
        public string OriginalName { get; set; } = name;
        public string TranslatedName { get; set; } = translatedName;
    }

    internal class TranslationDatas<T>(T? originalValue, T? translatedValue) where T : ExcelRow
    {
        public T? Original { get; } = originalValue;
        public T? Target { get; } = translatedValue;
    }
}
