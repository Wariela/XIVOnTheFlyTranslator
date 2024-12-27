using Lumina.Excel;

namespace OnTheFlyTranslator.Translation
{
    internal class TranslationDatabase<T> where T : struct, IExcelRow<T>
    {
        private ExcelSheet<T>? originalSheet;
        private ExcelSheet<T>? targetSheet;

        public TranslationDatabase()
        {
            UpdateSheets();
        }

        public void UpdateSheets()
        {
            var targetLanguage = Configuration.GetInstance().eTargetLanguage;
            targetSheet = DalamudApi.DataManager.GetExcelSheet<T>(targetLanguage);
            originalSheet = DalamudApi.DataManager.GetExcelSheet<T>(DalamudApi.DataManager.Language);
        }

        public ExcelSheet<T>? GetSheet(bool bOriginalLanguageSheet = true)
        {
            UpdateSheets();
            return bOriginalLanguageSheet ? originalSheet : targetSheet;
        }

        public TranslationDatas<T> GetAvailableTranslation(uint rowId)
        {
            UpdateSheets();
            return new TranslationDatas<T>(originalSheet?.GetRow(rowId), targetSheet?.GetRow(rowId));
        }
    }
}
