using System;

namespace OnTheFlyTranslator.Translation
{
    public class TranslationService : IDisposable
    {
        private static TranslationService? Instance;

        private readonly TranslationDatabase<Lumina.Excel.GeneratedSheets.Action> actionDatabase;
        public TranslationService()
        {
            actionDatabase = new();
        }

        public void Dispose()
        {

        }

        public TranslationResult? GetActionTranslation(uint actionId)
        {
            var data = actionDatabase.GetAvailableTranslation(actionId);
            return new TranslationResult(data.Original?.Name ?? "", data.Target?.Name ?? "");
        }
        public static TranslationService GetInstance() => Instance ??= new TranslationService();
    }
}

