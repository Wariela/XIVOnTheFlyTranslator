using Dalamud;
using Dalamud.Utility;
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
            var targetLuminaLanguage = targetLanguage.ToLumina();
            if (originalSheet == null || targetSheet == null || targetSheet.Language != targetLuminaLanguage)
            {
                targetSheet = DalamudApi.DataManager.GetExcelSheet<T>(targetLanguage);
                originalSheet = DalamudApi.DataManager.GetExcelSheet<T>(DalamudApi.DataManager.Language);
            }
        }

        public TranslationDatas<T> GetAvailableTranslation(uint rowId)
        {
            UpdateSheets();
            return new TranslationDatas<T>(originalSheet?.GetRow(rowId), targetSheet?.GetRow(rowId));
        }
    }
}
