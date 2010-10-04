using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bounce.Framework {
    public class Iis6WebSite : Iis6Task {
        [Dependency]
        public Val<string> Directory;
        [Dependency]
        public Val<int> Port;
        [Dependency]
        public Val<string> Name;
        [Dependency]
        public Val<IEnumerable<Iis6ScriptMap>> ScriptMapsToAdd;
        [Dependency]
        public Val<IEnumerable<Iis6Authentication>> Authentication;
        [Dependency]
        public Iis6AppPool AppPool;

        private static Iis6ScriptMap[] _mvcScriptMaps;

        public Iis6WebSite() {
            Port = 80;
            ScriptMapsToAdd = new Iis6ScriptMap[0];
            Authentication = new[] {Iis6Authentication.Basic, Iis6Authentication.NTLM};
            AppPool = null;
        }

        public static Iis6ScriptMap[] MvcScriptMaps {
            get {
                if (_mvcScriptMaps == null) {
                    _mvcScriptMaps = new[] {
                        new Iis6ScriptMap {
                            Executable = @"c:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\aspnet_isapi.dll",
                            Extension = ".mvc",
                        },
                        new Iis6ScriptMap {
                            Executable = @"c:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\aspnet_isapi.dll",
                            Extension = "*",
                            ScriptEngine = false,
                            IncludedVerbs = "All",
                            VerifyThatFileExists = false,
                        },
                    };
                }
                return _mvcScriptMaps;
            }
        }

        public override void Build() {
            DeleteIfExtant();
            IisWebSite webSite = Iis.CreateWebSite(Name.Value, new[] {new IisWebSiteBinding {Port = Port.Value}}, Path.GetFullPath(Directory.Value));

            webSite.AddScriptMapsToSite(ScriptMapsToAdd.Value);
            webSite.Authentication = Authentication.Value;
            if (AppPool != null) {
                webSite.AppPoolName = AppPool.Name.Value;
            }
        }

        private void DeleteIfExtant() {
            IisWebSite website = Iis.TryGetWebSiteByServerComment(Name.Value);

            if (website != null) {
                website.Delete();
            }
        }

        public override void Clean() {
            DeleteIfExtant();
        }
    }
}