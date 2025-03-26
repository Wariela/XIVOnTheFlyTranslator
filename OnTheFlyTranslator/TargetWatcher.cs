using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;

using OnTheFlyTranslator.Translation;
using Dalamud.Utility;

namespace OnTheFlyTranslator
{
    public unsafe class TargetWatcher : UIModifier
    {
        private static readonly uint STRING_ALLOC_SIZE = 1024;

        private readonly TranslationService translationService;
        private TranslationResult lastTranslationResult;
        private AtkTextNode* castBarAdditionalName;

        public TargetWatcher(): base("_TargetInfoCastBar")
        {
            translationService = TranslationService.GetInstance();
            lastTranslationResult = new TranslationResult("", "");
        }

        protected override void HandlePostRequestedUpdate(AddonEvent type, AddonArgs args)
        {
            AtkUnitBase* pUnitBase;
            if ((pUnitBase = (AtkUnitBase*)args.Addon) == null)
            {
                return;
            }

            UpdateElement(pUnitBase);
        }

        protected unsafe void UpdateElement(AtkUnitBase* pBaseNode)
        {
            if (pBaseNode == null)
            {
                return;
            }

            AtkTextNode* pTextNode = null;
            if (!Configuration.GetInstance().EnableTranslation || !GetTargetTextNode(pBaseNode, ref pTextNode) || pTextNode == null)
            {
                if(castBarAdditionalName != null)
                {
                    castBarAdditionalName->ToggleVisibility(false);
                }
                
                return;
            }

            // if(lastTranslationResult.OriginalName != pTextNode->NodeText.ToString())
            // {
                if (!GetTranslationFromString(pTextNode->NodeText.ToString(), ref lastTranslationResult))
                {
                    return;
                }
            // }


            switch (Configuration.GetInstance().eOption)
            {
                case CastBarTranslationStyle.Parenthesis:
                    pTextNode->SetText($"{lastTranslationResult.OriginalName} ({lastTranslationResult.TranslatedName})");
                    break;
                case CastBarTranslationStyle.SmallerText:
                    castBarAdditionalName = CreateAdditionalNameNode(pBaseNode, pTextNode);
                    if (castBarAdditionalName == null)
                    {
                        DalamudApi.PluginLog.Error("Couldn't get or create new ATK Text Node");
                        return;
                    }

                    castBarAdditionalName->AtkResNode.SetScale(0.8f, 0.8f);
                    castBarAdditionalName->AtkResNode.SetPositionFloat(7, -13);
                    castBarAdditionalName->SetAlignment(AlignmentType.Left);
                    castBarAdditionalName->SetAlpha(pTextNode->Alpha_2);
                    castBarAdditionalName->SetText(lastTranslationResult.TranslatedName);
                    castBarAdditionalName->ToggleVisibility(pTextNode->IsVisible());
                    break;
                case CastBarTranslationStyle.NoOriginalText:
                    pTextNode->SetText(lastTranslationResult.TranslatedName);
                    break;
            }
        }

        private bool GetTargetTextNode(AtkUnitBase* addon, ref AtkTextNode* pTextNode)
        {
            if (addon == null)
            {
                return false;
            }

            pTextNode = addon->GetTextNodeById(4);
            return pTextNode != null;
        }

        private bool GetTranslationFromString(string srcText, ref TranslationResult output)
        {
            if(srcText.IsNullOrEmpty())
            {
                return false;
            }

            uint uActionId = 0;
            if (!translationService.GetActionIDFromName(srcText, ref uActionId, true))
            {
                return false;
            }

            output = translationService.GetActionTranslation(uActionId);
            return !output.TranslatedName.IsNullOrEmpty() && !output.TranslatedName.Contains("_rsv_");
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
            {
                return null;
            }

            var newStrPtr = UIManipulationHelper.Alloc(STRING_ALLOC_SIZE);
            castBarAdditionalName->NodeText.StringPtr = (byte*)newStrPtr;
            castBarAdditionalName->NodeText.BufSize = STRING_ALLOC_SIZE;

            UIManipulationHelper.ExpandNodeList(addon, 1);
            addon->UldManager.NodeList[addon->UldManager.NodeListCount++] = (AtkResNode*)castBarAdditionalName;
            return castBarAdditionalName;

        }
    }
}
