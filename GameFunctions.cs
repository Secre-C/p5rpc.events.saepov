using p5rpc.events.saepov.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Numerics;
using System.Runtime.InteropServices;

namespace p5rpc.events.saepov;

internal unsafe class GameFunctions
{
    private ModContext _context;
    private ILogger _logger;
    private IReloadedHooks _hooks;
    private IStartupScanner _scanner;

    internal delegate Vector3* d_ModelGetTranslate(ModelResource* modelResource);
    internal d_ModelGetTranslate ModelGetTranslate;

    internal delegate Quaternion* d_ModelGetRotate(ModelResource* modelResource);
    internal d_ModelGetRotate ModelGetRotate;

    internal delegate ModelNodeInfo* d_ModelGetNodeFromName(void* modelNodeData, string nodeName);
    internal d_ModelGetNodeFromName ModelGetNodeFromName;

    internal delegate ModelData* d_ModelGetData(ModelResource* modelResource);
    internal d_ModelGetData ModelGetData;

    internal ModelResource* ModelResourceList { get; private set; }

    internal GameFunctions(ModContext context, IStartupScanner scanner)
    {
        _context = context;
        _logger = _context.Logger;
        _hooks = _context.Hooks;
        _scanner = scanner;

        _scanner.AddMainModuleScan(@"48 8B D1 48 85 C9 74 ?? 48 8B 49 ?? 0F B6 C1 C0 E8 05 A8 01 75 ?? 4C 8B 02 49 C1 E8 3A 41 83 F8 07 77 ?? 48 8B C1 48 C1 E8 09 A8 01 74 ?? 48 C1 E9 12 F6 C1 01 74 ?? 33 C0 C3 F6 C1 01 74 ?? 41 8D 40 ?? 83 F8 1E 77 ?? 4C 8D 05 ?? ?? ?? ?? 41 0F B6 84 ?? ?? ?? ?? ?? 41 8B 8C ?? ?? ?? ?? ?? 49 03 C8 FF E1 48 8B 82 ?? ?? ?? ?? 48 85 C0 74 ?? 48 8B 40 ?? 48 05 A0 00 00 00",
            result =>
            {
                if (result.Found)
                {
                    var adr = Utils.BaseAddress + result.Offset;
                    _logger.WriteLine($"Found ModelGetTranslate function at 0x{adr:X8}");
                    ModelGetTranslate = _hooks.CreateWrapper<d_ModelGetTranslate>(adr, out var wrapperAddress);
                }
                else
                {
                    throw new Exception("Could not find ModelGetTranslate Function.");
                }
            });

        _scanner.AddMainModuleScan(@"48 8B D1 48 85 C9 74 ?? 48 8B 49 ?? 0F B6 C1 C0 E8 05 A8 01 75 ?? 4C 8B 02 49 C1 E8 3A 41 83 F8 07 77 ?? 48 8B C1 48 C1 E8 09 A8 01 74 ?? 48 C1 E9 12 F6 C1 01 74 ?? 33 C0 C3 F6 C1 01 74 ?? 41 8D 40 ?? 83 F8 1E 77 ?? 4C 8D 05 ?? ?? ?? ?? 41 0F B6 84 ?? ?? ?? ?? ?? 41 8B 8C ?? ?? ?? ?? ?? 49 03 C8 FF E1 48 8B 82 ?? ?? ?? ?? 48 85 C0 74 ?? 48 8B 40 ?? 48 05 B0 00 00 00",
            result =>
            {
                if (result.Found)
                {
                    var adr = Utils.BaseAddress + result.Offset;
                    _logger.WriteLine($"Found ModelGetRotate function at 0x{adr:X8}");
                    ModelGetRotate = _hooks.CreateWrapper<d_ModelGetRotate>(adr, out var wrapperAddress);
                }
                else
                {
                    throw new Exception("Could not find ModelGetRotate Function.");
                }
            });

        _scanner.AddMainModuleScan(@"40 53 48 83 EC 20 48 89 D3 49 89 C8",
            result =>
            {
                if (result.Found)
                {
                    var adr = Utils.BaseAddress + result.Offset;
                    _logger.WriteLine($"Found ModelGetNodeFromName function at 0x{adr:X8}");
                    ModelGetNodeFromName = _hooks.CreateWrapper<d_ModelGetNodeFromName>(adr, out var wrapperAddress);
                }
                else
                {
                    throw new Exception("Could not find ModelGetNodeFromName Function.");
                }
            });

        _scanner.AddMainModuleScan(@"48 8B 11 48 C1 EA 3A FF CA 83 FA 1E 77 ?? 4C 8D 05 ?? ?? ?? ?? 41 0F B6 84 ?? ?? ?? ?? ?? 41 8B 94 ?? ?? ?? ?? ?? 49 03 D0 FF E2 48 83 B9 ?? ?? ?? ?? 00 75 ?? 48 8B 81 ?? ?? ?? ?? C3 48 8B 81 ?? ?? ?? ?? C3 48 8B 81 ?? ?? ?? ?? C3 80 B9 ?? ?? ?? ?? 00",
            result =>
            {
                if (result.Found)
                {
                    var adr = Utils.BaseAddress + result.Offset;
                    _logger.WriteLine($"Found ModelGetData function at 0x{adr:X8}");
                    ModelGetData = _hooks.CreateWrapper<d_ModelGetData>(adr, out var wrapperAddress);
                }
                else
                {
                    throw new Exception("Could not find ModelGetData Function.");
                }
            });

        GetModelResourceList();
    }

