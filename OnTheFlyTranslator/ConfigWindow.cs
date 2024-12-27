using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using OnTheFlyTranslator.Translation;
using System;
using System.Numerics;
using System.Transactions;

namespace OnTheFlyTranslator
{
    public class ConfigWindow : Window, IDisposable
    {
        public ConfigWindow(Translator plugin) : base("Spell translation configuration")
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 100),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
        }

        public void Dispose()
        {

        }

        public override void Draw()
        {
            var configuration = Configuration.GetInstance();
            ImGui.Text($"Client language: {DalamudApi.DataManager.Language}");
            ImGui.NewLine();

            var translationEnabled = configuration.EnableTranslation;
            ImGui.Checkbox("Enable translation", ref translationEnabled);
            if (translationEnabled != configuration.EnableTranslation)
                configuration.EnableTranslation = translationEnabled;

            configuration.eTargetLanguage = DrawComboEnum("Target language", configuration.eTargetLanguage);
            configuration.eOption = DrawComboEnum("Style option", configuration.eOption);

            if (ImGui.TreeNodeEx("Debugging options", ImGuiTreeNodeFlags.Framed))
                Configuration.DrawDebugWindow |= ImGui.Button("Display database debug window");
        }

        private static T DrawComboEnum<T>(string comboName, T target) where T : Enum
        {
            var selected = target;
            if (ImGui.BeginCombo(comboName, target.ToString(), ImGuiComboFlags.HeightSmall))
            {
                foreach (T enumValue in Enum.GetValues(typeof(T)))
                {
                    if (ImGui.Selectable(enumValue.ToString()))
                        target = enumValue;

                    if (target.Equals(enumValue))
                    {
                        selected = enumValue;
                        ImGui.SetItemDefaultFocus();
                    }   
                }
                ImGui.EndCombo();
            }
            return selected;
        }
    }
}

