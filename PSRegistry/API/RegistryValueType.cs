namespace MartinGC94.PSRegistry.API
{
    public enum RegistryValueType : uint
    {
        None = 0,
        String = 1,
        ExpandString = 2,
        Binary = 3,
        DWord = 4,
        MultiString = 7,
        QWord = 11,
        UwpInt32 = 100000004,
        UwpUint32 = 100000005,
        UwpBool = 100000011,
        UwpString = 100000012,
        UwpDateTimeOffset = 100000014
    }
}