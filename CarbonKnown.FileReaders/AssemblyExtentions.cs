using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CarbonKnown.FileReaders
{
    public static class AssemblyExtentions
    {
        public static IEnumerable<Type> AllAssemblyTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetLoadableTypes);
        }

        public static IEnumerable<Type> AllAssemblyTypes(Type type)
        {
            return AllAssemblyTypes().Where(type.IsAssignableFrom);
        }

        public static IEnumerable<Type> AllAssemblyTypes<T>()
        {
            return AllAssemblyTypes(typeof (T));
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly, Type type)
        {
            return GetLoadableTypes(assembly).Where(type.IsAssignableFrom);
        }

        public static IEnumerable<Type> GetLoadableTypes<T>(this Assembly assembly)
        {
            return GetLoadableTypes(assembly, typeof(T));
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
