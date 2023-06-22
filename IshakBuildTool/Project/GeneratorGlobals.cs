using IshakBuildTool.ProjectFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IshakBuildTool.Project
{
    enum EVCFileType
    {
        None,
        ClCompile,
        ClInclude
    }

    internal class GeneratorGlobals
    {
        static public EVCFileType GetVCFileTypeForFile(FileReference file)
        {
            switch (file.FileType)
            {
                case EFileType.Header:
                    return EVCFileType.ClInclude;

                case EFileType.Source:
                    return EVCFileType.ClCompile;

                default: return EVCFileType.None;
            }
        }

        public static Guid BuildGUID(string projectPath, string projectName)
        {
            string PathForMakingGUID = string.Format("{0}/{1}", projectPath, projectName);
            return MakeMd5Guid(Encoding.UTF8.GetBytes(PathForMakingGUID));
        }

        public static Guid BuildGuid(string hashStr)
        {
            return MakeMd5Guid(Encoding.UTF8.GetBytes(hashStr));
        }

        static Guid MakeMd5Guid(byte[] Input)
        {
            byte[] Hash = MD5.Create().ComputeHash(Input);
            Hash[6] = (byte)(0x30 | Hash[6] & 0x0f); // 0b0011'xxxx Version 3 UUID (MD5)
            Hash[8] = (byte)(0x80 | Hash[8] & 0x3f); // 0b10xx'xxxx RFC 4122 UUID
            Array.Reverse(Hash, 0, 4);
            Array.Reverse(Hash, 4, 2);
            Array.Reverse(Hash, 6, 2);
            return new Guid(Hash);
        }

    }
}
