using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Plugin;
using System;

namespace OnTheFlyTranslator
{
    public enum CastBarTranslationStyle
    { 
        Parenthesis = 0,
        SmallerText = 1,
        NoOriginalText = 2,
    }

    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        [NonSerialized]
        private static Configuration? Instance;

        [NonSerialized]
        private IDalamudPluginInterface? pluginInterface;

        public int Version { get; set; } = 1;
        public ClientLanguage eTargetLanguage { get; set; } = ClientLanguage.French;
        public CastBarTranslationStyle eOption { get; set; } = CastBarTranslationStyle.SmallerText;
        public bool EnableTranslation { get; set; } = true;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            pluginInterface!.SavePluginConfig(this);
        }

        public static Configuration GetInstance() => Instance ??= new Configuration();
        public static void SetInstance(IDalamudPluginInterface pluginInterface)
        {
            Instance = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Instance?.Initialize(pluginInterface);
        }
    }
}
