using JavaResolver;
using JavaResolver.Class;
using JavaResolver.Class.Code;
using JavaResolver.Class.Emit;
using JavaResolver.Class.Metadata.Attributes;
using JavaResolver.Class.TypeSystem;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using TIPatcher.Interfaces;

namespace TIPatcher
{
    public class Patcher(ILogger logger)
    {
        // constants
        private static string _tiPath => """C:\Program Files\TI Education\TI-Nspire CX CAS Student Software\lib\""";
        private static string _classPath => """com\ti\et\phoenix\jni\ApplWrapper.class""";
        private static string _jarName => "docfw.jar";
        private static string _tempDirname => "Temp";
        private static string[] _fieldsToPatch => ["isLicenseCheckEnabled", "isVerificationEnabled"];


        public string OutputDir { set; get; } = AppContext.BaseDirectory;
        public string TempDir { set; get; } = "";
        public string PathToJar { set; get; } = Path.Combine(_tiPath, _jarName);

        public bool Patch()
        {
            logger.Log("TIPatcher: Starting patch...");
            if (!File.Exists(PathToJar))
            {
                logger.Log("TIPatcher: Can't find docfw jar! You need to install TI-Nspire CX CAS Student Software.");
                return false;
            }

            if (!Directory.Exists(OutputDir))
            {
                logger.Log("TIPatcher: Output directory doesn't exist! Making directory...");
                Directory.CreateDirectory(OutputDir);
            }

            TempDir = Path.Combine(OutputDir, _tempDirname);

            if (Directory.Exists(TempDir))
            {
                Directory.Delete(TempDir, true);
            }
            Directory.CreateDirectory(TempDir);
            logger.Log($"TIPatcher: Created temporary directory {TempDir}");


            ZipFile.ExtractToDirectory(PathToJar, TempDir);

            if (File.Exists(Path.Combine(TempDir, "META-INF", "TI.RSA")))
            {
                File.Delete(Path.Combine(TempDir, "META-INF", "TI.RSA"));
            }
            if (File.Exists(Path.Combine(TempDir, "META-INF", "TI.SF")))
            {
                File.Delete(Path.Combine(TempDir, "META-INF", "TI.SF"));
            }
            logger.Log("TIPatcher: Removed signatures");

            var classFile = JavaClassFile.FromFile(Path.Combine(TempDir, _classPath));

            ByteOpCode opCode = ByteOpCodes.IConst_0;
            ByteOpCode opCode2 = ByteOpCodes.IReturn;

            foreach (var field in classFile.Methods)
            {
                string methodName = classFile.ConstantPool.ResolveString(field.NameIndex);
                if (_fieldsToPatch.Contains(methodName))
                {
                    logger.Log("TIPatcher: Patching " + methodName);

                    var codeAttrib = field.Attributes.First(a => classFile.ConstantPool.ResolveString(a.NameIndex) == CodeAttribute.AttributeName);

                    var contents = CodeAttribute.FromReader(new MemoryBigEndianReader(codeAttrib.Contents));

                    using (var stream = new MemoryStream())
                    {
                        var writer = new BigEndianStreamWriter(stream);
                        var asm = new ByteCodeAssembler(writer);

                        asm.Write(new ByteCodeInstruction(opCode));
                        asm.Write(new ByteCodeInstruction(opCode2));

                        contents.Code = stream.ToArray();
                    }

                    using (var attribStream = new MemoryStream())
                    {
                        var writer = new BigEndianStreamWriter(attribStream);
                        writer.Write((ushort)contents.MaxStack);
                        writer.Write((ushort)contents.MaxLocals);
                        writer.Write((uint)contents.Code.Length);
                        writer.Write(contents.Code);
                        writer.Write((ushort)0); 
                        writer.Write((ushort)0); 
                        codeAttrib.Contents = attribStream.ToArray();
                    }
                }
            }

            using (var patched = File.Create(Path.Combine(TempDir, _classPath)))
            {
                classFile.Write(patched);
            }

            if (File.Exists(Path.Combine(OutputDir, _jarName)))
            {
                File.Delete(Path.Combine(OutputDir, _jarName));
            }

            ZipFile.CreateFromDirectory(TempDir, Path.Combine(OutputDir, _jarName));
            logger.Log("TIPatcher: Created patched jar");

            logger.Log($"\nTIPatcher: \n\tPatching completed. \n\tPatched jar is in {Path.Combine(OutputDir,_jarName)}. \n\tCopy it to {_tiPath}, overwriting the existing {_jarName}");

            return true;
        }
    }
}
