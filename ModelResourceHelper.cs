namespace p5rpc.events.saepov;

internal unsafe class ModelResourceHelper
{
    internal ModelResourceHelper(ModelResource* model)
    {
        ModelResource = model;
    }


    internal ModelResource* ModelResource { get; init; }
    internal long ModelIds { get => ModelResource->ModelIds; }
    internal short MajorId { get => GetMajorId(ModelResource);}
    internal short MinorId { get => GetMinorId(ModelResource);}
    internal sbyte SubId { get => GetSubId(ModelResource);}

    internal bool IsVisible { get => GetVisible(ModelResource); }

    static int UpperFour(long value) => (int)(value >> 0x20);

    static int LowerFour(long value) => (int)(value & -1);

    internal static short GetMajorId(ModelResource* model) => (short)((*(int*)model >> 0x14) & 0xffff);
    internal static short GetMinorId(ModelResource* model) => (short)(*(int*)model & 0xfff);
    internal static sbyte GetSubId(ModelResource* model) => (sbyte)((*(int*)model >> 0xc) & 0xff);
    internal static bool GetVisible(ModelResource* model) => model->Unk12 != 0;
}
