using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

#pragma warning disable CS8625 // Impossible de convertir un littéral ayant une valeur null en type référence non-nullable.

namespace OnTheFlyTranslator
{
    public class DalamudApi
    {
        public static void Initialize(IDalamudPluginInterface pluginInterface) => pluginInterface.Create<DalamudApi>();
        [PluginService] public static ITargetManager TargetManager { get; private set; } = null;
        [PluginService] public static IPluginLog PluginLog { get; private set; } = null;
        [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null;
        [PluginService] public static IDataManager DataManager { get; private set; } = null;
    }
}

#pragma warning restore CS8625
