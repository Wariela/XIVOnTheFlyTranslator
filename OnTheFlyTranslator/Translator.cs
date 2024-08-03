using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using OnTheFlyTranslator.Translation;

namespace OnTheFlyTranslator
{
    public sealed class Translator : IDalamudPlugin
    {
        public WindowSystem WindowSystem = new("XIV On the fly translator settings");
        public string Name => "On the fly translator";
        private IDalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        private ConfigWindow ConfigWindow { get; init; }
        public TargetWatcher TargetWatcher { get; init; }


        public Translator(IDalamudPluginInterface pluginInterface, ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            Configuration.SetInstance(this.PluginInterface);
            DalamudApi.Initialize(this.PluginInterface);

            ConfigWindow = new ConfigWindow(this);            
            WindowSystem.AddWindow(ConfigWindow);

            this.CommandManager.AddHandler("/otft", new CommandInfo(OpenSettings)
            {
                HelpMessage = "Open the translator configuration"
            });

            this.TargetWatcher = new TargetWatcher();
            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenMainUi += DrawConfigUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            ConfigWindow.Dispose();
            Configuration.GetInstance().Save();
            TranslationService.GetInstance()?.Dispose();
            this.CommandManager.RemoveHandler("/otft");
        }

        private void OpenSettings(string command, string args)
        {
            ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
