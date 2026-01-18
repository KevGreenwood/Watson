using Microsoft.Win32;
using System;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            version.Text = WindowsHandler.DisplayVersion;
            buildVersion.Text = WindowsHandler.Version + ' ' + WindowsHandler.Platform;
            productID.Text = WindowsHandler.ProductID;
            actID.Text = WindowsHandler.ActID;
            var filt = WindowsHandler.oemDescription.Split(' ');
            oemEdition.Text = filt[1] + " " + filt[2];



            softKey.Text = WindowsHandler.licenseKey != string.Empty ? WindowsHandler.licenseKey : "Not found";
            var splitted = key.Split('-');
            for (int i = 0; i < splitted.Length - 1; i++)
            {
                splitted[i] = "XXXXX";
            }

            censoredKey = string.Join("-", splitted);

            oemKey.Text = censoredKey;

            backupKey.Text = WindowsHandler.backupKey;
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

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(key);
        }

        private void showOem_Btn_Click(object sender, RoutedEventArgs e)
        {
            isCensored = !isCensored;

            oemKey.Text = isCensored
                ? censoredKey
                : key;

            showOem_Btn.Icon = new SymbolIcon
            {
                Symbol = isCensored ? SymbolRegular.Eye20 : SymbolRegular.EyeOff20
            };
        }

        private void showSoft_Btn_Click(object sender, RoutedEventArgs e)
        {
            isCensored = !isCensored;

            softKey.Text = isCensored
                ? censoredKey
                : key;

            showSoft_Btn.Icon = new SymbolIcon
            {
                Symbol = isCensored ? SymbolRegular.Eye20 : SymbolRegular.EyeOff20
            };
        }

        private void showBackup_Btn_Click(object sender, RoutedEventArgs e)
        {
            isCensored = !isCensored;

            backupKey.Text = isCensored
                ? censoredKey
                : key;

            showBackup_Btn.Icon = new SymbolIcon
            {
                Symbol = isCensored ? SymbolRegular.Eye20 : SymbolRegular.EyeOff20
            };
        }
    }

    public static class WindowsHandler
    {
        public static RegistryKey WindowsRK = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false);
        public static RegistryKey BKkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform", false);
        public static string ProductName = WindowsRK.GetValue("ProductName").ToString();
        public static string DisplayVersion { get; set; }
        public static int Build = Convert.ToInt32(WindowsRK.GetValue("CurrentBuildNumber"));
        public static string UBR { get; set; }
        public static string Version { get; set; }
        public static string Platform = Environment.Is64BitOperatingSystem ? "(64-bit)" : "(32-bit)";
        public static string ProductID = WindowsRK.GetValue("ProductId").ToString();
        public static string oemDescription { get; set; }
        public static string backupKey = BKkey.GetValue("BackupProductKeyDefault").ToString();
        public static string ActID { get; set; }
        public static string ProductKey = Encoding.Unicode.GetString((byte[])WindowsRK.GetValue("DigitalProductId4"), 0x3F8, 0x80);
        public static string licenseKey { get; private set; }
        private static float CurrentVersion = float.Parse(WindowsRK.GetValue("CurrentVersion").ToString()) / 10f;

        public static async Task InitializeAsync()
        {
            switch (CurrentVersion)
            {
                case 6.1f:
                    Version = WindowsRK.GetValue("CSDVersion")?.ToString() ?? string.Empty;
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
                        Version = $"{Build}.{UBR}";

                        if (Build >= 22000)
                        {
                            ProductName = ProductName.Replace("Windows 10", "Windows 11");
                        }
                    }
                    break;
            }

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

            key = key.Substring(1, last) + "N" + key.Substring(last + 1, key.Length - (last + 1));

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
