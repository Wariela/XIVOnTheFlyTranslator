using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Addon.Lifecycle;

using System;

using OnTheFlyTranslator.Translation;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
namespace OnTheFlyTranslator
{
    public unsafe class TargetWatcher : IDisposable
    {
        private readonly TranslationService translationService;
        private AtkTextNode* castBarAdditionalName;

        public TargetWatcher()
        {
            translationService = TranslationService.GetInstance();
            DalamudApi.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, HandlePostRefreshEvent);
            DalamudApi.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, HandlePreDrawEvent);
        }

        public void Dispose()
        {
            DalamudApi.AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, HandlePostRefreshEvent);
            DalamudApi.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, HandlePreDrawEvent);
        }

        private void HandlePostRefreshEvent(AddonEvent type, AddonArgs args)
        {
            if(args.AddonName == "HudLayout")
                castBarAdditionalName = null;
        }

        private void HandlePreDrawEvent(AddonEvent type, AddonArgs args)
        {
            var unitBase = (AtkUnitBase*)args.Addon;
            if(unitBase == null)
                return;

            if (castBarAdditionalName != null)
                castBarAdditionalName->SetText("");

            if (args.AddonName == "_TargetInfoCastBar")
                UpdateTargetInfoCastBar(unitBase);
        }
        
        private void UpdateTargetInfoCastBar(AtkUnitBase* addon) 
        {
            var configuration = Configuration.GetInstance();
            if (!configuration.EnableTranslation)
                return;

            var target = DalamudApi.TargetManager.Target as IBattleChara ?? DalamudApi.TargetManager.SoftTarget as IBattleChara;
            if (target == null || !target.IsCasting)
                return;

            var translatedAction = translationService.GetActionTranslation(target.CastActionId);
            if(translatedAction == null) 
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
                    if(castBarAdditionalName == null)
                    {
                        DalamudApi.PluginLog.Warning("Necessary UI not created. Creating now.");
                        castBarAdditionalName = CloneNode(castNameNode);
                        if (castBarAdditionalName == null)
                        {
                            DalamudApi.PluginLog.Error("Couldn't create atk text node");
                            return;
                        }

                        var newStrPtr = Alloc(1024);
                        castBarAdditionalName->NodeText.StringPtr = (byte*)newStrPtr;
                        castBarAdditionalName->NodeText.BufSize = 1024;

                        ExpandNodeList(addon, 1);
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

        private static void ExpandNodeList(AtkUnitBase* atkUnitBase, ushort addSize)
        {
            var newNodeList = ExpandNodeList(atkUnitBase->UldManager.NodeList, atkUnitBase->UldManager.NodeListCount, (ushort)(atkUnitBase->UldManager.NodeListCount + addSize));
            atkUnitBase->UldManager.NodeList = newNodeList;
        }

        private static AtkResNode** ExpandNodeList(AtkResNode** originalList, ushort originalSize, ushort newSize = 0)
        {
            if (newSize <= originalSize) newSize = (ushort)(originalSize + 1);
            var oldListPtr = new IntPtr(originalList);
            var newListPtr = Alloc((ulong)((newSize + 1) * 8));
            var clone = new IntPtr[originalSize];
            Marshal.Copy(oldListPtr, clone, 0, originalSize);
            Marshal.Copy(clone, 0, newListPtr, originalSize);
            return (AtkResNode**)(newListPtr);
        }

        private static unsafe AtkTextNode* CloneNode(AtkTextNode* original)
        {
            var newAllocation = Alloc((ulong)sizeof(AtkTextNode));

            var bytes = new byte[sizeof(AtkTextNode)];
            Marshal.Copy(new IntPtr(original), bytes, 0, bytes.Length);
            Marshal.Copy(bytes, 0, newAllocation, bytes.Length);
            var newNode = (AtkTextNode*)newAllocation;

            newNode->AtkResNode.NextSiblingNode = (AtkResNode*)original;
            original->AtkResNode.PrevSiblingNode = (AtkResNode*)newNode;
            if (newNode->AtkResNode.PrevSiblingNode != null)
                newNode->AtkResNode.NextSiblingNode = (AtkResNode*)newNode;
            newNode->AtkResNode.ParentNode->ChildCount += 1;
            return newNode;
        }

        private static unsafe IntPtr Alloc(ulong size)
        {
            return new IntPtr(IMemorySpace.GetUISpace()->Malloc(size, 8UL));
        }

        private static unsafe IntPtr Alloc(int size)
        {
            if (size <= 0) throw new ArgumentException("Allocation size must be positive.");
            return Alloc((ulong)size);
        }
    }
}
