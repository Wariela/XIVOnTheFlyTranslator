using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;

using OnTheFlyTranslator.Translation;
using Dalamud.Game.ClientState.Objects.Types;
namespace OnTheFlyTranslator
{
    public unsafe class TargetWatcher : UIModifier
    {
        private readonly TranslationService translationService;
        private AtkTextNode* castBarAdditionalName;

        public TargetWatcher()
        {
            translationService = TranslationService.GetInstance();
        }

        protected override void HandlePostRefreshEvent(AddonEvent type, AddonArgs args)
        {
            if(args.AddonName == "HudLayout")
                castBarAdditionalName = null;
        }

        protected override void HandlePreDrawEvent(AddonEvent type, AddonArgs args)
        {
            var unitBase = (AtkUnitBase*)args.Addon;
            if (unitBase == null)
                return;

            if (castBarAdditionalName != null)
                castBarAdditionalName->SetText("");

            if (args.AddonName == "_TargetInfoCastBar")
                UpdateElement(unitBase);
        }

        protected override unsafe void UpdateElement(AtkUnitBase* addon)
        {
            var configuration = Configuration.GetInstance();
            if (!configuration.EnableTranslation)
                return;

            var target = DalamudApi.TargetManager.Target as IBattleChara ?? DalamudApi.TargetManager.SoftTarget as IBattleChara;
            if (target == null || !target.IsCasting)
                return;

            var translatedAction = translationService.GetActionTranslation(target.CastActionId);
            if (translatedAction == null || translatedAction.TranslatedName.Contains("_rsv_"))
                return;

            var castNameNode = addon->GetTextNodeById(4);
            if (castNameNode == null)
                return;

            switch (configuration.eOption)
            {
                case CastBarTranslationStyle.Parenthesis:
                    castNameNode->SetText($"{translatedAction.OriginalName} ({translatedAction.TranslatedName})");
                    break;
                case CastBarTranslationStyle.SmallerText:
                    if (castBarAdditionalName == null)
                    {
                        DalamudApi.PluginLog.Warning("Necessary UI not created. Creating now.");
                        castBarAdditionalName = UIManipulationHelper.CloneNode(castNameNode);
                        if (castBarAdditionalName == null)
                        {
                            DalamudApi.PluginLog.Error("Couldn't create atk text node");
                            return;
                        }

                        var newStrPtr = UIManipulationHelper.Alloc(1024);
                        castBarAdditionalName->NodeText.StringPtr = (byte*)newStrPtr;
                        castBarAdditionalName->NodeText.BufSize = 1024;

                        UIManipulationHelper.ExpandNodeList(addon, 1);
                        addon->UldManager.NodeList[addon->UldManager.NodeListCount++] = (AtkResNode*)castBarAdditionalName;
                    }

                    if (castBarAdditionalName == null)
                        return;

                    castBarAdditionalName->AtkResNode.SetScale(0.8f, 0.8f);
                    castBarAdditionalName->AtkResNode.SetPositionFloat(7, -13);
                    castBarAdditionalName->SetAlignment(AlignmentType.Left);
                    castBarAdditionalName->SetAlpha(castNameNode->Alpha_2);
                    castBarAdditionalName->SetText(translatedAction.TranslatedName);
                    break;
                case CastBarTranslationStyle.NoOriginalText:
                    castNameNode->SetText(translatedAction.TranslatedName);
                    break;
            }
        }
    }
}
