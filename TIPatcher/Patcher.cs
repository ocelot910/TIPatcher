using JavaResolver;
using JavaResolver.Class;
using JavaResolver.Class.Code;
using JavaResolver.Class.Emit;
using JavaResolver.Class.Metadata.Attributes;
using JavaResolver.Class.TypeSystem;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using TIPatcher.Interfaces;

namespace TIPatcher
{
    public class Patcher(ILogger logger)
    {
        // constants
        private static string _tiUrl => """https://education.ti.com/en/software/details/en/36BE84F974E940C78502AA47492887AB/ti-nspirecxcas_pc_full""";
        private static string _tiPath
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        ".wine",
                        "drive_c",
                        "Program Files",
                        "TI Education",
                        "TI-Nspire CX CAS Student Software");
                }
                // assume Windows
                return """C:\Program Files\TI Education\TI-Nspire CX CAS Student Software\""";
            }
        }
        private static string _libPath => Path.Combine(_tiPath, "lib");
        private static string _tiName => "TI-Nspire CX CAS Student Software";
        private static string _tiExeName => _tiName + ".exe";
        private static string _classPath => Path.Combine("com", "ti", "et", "phoenix", "jni", "ApplWrapper.class");
        private static string _jarName => "docfw.jar";
        private static string _tempDirname => "Temp";
        private static string[] _fieldsToPatch => ["isLicenseCheckEnabled", "isVerificationEnabled"];


        // properties
        public string OutputDir { set; get; } = AppContext.BaseDirectory;
        public string TempDir { set; get; } = "";
        public string PathToJar { set; get; } = Path.Combine(_libPath, _jarName);

        public bool Patch()
        {
            logger.Log("TIPatcher: Starting patch...");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                logger.Log("TIPatcher: TIPatcher doesn't support MacOS! Quitting...");
                return false;
            }

            if (!File.Exists(PathToJar))
            {
                logger.Log($"TIPatcher: Can't find docfw jar at {PathToJar}! You need to install " + _tiName);

                logger.Log("TIPatcher: Do you want to open the download page for " + _tiName + "? (y/n)");
                string response = logger.Ask();

                if (response.Contains('y'))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(_tiUrl) { UseShellExecute = true });
                        logger.Log("TIPatcher: Opened download page. Select the installer for Windows.");
                    }
                    catch
                    {
                        logger.Log("TIPatcher: Failed to launch download page!");
                    }
                }
                return false;
            }

            if (!Directory.Exists(OutputDir))
            {
                logger.Log("TIPatcher: Output directory doesn't exist! Making directory...");
                Directory.CreateDirectory(OutputDir);
            }

            TempDir = Path.Combine(OutputDir, _tempDirname);
            logger.Log($"TIPatcher: Using temporary directory {TempDir}");

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

            ByteOpCode opCode0 = ByteOpCodes.IConst_0;
            ByteOpCode opCodeRet = ByteOpCodes.IReturn;

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

                        asm.Write(new ByteCodeInstruction(opCode0));
                        asm.Write(new ByteCodeInstruction(opCodeRet));

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
            logger.Log($"TIPatcher: Patching completed.");
            

            if (Path.Exists(_libPath))
            {
                logger.Log($"TIPatcher: Do you want to copy the patched file automatically to {_libPath}?");
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    logger.Log($"TIPatcher: This will require administrator privileges.");
                }
                logger.Log($"TIPatcher: Copy? (y/n)");
                string response = logger.Ask();

                if (response.Contains('y'))
                {
                    Process[] processes = Process.GetProcessesByName(_tiName);
                    foreach (Process process in processes)
                    {
                        logger.Log("TIPatcher: " + _tiName + " is running! Stopping " + _tiExeName);

                        try
                        {
                            process.Kill();
                            process.WaitForExit();
                            logger.Log("TIPatcher: Successfully killed " + _tiExeName);
                        }
                        catch
                        {
                            logger.Log("TIPatcher: Failed to kill " + _tiExeName + "!");
                        }
                    }


                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        logger.Log("TIPatcher: Linux detected!");

                        try
                        {
                            File.Copy(
                                Path.Combine(OutputDir, _jarName),
                                Path.Combine(_libPath, _jarName),
                                overwrite: true
                            );

                            logger.Log("TIPatcher: Patched successfully!");

                            logger.Log("TIPatcher: Do you want to launch " + _tiName + "? (y/n)");

                            string startTi = logger.Ask();

                            if (startTi.Contains('y'))
                            {
                                if (File.Exists(Path.Combine(_tiPath, _tiExeName)))
                                {
                                    logger.Log("TIPatcher: Starting " + _tiExeName);

                                    ProcessStartInfo tiPSI = new("wine")
                                    {
                                        Arguments = $"\"{Path.Combine(_tiPath, _tiExeName)}\""
                                    };

                                    try
                                    {
                                        Process? tiProcess = Process.Start(tiPSI);
                                    }
                                    catch
                                    {
                                        logger.Log("TIPatcher: Failed to start " + _tiExeName + "!");
                                        logger.Log("TIPatcher: This may be due to your Wine configuration");
                                        return false;
                                    }
                                }
                            }
                        }
                        catch (DirectoryNotFoundException)
                        {
                            logger.Log("TIPatcher: Directory not found (Wine)");
                            return false;
                        }
                        catch
                        {
                            logger.Log("TIPatcher: An error occurred while copying the file.");
                            return false;
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        ProcessStartInfo psi = new()
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c copy /Y \"{Path.Combine(OutputDir, _jarName)}\" \"{_libPath}\"",
                            Verb = "runas",
                            UseShellExecute = true
                        };

                        try
                        {
                            Process? p = Process.Start(psi);
                            p?.WaitForExit();
                            if (p?.ExitCode == 0)
                            {
                                logger.Log("TIPatcher: Copying successful!");

                                logger.Log("TIPatcher: Do you want to launch " + _tiName + "? (y/n)");

                                string startTi = logger.Ask();

                                if (startTi.Contains('y'))
                                {
                                    if (File.Exists(Path.Combine(_tiPath, _tiExeName)))
                                    {
                                        logger.Log("TIPatcher: Starting " + _tiExeName);

                                        ProcessStartInfo tiPSI = new(Path.Combine(_tiPath, _tiExeName));
                                        try
                                        {
                                            Process? tiProcess = Process.Start(tiPSI);
                                        }
                                        catch
                                        {
                                            logger.Log("TIPatcher: Failed to start " + _tiExeName + "!");
                                            return false;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                logger.Log($"TIPatcher: Copy failed. Exit code is {p?.ExitCode}");
                                return false;
                            }
                        }
                        catch (Win32Exception)
                        {
                            logger.Log("TIPatcher: UAC is required to copy to the TI directory, as it is write-protected!");
                            return false;
                        }
                    }
                }
                else
                {
                    logger.Log($"TIPatcher: Patched jar is in {Path.Combine(OutputDir, _jarName)}.");
                    logger.Log($"TIPatcher: Copy it to {_libPath}, overwriting the existing {_jarName}");
                }
            }

            return true;
        }
    }
}
