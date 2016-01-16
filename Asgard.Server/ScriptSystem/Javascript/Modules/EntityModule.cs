using Artemis;
using Artemis.Manager;
using Asgard.Core.System;
using NiL.JS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.ScriptSystem.Javascript.Modules
{
    internal class EntityModule
    {
        private AsgardBase _instance;

        public EntityModule(AsgardBase server)
        {
            _instance = server;
        }

        public Entity FindEntity(uint id)
        {
            var em = _instance.EntityManager;
            var ent = em.GetEntityByUniqueId(id);
            return ent;
        }

        public List<JSValue> GetEntities(Aspect aspect)
        {
            var em = _instance.EntityManager;
            var lst = em.GetEntities(aspect);
            List<JSValue> ents = new List<JSValue>();
            foreach(var e in lst)
            {
                ents.Add(JSValue.Wrap(e));
            }
            return ents;
        }

        private Type getTypeByName(string className)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = a.GetType(className,false,true);
                if (t != null) return t;
            }

            return null;

        }

        private Type[] ConvertTypeList(string type1, string type2, string type3, string type4)
        {
            string[] stringTypes = new string[4];
            stringTypes[0] = type1;
            stringTypes[1] = type2;
            stringTypes[2] = type3;
            stringTypes[3] = type4;
            List<Type> types = new List<Type>();

            foreach (var type in stringTypes)
            {
                if (type == null) break;
                {
                    var t = getTypeByName(type);
                    if (t != null)
                    {
                        types.Add(t);
                    }
                }
            }

            return types.ToArray();
        }

        public ComponentType getComponentType(string type)
        {
            var t = getTypeByName(type);
            var compType = ComponentTypeManager.GetTypeFor(t);
            return compType;
        }

        public Aspect AspectOne(string type1, string type2, string type3, string type4)
        {
            var types = ConvertTypeList(type1, type2, type3, type4);
            return Aspect.One(types);
        }

        public Aspect AspectAll(string type1, string type2, string type3, string type4)
        {

            var types = ConvertTypeList(type1, type2, type3, type4);
            return Aspect.All(types);
        }

        public Aspect AspectExclude(string type1, string type2, string type3, string type4)
        {
            var types = ConvertTypeList(type1, type2, type3, type4);
            return Aspect.Exclude(types);
        }

    }

}
