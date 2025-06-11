using GooglePlayGamesLibrary;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace PlayServiceShim
{
    class Program
    {
        static string DataPath;
        //static string InstallationPath;
        static string serviceExe;
        static string serviceLib;
        static string serviceDir;

        //static Dictionary<string, Assembly> ServiceModules = new Dictionary<string, Assembly>();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            DelayedDepndencyWrapper();
        }

        static void DelayedDepndencyWrapper()
        {
            try
            {
                // Gather data from registry or detaults
                DataPath = GooglePlayGames.DataPath;
                //InstallationPath = GooglePlayGames.InstallationPath;
                serviceExe = GooglePlayGames.ServiceExecutablePath;
                serviceLib = GooglePlayGames.ServiceLibraryPath;
                //serviceDir = Path.GetDirectoryName(serviceExe);
                serviceDir = Path.GetDirectoryName(serviceLib);

                string battlestarLib = GooglePlayGames.BattlestarLibraryPath;

                // Local Data Files
                string appLibraryEncryptionKey = Path.Combine(DataPath, "app_library_encryption_key");
                string storeDb = Path.Combine(DataPath, "store.db");

                // Update environment so we can find needed DLLs
                Environment.SetEnvironmentVariable("PATH", serviceDir + @"\" + ";" + Environment.GetEnvironmentVariable("PATH"));

                Dictionary<string, Assembly> ServiceModules = new Dictionary<string, Assembly>();
                HashSet<string> AssembliesScanned = new HashSet<string>();

                // Reflect into the Google Play Games Service.exe
                Assembly serviceExeAsm = Assembly.LoadFrom(serviceExe);

                // load all the assemblies types we need might be in
                if (!string.IsNullOrWhiteSpace(serviceDir))
                {
                    Assembly targetAsm = serviceExeAsm;
                    Queue<Assembly> assemblies = new Queue<Assembly>();
                    for (; ; )
                    {
                        foreach (var assemblyName in targetAsm.GetReferencedAssemblies())
                        {
                            if (AssembliesScanned.Contains(assemblyName.Name))
                                continue;
                            AssembliesScanned.Add(assemblyName.Name);
                            var dllName = assemblyName.Name + ".dll";
                            string serviceDllPath = Path.Combine(serviceDir, dllName);
                            if (File.Exists(serviceDllPath))
                            {
                                if (!ServiceModules.ContainsKey(assemblyName.Name))
                                {
                                    Assembly asm = Assembly.LoadFrom(serviceDllPath);
                                    assemblies.Enqueue(asm);
                                    ServiceModules.Add(assemblyName.Name, asm);
                                }
                            }
                        }
                        if (assemblies.Count == 0)
                            break;
                        targetAsm = assemblies.Dequeue();
                    }
                }

                // find IDataStore Type
                Type IDataStoreType = null;
                foreach (var pair in ServiceModules)
                {
                    IDataStoreType = pair.Value.GetTypes().Where(dr => dr.Name == "IDataStore").FirstOrDefault();
                    if (IDataStoreType != null)
                        break;
                }

                // find possible implementations of IDataStore
                //List<Type> ImplementersOfIDataStoreType = new List<Type>();
                //foreach (var pair in ServiceModules)
                //{
                //    try
                //    {
                //        foreach (var item in pair.Value.GetTypes().Where(dr => dr.GetInterfaces().Any(dx => dx == IDataStoreType_)))
                //        {
                //            ImplementersOfIDataStoreType.Add(item);
                //        }
                //    }
                //    catch { }
                //}

                // find DataStore Type
                Type DataStoreType = null;
                foreach (var pair in ServiceModules)
                {
                    DataStoreType = pair.Value.GetTypes().Where(dr => dr.Name == "DataStore").FirstOrDefault();
                    if (DataStoreType != null)
                        break;
                }

                // find AppLibraryRowData Type
                Type AppLibraryRowDataType = null;
                foreach (var pair in ServiceModules)
                {
                    AppLibraryRowDataType = pair.Value.GetTypes().Where(dr => dr.Name == "AppLibraryRowData").FirstOrDefault();
                    if (AppLibraryRowDataType != null)
                        break;
                }

                // find AppLibraryModule Type
                Type AppLibraryModuleType = null;
                foreach (var pair in ServiceModules)
                {
                    AppLibraryModuleType = pair.Value.GetTypes().Where(dr => dr.Name == "AppLibraryModule").FirstOrDefault();
                    if (AppLibraryModuleType != null)
                        break;
                }

                // find BattlestarStartupEnvironment Type
                Type BattlestarStartupEnvironmentType = null;
                foreach (var pair in ServiceModules)
                {
                    try
                    {
                        BattlestarStartupEnvironmentType = pair.Value.GetTypes().Where(dr => dr.Name == "BattlestarStartupEnvironment").FirstOrDefault();
                        if (BattlestarStartupEnvironmentType != null)
                            break;
                    }
                    catch { }
                }

                // find IBattlestarStartupEnvironment Type
                Type IBattlestarEnvironmentType = null;
                foreach (var pair in ServiceModules)
                {
                    try
                    {
                        IBattlestarEnvironmentType = pair.Value.GetTypes().Where(dr => dr.Name == "IBattlestarEnvironment").FirstOrDefault();
                        if (IBattlestarEnvironmentType != null)
                            break;
                    }
                    catch { }
                }

                // find DefaultBattlestarEnvironmentImpl Type
                Type DefaultBattlestarEnvironmentImplType = null;
                foreach (var pair in ServiceModules)
                {
                    try
                    {
                        DefaultBattlestarEnvironmentImplType = pair.Value.GetTypes().Where(dr => dr.Name == "DefaultBattlestarEnvironmentImpl").FirstOrDefault();
                        if (DefaultBattlestarEnvironmentImplType != null)
                            break;
                    }
                    catch { }
                }

                // Execute reflected functions to read encrypted data from database and decrypt it
                object BattlestarStartupEnvironment = null;
                ConstructorInfo BattlestarStartupEnvironmentConstructor = BattlestarStartupEnvironmentType.GetConstructor(new Type[] {
                    typeof(System.String), typeof(System.String), typeof(System.String), typeof(System.String), typeof(System.String)
                });

                if (BattlestarStartupEnvironmentConstructor != null)
                    BattlestarStartupEnvironment = BattlestarStartupEnvironmentConstructor.Invoke(new object[] { null, null, null, null, GooglePlayGames.InstallationPath });

                if (BattlestarStartupEnvironment == null)
                {
                    MethodInfo BattlestarStartupEnvironment_CreateProd = BattlestarStartupEnvironmentType.GetMethod("CreateProd", BindingFlags.Static | BindingFlags.Public);
                    BattlestarStartupEnvironment = BattlestarStartupEnvironment_CreateProd.Invoke(null, new object[] { null, GooglePlayGames.DataPath });
                }

                if (BattlestarStartupEnvironment == null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(new { success = false, error = "No Battlestar Startup Environment" }));
                    return;
                }

                ConstructorInfo DefaultBattlestarEnvironmentImplConstructor = DefaultBattlestarEnvironmentImplType.GetConstructor(new Type[] { BattlestarStartupEnvironmentType });
                object DefaultBattlestarEnvironmentImpl = DefaultBattlestarEnvironmentImplConstructor.Invoke(new object[] { BattlestarStartupEnvironment });

                ConstructorInfo IBattlestarEnvironmentConstructor = DataStoreType.GetConstructor(new Type[] { IBattlestarEnvironmentType });
                //object DataStore = DataStoreType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { storeDb });
                object DataStore = IBattlestarEnvironmentConstructor.Invoke(new object[] { DefaultBattlestarEnvironmentImpl });

                object AppLibraryRowData = IDataStoreType.GetMethod("LoadAppLibrary", BindingFlags.Instance | BindingFlags.Public).Invoke(DataStore, null);
                object AppLibraryState = AppLibraryRowDataType.GetProperty("AppLibraryState", BindingFlags.Instance | BindingFlags.Public).GetValue(AppLibraryRowData);
                byte[] DataEncrypted = AppLibraryState as byte[];

                // Prepare the decryption key and decrypt the data
                byte[] EncKey = ProtectedData.Unprotect(File.ReadAllBytes(appLibraryEncryptionKey), null, DataProtectionScope.CurrentUser);
                byte[] DataDecrypted = Decrypt(EncKey, DataEncrypted);

                // Process decrpyted bytes into Library object via Service.exe
                object LibraryObject = null;
                MethodInfo ParseAndUpgradeFrom = AppLibraryModuleType.GetMethod("ParseAndUpgradeFrom", BindingFlags.Static | BindingFlags.NonPublic);
                try
                {
                    LibraryObject = ParseAndUpgradeFrom.Invoke(null, new object[] { DataDecrypted });
                }
                catch
                {
                    LibraryObject = ParseAndUpgradeFrom.Invoke(null, new object[] { DataEncrypted });
                }

                Console.WriteLine(JsonConvert.SerializeObject(new { success = true, data = LibraryObject }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(JsonConvert.SerializeObject(new { success = false, error = ex.ToString() }));
            }
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs e)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();

            // Get the Name of the AssemblyFile
            var assemblyName = new AssemblyName(e.Name);
            var dllName = assemblyName.Name + ".dll";

            if (!string.IsNullOrWhiteSpace(serviceDir))
            {
                string serviceDllPath = Path.Combine(serviceDir, dllName);
                if (File.Exists(serviceDllPath))
                {
                    try
                    {
                        return Assembly.Load(File.ReadAllBytes(serviceDllPath));
                    }
                    catch (IOException)
                    {
                    }
                    catch (BadImageFormatException)
                    {
                    }
                }
            }


            // Load from Embedded Resources - This function is not called if the Assembly is already
            // in the same folder as the app.
            var resources = thisAssembly.GetManifestResourceNames().Where(s => s.EndsWith(dllName));
            if (resources.Any())
            {

                // 99% of cases will only have one matching item, but if you don't,
                // you will have to change the logic to handle those cases.
                var resourceName = resources.First();
                using (var stream = thisAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;
                    var block = new byte[stream.Length];

                    // Safely try to load the assembly.
                    try
                    {
                        stream.Read(block, 0, block.Length);
                        return Assembly.Load(block);
                    }
                    catch (IOException)
                    {
                        return null;
                    }
                    catch (BadImageFormatException)
                    {
                        return null;
                    }
                }
            }

            // in the case the resource doesn't exist, return null.
            return null;
        }

        static byte[] Decrypt(byte[] _encryptionKey, byte[] b)
        {
            byte[] decryptedBuffer = new byte[BoringSslInterop.get_max_decrypted_length(b.Length)];
            int decryptedActualLength = 0;
            if (!BoringSslInterop.decrypt_string(b, b.Length, _encryptionKey, _encryptionKey.Length, decryptedBuffer, decryptedBuffer.Length, ref decryptedActualLength))
            {
                byte[] error = new byte[BoringSslInterop.last_error_length()];
                BoringSslInterop.get_last_error(error, error.Length);
                throw new CryptographicException("Error decrypting string: " + Encoding.UTF8.GetString(error));
            }
            byte[] decrypted = new byte[decryptedActualLength];
            Array.Copy(decryptedBuffer, decrypted, decryptedActualLength);
            return decrypted;
        }
        internal class BoringSslInterop
        {
            [DllImport("boringssl_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int last_error_length();

            [DllImport("boringssl_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            internal static extern void get_last_error(byte[] error, int len);

            [DllImport("boringssl_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int key_length();

            [DllImport("boringssl_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int get_max_encrypted_length(int plaintext_len);

            [DllImport("boringssl_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            internal static extern int get_max_decrypted_length(int encrypted_len);

            [DllImport("boringssl_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            internal static extern bool encrypt_string(byte[] plaintext, int len, byte[] key, int key_len, byte[] out_encrypted, int max_out_len, ref int out_actual_len);

            [DllImport("boringssl_wrapper.dll", CallingConvention = CallingConvention.Cdecl)]
            internal static extern bool decrypt_string(byte[] encrypted, int len, byte[] key, int key_lane, byte[] out_plaintext, int max_out_len, ref int out_actual_len);
        }
    }
}
