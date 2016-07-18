﻿using Rocket.API;
using Rocket.API.Assets;
using Rocket.API.Collections;
using Rocket.API.Extensions;
using Rocket.API.Permissions;
using Rocket.API.Plugins;
using Rocket.Core.Extensions;
using Rocket.Core.Logging;
using Rocket.Core.Permissions;
using Rocket.Core.RCON;
using Rocket.Core.Serialization;
using Rocket.Core.Tasks;
using Rocket.Plugins.Native;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rocket.Core
{
    public class R : MonoBehaviour
    {
        public delegate void RockedInitialized();

        public static event RockedInitialized OnRockedInitialized;

        public static R Instance;
        public static IRocketImplementation Implementation;

        public static XMLFileAsset<RocketSettings> Settings = null;
        public static XMLFileAsset<TranslationList> Translation = null;
        public static IRocketPermissionsProvider Permissions = null;

        public static List<IRocketPluginManager<IRocketPlugin>> Plugins = new List<IRocketPluginManager<IRocketPlugin>>();

        private static readonly TranslationList defaultTranslations = new TranslationList(){
                {"rocket_join_public","{0} connected to the server" },
                {"rocket_leave_public","{0} disconnected from the server"},
                {"command_no_permission","You do not have permissions to execute this command."},
                {"command_cooldown","You have to wait {0} seconds before you can use this command again."}
        };

        private void Awake()
        {
            Instance = this;

            new Logger();
            Implementation = (IRocketImplementation)GetComponent(typeof(IRocketImplementation));

#if DEBUG
            gameObject.TryAddComponent<Debugger>();
#else
                Initialize();
#endif
        }

        internal void Initialize()
        {
            Environment.Initialize();
            try
            {
                Implementation.OnRocketImplementationInitialized += () =>
                {
                    gameObject.TryAddComponent<TaskDispatcher>();
                    if (Settings.Instance.RCON.Enabled) gameObject.TryAddComponent<RCONServer>();
                };

                Settings = new XMLFileAsset<RocketSettings>(Environment.SettingsFile);
                Translation = new XMLFileAsset<TranslationList>(String.Format(Environment.TranslationFile, Settings.Instance.LanguageCode), new Type[] { typeof(TranslationList), typeof(TranslationListEntry) }, defaultTranslations);
                defaultTranslations.AddUnknownEntries(Translation);
                Permissions = gameObject.TryAddComponent<RocketPermissionsManager>();

                Plugins.Add(gameObject.TryAddComponent<NativeRocketPluginManager<NativeRocketPlugin>>());

                if (Settings.Instance.MaxFrames < 10 && Settings.Instance.MaxFrames != -1) Settings.Instance.MaxFrames = 10;
                Application.targetFrameRate = Settings.Instance.MaxFrames;

                OnRockedInitialized.TryInvoke();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public static string Translate(string translationKey, params object[] placeholder)
        {
            return Translation.Instance.Translate(translationKey, placeholder);
        }

        public static void Reload()
        {
            Settings.Load();
            Translation.Load();
            Permissions.Reload();
            Implementation.Reload();
        }
    }
}