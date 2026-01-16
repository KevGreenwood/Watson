using Microsoft.Win32;
using System;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace Watson
{
    public partial class MainWindow : FluentWindow
    {
        public string key = KeyFinder.GetWindowsKey62();

        public string censoredKey;
        private bool isCensored = true;

        public MainWindow()
        {
            InitializeComponent();

            //Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
              Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
              WindowBackdropType.Auto,  // Background type
              true                                      // Whether to change accents automatically
            );

            WindowsHandler.InitializeAsync();
            productName.Text = WindowsHandler.ProductName;
            productID.Text = WindowsHandler.ProductID;
            buildVersion.Text = WindowsHandler.Version;
            softKey.Text = WindowsHandler.licenseKey != string.Empty ? WindowsHandler.licenseKey : "Not found";
            var splitted = key.Split('-');
            for (int i = 0; i < splitted.Length-1; i++)
            {
                splitted[i] = "XXXXX";
            }

           censoredKey = string.Join("-", splitted);

            oemKey.Text = censoredKey;
            oemEdition.Text = WindowsHandler.oemDescription;
        }

        private void oemKey_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                return;

            isCensored = !isCensored;

            oemKey.Text = isCensored
                ? censoredKey
                : key;

        }
    }

    public static class WindowsHandler
    {
        public static RegistryKey WindowsRK = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false);
        public static string UBR { get; set; }
        public static string Version { get; set; }
        public static string ActID { get; set; }
        public static string ProductID = WindowsRK.GetValue("ProductId").ToString();

        public static string ProductKeyID = Encoding.Unicode.GetString((byte[])WindowsRK.GetValue("DigitalProductId4"), 0x08, 0x64);
        public static string ProductKey = Encoding.Unicode.GetString((byte[])WindowsRK.GetValue("DigitalProductId4"), 0x3F8, 0x80);
        public static string oemDescription { get; set; }
        public static string licenseKey { get; private set; }
        public static string ProductName = WindowsRK.GetValue("ProductName").ToString();
        public static string EditionID = WindowsRK.GetValue("EditionID").ToString();
        public static float CurrentVersion = float.Parse(WindowsRK.GetValue("CurrentVersion").ToString()) / 10f;
        public static int Build = Convert.ToInt32(WindowsRK.GetValue("CurrentBuildNumber").ToString());
        //public static string GetMinimalInfo = $"{ProductName} {Platform}";
        public static string GetAllInfo { get; private set; }
        private static string Platform = Environment.Is64BitOperatingSystem ? "64 bits" : "32 bits";
        private static string DisplayVersion { get; set; }

        public static async Task InitializeAsync()
        {
            switch (CurrentVersion)
            {
                case 6.1f:
                    UBR = WindowsRK.GetValue("CSDVersion")?.ToString() ?? string.Empty;
                    Version = $"{UBR} ({Build})";
                    break;

                case 6.2f:
                    Version = Build.ToString();
                    break;

                case 6.3f:
                    UBR = WindowsRK.GetValue("UBR")?.ToString() ?? string.Empty;

                    if (ProductName.Contains("8.1"))
                    {
                        Version = $"{Build}.{UBR}";
                    }
                    else
                    {
                        DisplayVersion = WindowsRK.GetValue("DisplayVersion").ToString();
                        Version = $"{DisplayVersion} ({Build}.{UBR})";

                        if (Build >= 22000)
                        {
                            ProductName = ProductName.Replace("Windows 10", "Windows 11");
                        }


                        //GetMinimalInfo = $"{ProductName} {DisplayVersion} {Platform}";
                    }
                    break;
            }



            if (ProductName.Contains("Enterprise LTSB 2016"))
            {
                EditionID = "EnterpriseSB";
            }
            else if (ProductName.Contains("Enterprise N LTSB 2016"))
            {
                EditionID = "EnterpriseSNB";
            }

            if (EditionID == "ServerRdsh" && Build <= 17134)
            {
                EditionID = "ServerRdsh134";
            }

            GetAllInfo = $"{ProductName} {Platform}";



            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ID FROM SoftwareLicensingProduct WHERE (ApplicationID='55c92734-d682-4d71-983e-d6ec3f16059f' AND PartialProductKey <> NULL)");
            ActID = searcher.Get().Cast<ManagementObject>()
                                 .FirstOrDefault()?["ID"] as string ?? string.Empty;
            GetLicenseKey();
            //WindowsRK.Close();
        }

        public static void GetLicenseKey()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT OA3xOriginalProductKey FROM SoftwareLicensingService");
            licenseKey = searcher.Get().Cast<ManagementObject>()
                                 .FirstOrDefault()?["OA3xOriginalProductKey"] as string ?? string.Empty;
            searcher = new ManagementObjectSearcher("SELECT OA3xOriginalProductKeyDescription FROM SoftwareLicensingService");

            oemDescription = searcher.Get().Cast<ManagementObject>()
                                 .FirstOrDefault()?["OA3xOriginalProductKeyDescription"] as string ?? string.Empty;
        }
    }

    // Based on https://github.com/guilhermelim/Get-Windows-Product-Key
    public static class KeyFinder
    {
        private static byte[] productKeyID = (byte[])WindowsHandler.WindowsRK.GetValue("DigitalProductId");
        private const string Digits = "BCDFGHJKMPQRTVWXY2346789";
        private const int keyOffset = 52;
        private const int decodeLength = 29;

        // For NT 6.2 + (Windows 8 and above)
        public static string GetWindowsKey62()
        {
            string key = String.Empty;
            Span<byte> rawKey = new Span<byte>(productKeyID, keyOffset, 15);
            rawKey[14] &= 0xf7;

            int last = 0;
            for (int i = 24; i >= 0; i--)
            {
                int current = 0;
                for (int j = 14; j >= 0; j--)
                {
                    current = rawKey[j] | (current << 8);
                    rawKey[j] = (byte)(current / 24);
                    current %= 24;
                    last = current;
                }
                key = Digits[current] + key;
            }

            string keypart1 = key.Substring(1, last);
            string keypart2 = key.Substring(last + 1, key.Length - (last + 1));
            key = keypart1 + "N" + keypart2;

            for (int i = 5; i < key.Length; i += 6)
            {
                key = key.Insert(i, "-");
            }

            return key;
        }

        // For NT 6.1 (Windows 7 and lower)
        public static string GetWindowsKey61()
        {
            char[] decodedChars = new char[decodeLength];
            Span<byte> rawKey = new Span<byte>(productKeyID, keyOffset, 15);

            for (int i = decodeLength - 1; i >= 0; i--)
            {
                if ((i + 1) % 6 != 0)
                {
                    int current = 0;
                    for (int j = 14; j >= 0; j--)
                    {
                        current = (current << 8) | (byte)rawKey[j];
                        rawKey[j] = (byte)(current / 24);
                        current %= 24;
                        decodedChars[i] = Digits[current];
                    }
                }
                else
                {
                    decodedChars[i] = '-';
                }
            }
            return new string(decodedChars);
        }
    }
}
