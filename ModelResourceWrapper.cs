namespace p5rpc.events.saepov;

internal unsafe class ModelResourceWrapper
{
    internal ModelResourceWrapper(ModelResource* model)
    {
        ModelResource = model;
    }


    internal ModelResource* ModelResource { get; init; }
    internal long ModelIds { get { return ModelResource->ModelIds; } }

    internal short MajorId { get { return (short)((UpperFour(ModelIds) >> 0x14) & 0xffff); } }
    internal short MinorId { get { return (short)(*(int*)ModelIds & 0xfff); } }
    internal sbyte SubId { get { return (sbyte)((*(int*)ModelIds >> 0xc) & 0xff); } }

    internal bool IsVisible { get { return ModelResource->Unk12 != 0; } }

    int UpperFour(long value) => (int)(value >> 0x20);

    int LowerFour(long value) => (int)(value & 0xffff);
}