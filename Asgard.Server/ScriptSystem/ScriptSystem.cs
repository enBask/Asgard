using Asgard.Core.Network;
using Asgard.ScriptSystem.Javascript.Modules;
using NiL.JS;
using NiL.JS.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.ScriptSystem
{
    public abstract class BaseScriptSystem : BaseSystem
    {
        public BaseScriptSystem()
        {
        }

        public abstract bool Execute(string file);

        public override abstract void Tick(double delta);
    }

    public class JavascriptSystem : BaseScriptSystem
    {
        internal class TimerData
        {
            public double interval;
            public bool repeat;
            public double timeout;
            public JSValue callback;
        }

        Context _context;
        AsgardModule _asgardModule;
        EntityModule _entityModule;
        List<string> _preloadScripts = new List<string>();
        bool _running = false;

        public JavascriptSystem()
        {
            _context = new Context();

        }

        public override bool Execute(string file)
        {
            file = PathHelpers.Resolve(file);
            if (!File.Exists(file)) return false;

            if (!_running)
                _preloadScripts.Add(file);
            else
            {
                var contents = File.ReadAllText(file);
                _context.Eval(contents, true);
            }

            return true;
        }

        public override bool Start()
        {
            _running = true;
            BundleRequire();
            BundleConsole();
            BundleCore();


            foreach (var script in _preloadScripts)
            {
                var contents = File.ReadAllText(script);
                _context.Eval(contents, true);
            }

            return base.Start();
        }

        public override bool Stop()
        {
            _running = false;
            return base.Stop();
        }

        public override void Tick(double delta)
        {
            #region timer ticks
            List<TimerData> _remList = new List<TimerData>();
            bool resort = false;
            for (int i = 0; i < _callbackTimers.Count; ++i)
            {
                var timer = _callbackTimers[i];
                if (timer.timeout > NetTime.RealTime)
                    break;

                Execute(timer.callback);

                if (!timer.repeat)
                {
                    _remList.Add(timer);
                }
                else
                {
                    timer.timeout = NetTime.RealTime + timer.interval;
                    resort = true;
                }
            }

            foreach (var timer in _remList)
                _callbackTimers.Remove(timer);

            if (resort)
            {
                _callbackTimers.Sort((a, b) =>
                {
                    return (a.timeout.CompareTo(b.timeout));
                });
            }
            #endregion

            _asgardModule.Tick(delta);
        }

        private JSValue Execute(JSValue f)
        {
            return (f.Value as NiL.JS.BaseLibrary.Function).Call(null);
        }

        Dictionary<string, Javascript.Module> _cachedRequireModules = new Dictionary<string, Javascript.Module>();
        List<TimerData> _callbackTimers = new List<TimerData>();
        private void BundleRequire()
        {
            Func<string, JSValue> requireFunc = new Func<string, JSValue>((file) =>
            {
                if (!_coreModuleList.Contains(file))
                {
                    file = PathHelpers.Resolve(file);
                    if (!File.Exists(file)) return null;
                }


                Javascript.Module module = null;
                if (_cachedRequireModules.TryGetValue(file, out module))
                {
                    return module.exports;
                }


                var contents = File.ReadAllText(file);
                StringBuilder sb = new StringBuilder();
                sb.Append("var f = function(){var module = new Module(); var exports = module.exports;");
                sb.Append(contents);
                sb.Append("return module;}; f();");

                JSValue jsVal = _context.Eval(sb.ToString());
                module = jsVal.Value as Javascript.Module;

                _cachedRequireModules[file] = module;
                return module.exports;

            });

            _context.DefineConstructor(typeof(Javascript.Module));
            _context.DefineVariable("require").Assign(JSValue.Marshal(requireFunc));
        }

        private void BundleConsole()
        {
            Javascript.Console console = new Javascript.Console();
            _context.DefineVariable("console").Assign(JSValue.Wrap(console));
        }

        List<string> _coreModuleList = new List<string>(new string[]
            {
                "asgard",
                "entityManager"
            });

        private void BundleCore()
        {
            _asgardModule = new AsgardModule(Base);
            _entityModule = new EntityModule(Base);
            Javascript.Module module = new Javascript.Module();
            module.exports = JSValue.Wrap(_asgardModule);
            _cachedRequireModules["asgard"] = module;

            module = new Javascript.Module();
            module.exports = JSValue.Wrap(_entityModule);
            _cachedRequireModules["entityManager"] = module;

            Func<JSValue, int, TimerData> setIntervalFunc = new Func<JSValue, int, TimerData>((cb, ms) =>
            {
                var expireTime = NetTime.RealTime + ((double)ms / 1000.0);
                var tup = new TimerData()
                {
                    interval = ((double)ms / 1000.0),
                    repeat = true,
                    timeout = expireTime,
                    callback =cb
                };

                _callbackTimers.Add(tup);
                _callbackTimers.Sort((a, b) =>
                {
                    return (a.timeout.CompareTo(b.timeout));
                });

                return tup;
            });
            _context.DefineVariable("setInterval").Assign(JSValue.Marshal(setIntervalFunc));

            Func<JSValue, int, TimerData> setTimeoutFunc = new Func<JSValue, int, TimerData>((cb, ms) =>
            {
                var expireTime = NetTime.RealTime + ((double)ms / 1000.0);
                var tup = new TimerData()
                {
                    interval = ((double)ms / 1000.0),
                    repeat = false,
                    timeout = expireTime,
                    callback = cb
                };

                _callbackTimers.Add(tup);
                _callbackTimers.Sort((a, b) =>
                {
                    return (a.timeout.CompareTo(b.timeout));
                });

                return tup;
            });
            _context.DefineVariable("setTimeout").Assign(JSValue.Marshal(setTimeoutFunc));

            Action<TimerData> clearTimeout = new Action<TimerData>((td) =>
            {
                if (_callbackTimers.Contains(td))
                    _callbackTimers.Remove(td);
            });
            _context.DefineVariable("clearTimeout").Assign(JSValue.Marshal(clearTimeout));
            _context.DefineVariable("clearInterval").Assign(JSValue.Marshal(clearTimeout));

        }
    }
}