    private void GetModelResourceList()
    {
        var modelResourceListAccessorPattern = @"48 8D 15 ?? ?? ?? ?? 4C 8D 05 ?? ?? ?? ?? 0F 1F 00 48 8B 0A 48 85 C9 74 ?? 39 41 ?? 74 ?? 48 8B 89 ?? ?? ?? ?? 48 85 C9 75 ?? 48 83 C2 08 49 3B D0 7C ?? 48 8B 05 ?? ?? ?? ?? C7 80 ?? ?? ?? ?? 00 00 00 00 C6 40 ?? 01 B8 01 00 00 00 48 83 C4 28 C3 48 85 C9 74 ?? E8 ?? ?? ?? ?? F3 0F 10 00";

        _scanner.AddMainModuleScan(modelResourceListAccessorPattern, result =>
        {
            if (result.Found)
            {
                var modelResourceListAccessor = Utils.BaseAddress + result.Offset;

                ModelResourceList = (ModelResource*)Utils.GetAddressFromGlobalRef(modelResourceListAccessor, 7);
            }
        });
    }
    internal bool TrySearchCharacterModelResource(short majorId, short minorId, byte subId, out List<ModelResourceWrapper> models)
    {
        models = new List<ModelResourceWrapper>();

        var block = (long)ModelResourceList;
        for (var i = 0; i < 0x20; i++)
        {
            if (*(long*)block == 0)
            {
                block += 8;
                continue;
            }

            for (var current = *(ModelResource**)block; current != null; current = current->Next)
            {
                var modelIds = current->ModelIds;

                var maj = (short)((*(int*)current >> 0x14) & 0xffff);
                var min = (short)(*(int*)current & 0xfff);
                var sub = (byte)((*(int*)current >> 0xc) & 0xff);

                if ((maj != majorId && majorId != -1) || (min != minorId && minorId != -1) || (sub != subId && subId != byte.MaxValue))
                    continue;

                models.Add(new ModelResourceWrapper(current));
            }

            block += 8;
        }

        return models.Count != 0;
    }

    internal bool TryGetModelNodeFromName(void* modelNodeData, string nodeName, out ModelNodeInfo* modelNodeInfo)
    {
        modelNodeInfo = ModelGetNodeFromName(modelNodeData, nodeName);
        return modelNodeInfo != null;
    }

    internal bool TryGetModelData(ModelResource* resource, out ModelData* modelData)
    {
        modelData = ModelGetData(resource);
        return modelData != null;
    }
}

[StructLayout(LayoutKind.Explicit)]
unsafe struct ModelResource
{
    [FieldOffset(0)]
    internal long ModelIds;

    [FieldOffset(8)]
    internal int Handle;

    [FieldOffset(0x12)]
    internal byte Unk12;

    [FieldOffset(0x18)]
    internal Vector3 Translate;

    [FieldOffset(0x2c8)]
    internal ModelResource* Next;
}

[StructLayout(LayoutKind.Explicit)]
struct ModelNodeInfo
{
    [FieldOffset(0x20)]
    internal Quaternion Rot1;
    [FieldOffset(0x30)]
    internal Quaternion Rot2;
    [FieldOffset(0x40)]
    internal Quaternion Rot3;

    [FieldOffset(0x50)]
    internal Vector3 WorldTranslate;
}

[StructLayout(LayoutKind.Explicit)]
unsafe struct ModelData
{
    [FieldOffset(0x28)]
    internal void* NodeInfo;
}