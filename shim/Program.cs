using GooglePlayGamesLibrary;
using Newtonsoft.Json;
using System;
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
        static string serviceDir;
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
                serviceDir = Path.GetDirectoryName(serviceExe);

                // Local Data Files
                string appLibraryEncryptionKey = Path.Combine(DataPath, "app_library_encryption_key");
                string storeDb = Path.Combine(DataPath, "store.db");

                // Update environment so we can find needed DLLs
                Environment.SetEnvironmentVariable("PATH",  serviceDir + @"\" + ";" + Environment.GetEnvironmentVariable("PATH"));

                // Reflect into the Google Play Games Service.exe
                Assembly serviceAsm = Assembly.LoadFrom(serviceExe);
                Type AppFactoryType = serviceAsm.GetTypes().Where(dr => dr.Name == "AppFactory").FirstOrDefault();
                Type DataStoreType = serviceAsm.GetTypes().Where(dr => dr.Name == "DataStore").FirstOrDefault();
                Type IDataStoreType = serviceAsm.GetTypes().Where(dr => dr.Name == "IDataStore").FirstOrDefault();
                Type AppLibraryRowDataType = serviceAsm.GetTypes().Where(dr => dr.Name == "AppLibraryRowData").FirstOrDefault();
                Type AppLibraryModuleType = serviceAsm.GetTypes().Where(dr => dr.Name == "AppLibraryModule").FirstOrDefault();

                // Execute reflected functions to read encrypted data from database and decrypt it
                object DataStore = DataStoreType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { storeDb });
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

                Console.WriteLine(JsonConvert.SerializeObject(LibraryObject));
            }
            catch (Exception ex)
            {
                Console.WriteLine("null");
                Console.WriteLine(ex.ToString());
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
