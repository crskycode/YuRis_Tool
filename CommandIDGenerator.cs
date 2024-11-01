using System;
using System.Reflection;
using System.Reflection.Emit;

namespace YuRis_Tool
{
    internal class CommandIDGenerator
    {
        static Type enumType;
        public static void GenerateType(YSCM cmdInfo)
        {
            AssemblyName assemblyName = new AssemblyName("DynamicEnumAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicEnumModule");


            EnumBuilder eb = moduleBuilder.DefineEnum("CommandID", TypeAttributes.Public, typeof(int));
            int i = 0;
            foreach (var c in cmdInfo.CommandsInfo)
            {
                eb.DefineLiteral(c.Name, i++);
            }

            enumType = eb.CreateTypeInfo();
        }

        public static Enum GetID(int id)
        {
            return (Enum)Enum.ToObject(enumType, id);
        }
    }
}
