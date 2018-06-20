using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Loki;
using Loki.Common;
using Loki.Game;
using Newtonsoft.Json;

namespace Legacy.AutoLogin
{
	/// <summary>Settings for the Dev tab. </summary>
	public class AutoLoginSettings : JsonSettings
	{
		private static AutoLoginSettings _instance;

		/// <summary>The current instance for this class. </summary>
		public static AutoLoginSettings Instance => _instance ?? (_instance = new AutoLoginSettings());

		/// <summary>The default ctor. Will use the settings path "AutoLogin".</summary>
		public AutoLoginSettings()
			: base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "AutoLogin")))
		{
			NextLoginTime = DateTime.Now;
			if (LoginAttemptDelay == default(TimeSpan))
			{
				LoginAttemptDelay = TimeSpan.FromMilliseconds(LokiPoe.Random.Next(3000, 6000));
			}
			if (SelectCharacterDelay == default(TimeSpan))
			{
				SelectCharacterDelay = TimeSpan.FromMilliseconds(LokiPoe.Random.Next(3000, 6000));
			}
		}

		private bool _autoLogin;
		private bool _loginUsingUserCredentials;
		private bool _loginUsingGateway;
		private string _email;
		private string _password;
		private string _gateway;
		private TimeSpan _loginAttemptDelay;
		private bool _delayBeforeLoginAttempt;
		private bool _autoSelectCharacter;
		private string _character;
		private bool _delayBeforeSelectingCharacter;
		private TimeSpan _selectCharacterDelay;
		private int _maxLoginAttempts;

		private DateTime _nextLoginTime;
		private int _loginAttempts;
		private int _selectCharacterAttempts;

		/// <summary>Should the bot login using the Gateway. </summary>
		[DefaultValue(false)]
		public bool LoginUsingGateway
		{
			get
			{
				return _loginUsingGateway;
			}
			set
			{
				if (value.Equals(_loginUsingGateway))
				{
					return;
				}
				_loginUsingGateway = value;
				NotifyPropertyChanged(() => LoginUsingGateway);
			}
		}

		/// <summary>The e-mail to login with.</summary>
		[DefaultValue("Auto-select Gateway")]
		public string Gateway
		{
			get
			{
				return _gateway;
			}
			set
			{
				if (value.Equals(_gateway))
				{
					return;
				}
				_gateway = value;
				NotifyPropertyChanged(() => Gateway);
			}
		}

		/// <summary>Should the bot try logging in. If this is set to false, the bot will never login on its own. </summary>
		[DefaultValue(false)]
		public bool AutoLogin
		{
			get
			{
				return _autoLogin;
			}
			set
			{
				if (value.Equals(_autoLogin))
				{
					return;
				}
				_autoLogin = value;
				NotifyPropertyChanged(() => AutoLogin);
			}
		}

		/// <summary>Should the Email and Password properties be used to login? If this value is false, the bot will use the default client credentials.</summary>
		[DefaultValue(false)]
		public bool LoginUsingUserCredentials
		{
			get
			{
				return _loginUsingUserCredentials;
			}
			set
			{
				if (value.Equals(_loginUsingUserCredentials))
				{
					return;
				}
				_loginUsingUserCredentials = value;
				NotifyPropertyChanged(() => LoginUsingUserCredentials);
			}
		}

		/// <summary>The e-mail to login with.</summary>
		[DefaultValue("")]
		public string Email
		{
			get
			{
				return _email;
			}
			set
			{
				if (value.Equals(_email))
				{
					return;
				}
				_email = value;
				NotifyPropertyChanged(() => Email);
			}
		}

		/// <summary>The password to login with.</summary>
		[DefaultValue("")]
		public string Password
		{
			get
			{
				return _password;
			}
			set
			{
				if (value.Equals(_password))
				{
					return;
				}
				_password = value;
				NotifyPropertyChanged(() => Password);
			}
		}

		/// <summary>Should the bot wait a certain amount of time before logging in. If this is set to false, the bot will login as soon as it can. </summary>
		[DefaultValue(true)]
		public bool DelayBeforeLoginAttempt
		{
			get
			{
				return _delayBeforeLoginAttempt;
			}
			set
			{
				if (value.Equals(_delayBeforeLoginAttempt))
				{
					return;
				}
				_delayBeforeLoginAttempt = value;
				NotifyPropertyChanged(() => DelayBeforeLoginAttempt);
			}
		}

		/// <summary>How much time in MS to wait before trying to login. </summary>
		public TimeSpan LoginAttemptDelay
		{
			get
			{
				return _loginAttemptDelay;
			}
			set
			{
				if (value.Equals(_loginAttemptDelay))
				{
					return;
				}
				_loginAttemptDelay = value;
				NotifyPropertyChanged(() => LoginAttemptDelay);
			}
		}

		/// <summary>Should the bot try selecting a character.  If this is set to false, the bot will never select a character on its own.</summary>
		[DefaultValue(false)]
		public bool AutoSelectCharacter
		{
			get
			{
				return _autoSelectCharacter;
			}
			set
			{
				if (value.Equals(_autoSelectCharacter))
				{
					return;
				}
				_autoSelectCharacter = value;
				NotifyPropertyChanged(() => AutoSelectCharacter);
			}
		}

		/// <summary>The character to login with.</summary>
		[DefaultValue("")]
		public string Character
		{
			get
			{
				return _character;
			}
			set
			{
				if (value.Equals(_character))
				{
					return;
				}
				_character = value;
				NotifyPropertyChanged(() => Character);
			}
		}

		/// <summary>Should the bot wait before selecting a character?</summary>
		[DefaultValue(true)]
		public bool DelayBeforeSelectingCharacter
		{
			get
			{
				return _delayBeforeSelectingCharacter;
			}
			set
			{
				if (value.Equals(_delayBeforeSelectingCharacter))
				{
					return;
				}
				_delayBeforeSelectingCharacter = value;
				NotifyPropertyChanged(() => DelayBeforeSelectingCharacter);
			}
		}

		/// <summary>How much time in MS to wait before selecting a character. </summary>
		public TimeSpan SelectCharacterDelay
		{
			get
			{
				return _selectCharacterDelay;
			}
			set
			{
				if (value.Equals(_selectCharacterDelay))
				{
					return;
				}
				_selectCharacterDelay = value;
				NotifyPropertyChanged(() => SelectCharacterDelay);
			}
		}

		/// <summary>The max number of login attempts before stopping the bot. If set to -1, it means no maximum.</summary>
		[DefaultValue(10)]
		public int MaxLoginAttempts
		{
			get
			{
				return _maxLoginAttempts;
			}
			set
			{
				if (value.Equals(_maxLoginAttempts))
				{
					return;
				}
				_maxLoginAttempts = value;
				NotifyPropertyChanged(() => MaxLoginAttempts);
			}
		}

		/// <summary>The value in which the login process should wait until trying to login again.</summary>
		[JsonIgnore]
		public DateTime NextLoginTime
		{
			get
			{
				return _nextLoginTime;
			}
			set
			{
				if (value.Equals(_nextLoginTime))
				{
					return;
				}
				_nextLoginTime = value;
				NotifyPropertyChanged(() => NextLoginTime);
			}
		}

		/// <summary>The current number of login attempts. </summary>
		[JsonIgnore]
		public int LoginAttempts
		{
			get
			{
				return _loginAttempts;
			}
			set
			{
				if (value.Equals(_loginAttempts))
				{
					return;
				}
				_loginAttempts = value;
				NotifyPropertyChanged(() => LoginAttempts);
			}
		}

		/// <summary>The number of select character attempts. </summary>
		[JsonIgnore]
		public int SelectCharacterAttempts
		{
			get
			{
				return _selectCharacterAttempts;
			}
			set
			{
				if (value.Equals(_selectCharacterAttempts))
				{
					return;
				}
				_selectCharacterAttempts = value;
				NotifyPropertyChanged(() => SelectCharacterAttempts);
			}
		}

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
	}
}
