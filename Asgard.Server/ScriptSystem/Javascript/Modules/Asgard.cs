using Asgard.Core.System;
using NiL.JS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.ScriptSystem.Javascript.Modules
{
    internal class AsgardModule : JSValue
    {
        public delegate void OnTickCallback(double delta);
        public event OnTickCallback OnTick;

        private AsgardBase _instance;

        public AsgardModule(AsgardBase server)
        {
            this.ValueType = JSValueType.Object;
            _instance = server;
            Value = Wrap(_instance);
        }

        internal void Tick(double delta)
        {
            if (OnTick != null)
                OnTick(delta);
        }

        private Type getTypeByName(string className)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = a.GetType(className, false, true);
                if (t != null) return t;
            }

            return null;

        }

        public ISystem getSystem(string typeName)
        {
            var type = getTypeByName(typeName);
            if (type == null) return null;
            return _instance.LookupSystem(type);
        }

        protected override void SetProperty(JSValue name, JSValue value, PropertyScope propertyScope, bool throwOnError)
        {
            if (name.ToString() == "OnTick")
            {
                OnTick += (d) =>
                {
                    var arg = new Arguments();
                    arg.Add(Marshal(d));
                    var func = (value.Value as NiL.JS.BaseLibrary.Function);
                    func.Call(arg);
                };
                return;
            }

            base.SetProperty(name, value, propertyScope, throwOnError);
        }

        protected override JSValue GetProperty(JSValue key, bool forWrite, PropertyScope propertyScope)
        {
            if (key.ToString() == "getSystem")
            {
                var f = new Func<string, ISystem>(s => getSystem(s));
                return Marshal(f);
            }

            var methodInfo = _instance.GetType().GetMethod(key.ToString(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo != null)
            {
                var del = methodInfo.CreateDelegate(Expression.GetDelegateType(

                        (from parameter in methodInfo.GetParameters() select parameter.ParameterType)
                        .Concat(new[] { methodInfo.ReturnType })
                        .ToArray()), _instance);

                return Marshal(del);
            }

            methodInfo = _instance.GetType().GetMethod(key.ToString(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (methodInfo != null)
            {
                var del = methodInfo.CreateDelegate(Expression.GetDelegateType(

                        (from parameter in methodInfo.GetParameters() select parameter.ParameterType)
                        .Concat(new[] { methodInfo.ReturnType })
                        .ToArray()));

                return Marshal(del);
            }

            return base.GetProperty(key, forWrite, propertyScope);
        }
    }
}
