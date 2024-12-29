using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
using System;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

namespace OnTheFlyTranslator
{
    public abstract unsafe class UIModifier: IDisposable
    {
        public UIModifier(string targetAddon)
        {
            DalamudApi.AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, targetAddon, HandlePostRefreshEvent);
            DalamudApi.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, targetAddon, HandlePreDrawEvent);
        }

        public void Dispose()
        {
            DalamudApi.AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, HandlePostRefreshEvent);
            DalamudApi.AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, HandlePreDrawEvent);
        }

        protected virtual void HandlePostRefreshEvent(AddonEvent type, AddonArgs args)
        {

        }

        protected virtual void HandlePreDrawEvent(AddonEvent type, AddonArgs args)
        {

        }
    }

    public unsafe class UIManipulationHelper
    {
        public static AtkResNode** ExpandNodeList(AtkResNode** originalList, ushort originalSize, ushort newSize = 0)
        {
            if (newSize <= originalSize) newSize = (ushort)(originalSize + 1);
            var oldListPtr = new IntPtr(originalList);
            var newListPtr = Alloc((ulong)((newSize + 1) * 8));
            var clone = new IntPtr[originalSize];
            Marshal.Copy(oldListPtr, clone, 0, originalSize);
            Marshal.Copy(clone, 0, newListPtr, originalSize);
            return (AtkResNode**)(newListPtr);
        }

        public static void ExpandNodeList(AtkUnitBase* atkUnitBase, ushort addSize)
        {
            var newNodeList = ExpandNodeList(atkUnitBase->UldManager.NodeList, atkUnitBase->UldManager.NodeListCount, (ushort)(atkUnitBase->UldManager.NodeListCount + addSize));
            atkUnitBase->UldManager.NodeList = newNodeList;
        }

        public static unsafe AtkTextNode* CloneNode(AtkTextNode* original)
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

        public static unsafe IntPtr Alloc(ulong size)
        {
            return new IntPtr(IMemorySpace.GetUISpace()->Malloc(size, 8UL));
        }

        public static unsafe IntPtr Alloc(int size)
        {
            if (size <= 0) throw new ArgumentException("Allocation size must be positive.");
            return Alloc((ulong)size);
        }
    }
}
