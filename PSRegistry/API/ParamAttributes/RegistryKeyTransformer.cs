using Microsoft.Win32;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.AccessControl;

namespace MartinGC94.PSRegistry.API.ParamAttributes
{
    public sealed class RegistryKeyTransformer : ArgumentTransformationAttribute
    {
        public RegistryKeyPermissionCheck PermissionCheck { get; }
        public RegistryRights RegistryRights { get; }
        public bool IsSingleKey { get; }

        public RegistryKeyTransformer(RegistryKeyPermissionCheck permissionCheck, RegistryRights registryRights, bool isSingleKey)
        {
            PermissionCheck = permissionCheck;
            RegistryRights = registryRights;
            IsSingleKey = isSingleKey;
        }

        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            return IsSingleKey
                ? ProcessSingleObjectInput(inputData)
                : ProcessMultiObjectInput(inputData);
        }

        private object ProcessSingleObjectInput(object inputData)
        {
            if (LanguagePrimitives.TryConvertTo(inputData, out RegistryKey _))
            {
                return inputData;
            }

            string inputPath = LanguagePrimitives.ConvertTo<string>(inputData);

            RegistryKey regKey = Util.GetRegKeyFromPath(inputPath, PermissionCheck, RegistryRights);
            return regKey;
        }

        private object ProcessMultiObjectInput(object inputData)
        {
            if (LanguagePrimitives.TryConvertTo(inputData, out RegistryKey[] _))
            {
                return inputData;
            }

            string[] inputPaths = LanguagePrimitives.ConvertTo<string[]>(inputData);
            var result = new List<RegistryKey>(inputPaths.Length);
            foreach (string path in inputPaths)
            {
                RegistryKey regKey = Util.GetRegKeyFromPath(path, PermissionCheck, RegistryRights);
                result.Add(regKey);
            }

            return result.ToArray();
        }
    }
}