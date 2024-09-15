using p5rpc.events.saepov.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Numerics;
using System.Runtime.InteropServices;
using static p5rpc.events.saepov.ModelResourceHelper;

namespace p5rpc.events.saepov;

internal unsafe class ModelFunctions
{
    private ModContext _context;
    private ILogger _logger;
    private IReloadedHooks _hooks;
    private IStartupScanner _scanner;

    internal delegate ModelNodeInfo* d_ModelGetNodeFromName(void* modelNodeData, string nodeName);
    internal d_ModelGetNodeFromName ModelGetNodeFromName;

    internal delegate ModelData* d_ModelGetData(ModelResource* modelResource);
    internal d_ModelGetData ModelGetData;

    internal ModelResource** ModelResourceList { get; private set; }

    internal ModelFunctions(ModContext context, IStartupScanner scanner)
    {
        _context = context;
        _logger = _context.Logger;
        _hooks = _context.Hooks;
        _scanner = scanner;
 

        _scanner.AddMainModuleScan(@"48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 89 D6 48 89 CF",
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

                ModelResourceList = (ModelResource**)Utils.GetAddressFromGlobalRef(modelResourceListAccessor, 7);
            }
        });
    }

    internal bool TrySearchCharacterModelResource(short majorId, short minorId, sbyte subId, out List<ModelResourceHelper> models)
    {
        models = new List<ModelResourceHelper>();

        var block = ModelResourceList;
        for (var i = 0; i < 0x20; i++)
        {
            if (*block == null)
            {
                block++;
                continue;
            }

            for (var current = *block; current != null; current = current->Next)
            {
                var modelIds = current->ModelIds;

                if ((GetMajorId(current) != majorId && majorId != -1) || (GetMinorId(current) != minorId && minorId != -1) || (GetSubId(current) != subId && subId != -1))
                    continue;

                models.Add(new ModelResourceHelper(current));
            }

            block++;
        }

        return models.Count != 0;
    }

    internal bool TryGetModelNodeFromName(void* modelNodeData, string nodeName, out ModelNodeInfo* modelNodeInfo)
    {
        modelNodeInfo = null;

        if (modelNodeData != null)
            modelNodeInfo = ModelGetNodeFromName(modelNodeData, nodeName);

        return modelNodeInfo != null;
    }

    internal bool TryGetModelData(ModelResource* resource, out ModelData* modelData)
    {
        modelData = null;

        if (resource != null)
            modelData = ModelGetData(resource);

        return modelData != null;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x2d0)]
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
    internal Matrix4x4 TransformationMatrix;

    [FieldOffset(0x50)]
    internal Vector3 WorldTranslate;
}

[StructLayout(LayoutKind.Explicit)]
unsafe struct ModelData
{
    [FieldOffset(0x28)]
    internal void* NodeInfo;
}


