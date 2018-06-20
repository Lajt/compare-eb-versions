using System.Runtime.Serialization;
using Loki;
using Loki.Common;

namespace Default.AutoLogin
{
    public class Settings : JsonSettings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());

        private Settings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, "AutoLoginEx.json"))
        {
        }

        private string _character;

        public string Character
        {
            get => _character;
            set
            {
                if (value == _character) return;
                _character = value;
                NotifyPropertyChanged(() => Character);
            }
        }

        public float LoginDelayInitial { get; set; } = 0.5f;
        public float LoginDelayStep { get; set; } = 3;
        public float LoginDelayFinal { get; set; } = 300;
        public int LoginDelayRandPct { get; set; } = 15;
        public float CharSelectDelay { get; set; } = 0.5f;

        public bool LoginUsingUserCredentials { get; set; }
        public bool LoginUsingGateway { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Gateway { get; set; }


        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (string.IsNullOrEmpty(Password))
                return;

            // Encrypt the key when serializing to file.
            Password = GlobalSettings.Crypto.EncryptStringAes(Password, "autologinsharedsecret");
        }

        [OnSerialized]
        internal void OnSerialized(StreamingContext context)
        {
            if (string.IsNullOrEmpty(Password))
                return;

            // Decrypt the key when we're done serializing, so we can have the plain-text version back.
            Password = GlobalSettings.Crypto.DecryptStringAes(Password, "autologinsharedsecret");
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            // Make sure we decrypt the license key, so we can use it.
            OnSerialized(context);
        }

        public static readonly string[] GatewayList =
        {
            "Auto-select Gateway",
            "Texas (US)",
            "Washington, D.C. (US)",
            "California (US)",
            "Amsterdam (EU)",
            "London (EU)",
            "Frankfurt (EU)",
            "Milan (EU)",
            "Singapore",
            "Australia",
            "Sao Paulo (BR)",
            "Paris (EU)",
            "Moscow (RU)",
            "Japan"
        };
    }
}