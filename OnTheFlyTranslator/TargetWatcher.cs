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
            AtkTextNode* pCastNameNode = null;
            TranslationResult? translationResult = null;
            if (!Configuration.GetInstance().EnableTranslation)
                return;

            var target = DalamudApi.TargetManager.Target as IBattleChara ?? DalamudApi.TargetManager.SoftTarget as IBattleChara;
            if (target == null || !target.IsCasting)
                return;


            pCastNameNode = addon->GetTextNodeById(4);
            if (pCastNameNode == null)
                return;

            translationResult = translationService.GetActionTranslation(target.CastActionId);
            if (translationResult == null || translationResult.TranslatedName.Contains("_rsv_"))
                return;

            switch (Configuration.GetInstance().eOption)
            {
                case CastBarTranslationStyle.Parenthesis:
                    pCastNameNode->SetText($"{translationResult.OriginalName} ({translationResult.TranslatedName})");
                    break;
                case CastBarTranslationStyle.SmallerText:
                    castBarAdditionalName = CreateAdditionalNameNode(addon, pCastNameNode);
                    if (castBarAdditionalName == null)
                    {
                        DalamudApi.PluginLog.Error("Couldn't get or create new ATK Text Node");
                        return;
                    }

                    castBarAdditionalName->AtkResNode.SetScale(0.8f, 0.8f);
                    castBarAdditionalName->AtkResNode.SetPositionFloat(7, -13);
                    castBarAdditionalName->SetAlignment(AlignmentType.Left);
                    castBarAdditionalName->SetAlpha(pCastNameNode->Alpha_2);
                    castBarAdditionalName->SetText(translationResult.TranslatedName);
                    break;
                case CastBarTranslationStyle.NoOriginalText:
                    pCastNameNode->SetText(translationResult.TranslatedName);
                    break;
            }
        }

        private AtkTextNode* CreateAdditionalNameNode(AtkUnitBase* addon, AtkTextNode* srcNode)
        {
            if (castBarAdditionalName != null)
            { 
                return castBarAdditionalName;
            }

            DalamudApi.PluginLog.Warning("Necessary UI not created. Creating now.");
            castBarAdditionalName = UIManipulationHelper.CloneNode(srcNode);
            if (castBarAdditionalName == null)
                return null;

            var newStrPtr = UIManipulationHelper.Alloc(1024);
            castBarAdditionalName->NodeText.StringPtr = (byte*)newStrPtr;
            castBarAdditionalName->NodeText.BufSize = 1024;

            UIManipulationHelper.ExpandNodeList(addon, 1);
            addon->UldManager.NodeList[addon->UldManager.NodeListCount++] = (AtkResNode*)castBarAdditionalName;
            return castBarAdditionalName;

        }
    }
}
