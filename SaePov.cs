using CriFs.V2.Hook.Interfaces;
using p5rpc.events.saepov.Template;
using p5rpc.lib.interfaces;
using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using static p5rpc.events.saepov.Utils;

namespace p5rpc.events.saepov;

internal unsafe class SaePov
{
    private ModelFunctions _gameFunctions;

    private delegate void CamPosFunc1(long a1, long a2);
    private IHook<CamPosFunc1> _camPosFunc1Hook;

    private ModContext _modContext;
    private IStartupScanner _scanner;
    private ILogger _logger;
    private IFlowCaller _flowCaller;
    private IP5RLib _p5rLib;
    private ICriFsRedirectorApi _criFsRedirectorApi;

    private long _fmwkAdr = 0;
    internal SaePov(ModContext context)
    {
        var hooks = context.Hooks;
        _logger = context.Logger;

        _modContext = context;
        _modContext.ModLoader.GetController<IStartupScanner>().TryGetTarget(out _scanner);
        _modContext.ModLoader.GetController<IP5RLib>().TryGetTarget(out _p5rLib);
        _modContext.ModLoader.GetController<ICriFsRedirectorApi>().TryGetTarget(out _criFsRedirectorApi);

        AddFixFiles();

        _flowCaller = _p5rLib.FlowCaller;

        _gameFunctions = new ModelFunctions(_modContext, _scanner);

        var evtCamPosSig = @"48 85 D2 0F 84 ?? ?? ?? ?? 48 8B C4 55 48 8D A8 ?? ?? ?? ??";

        _scanner.AddMainModuleScan(evtCamPosSig, result =>
        {
            if (!result.Found)
            {
                throw new Exception("Could not find EventCamPos Function.");
            }

            var adr = BaseAddress + result.Offset;

            _camPosFunc1Hook = hooks.CreateHook<CamPosFunc1>((a1, a2) =>
            {
                _camPosFunc1Hook.OriginalFunction(a1, a2);

                var POVMajorId = _modContext.Configuration.PovModelMajorId;
                if (_gameFunctions.TrySearchCharacterModelResource(POVMajorId, -1, -1, out var niijimaResources)) // -1 is a wildcard
                {
                    foreach (var niijimaResource in niijimaResources)
                    {
                        if (_gameFunctions.TryGetModelData(niijimaResource.ModelResource, out var niijimaModelData)
                        && _gameFunctions.TryGetModelNodeFromName(niijimaModelData->NodeInfo, "lookat", out var niijimaLookNode)
                        && _gameFunctions.TryGetModelNodeFromName(niijimaModelData->NodeInfo, "b l hana01", out var niijimaFaceNode)
                        && niijimaResource.IsVisible)
                        {
                            var niijimaTranslate = &niijimaFaceNode->WorldTranslate;
                            var niijimaLook = &niijimaLookNode->WorldTranslate;

                            var rot = GetRotationBetweenPoints(*niijimaTranslate, *niijimaLook);
                            var distance = _modContext.Configuration.CamDistance;
                            var fovy = _modContext.Configuration.CamFOV;

                            MoveCameraTowardsPoint(ref *niijimaTranslate, *niijimaLook, distance);
                            _flowCaller.FLD_CAMERA_SET_POS(niijimaTranslate->X, niijimaTranslate->Y + 3.5f, niijimaTranslate->Z);
                            _flowCaller.FLD_CAMERA_SET_ROT(rot.X, rot.Y, rot.Z, rot.W);
                            _flowCaller.FLD_CAMERA_SET_FOVY(fovy);
                        }
                    }
                }
            }, adr).Activate();
        });
    }

    internal void AddFixFiles()
    {
        var config = _modContext.Configuration;

        if (config.EventFixes)
            _criFsRedirectorApi.AddProbingPath(@"Fixes\EventFixes");

        if (config.HatRemoval)
            _criFsRedirectorApi.AddProbingPath(@"Fixes\RemoveShadowSaeHat");
    }
}
