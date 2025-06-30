using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Unity.APIComparison.Framework.Collectors;
using UnityEngine.Pool;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BehaviourSystem.BT
{
    public static class ReflectionHelper
    {
        public delegate object Getter(NodeBase instance);

        public delegate void Setter(NodeBase instance, object value);


        public struct FieldAccessor
        {
            public FieldAccessor(Getter getter, Setter setter, Type fieldType)
            {
                this.getter = getter;
                this.setter = setter;
                this.fieldType = fieldType;
            }

            public Getter getter { get; set; }
            public Setter setter { get; set; }
            
            public Type fieldType { get; set; }
        }

        private readonly static Dictionary<Type, FieldInfo[]> _FieldCacher = new Dictionary<Type, FieldInfo[]>();

        private readonly static Lazy<Dictionary<FieldInfo, FieldAccessor>> _DelegateCacher = new Lazy<Dictionary<FieldInfo, FieldAccessor>>(() => new());

        private readonly static Type[] _GetterParamTypes = new[] { typeof(NodeBase) };

        private readonly static Type[] _SetterParamTypes = new[] { typeof(NodeBase), typeof(object) };


#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ClearCache()
        {
            _FieldCacher.Clear();
            _DelegateCacher.Value.Clear();
        }
#endif

        public static FieldInfo[] GetCachedFieldInfo(Type type, in FieldReflectionDesc desc)
        {
            if (_FieldCacher.TryGetValue(type, out FieldInfo[] fieldInfos))
            {
                return fieldInfos;
            }

            List<FieldInfo> validFields = ListPool<FieldInfo>.Get();
            FieldInfo[] allFields = type.GetFields(desc.flagSettings);
            
            foreach (FieldInfo field in allFields)
            {
                if (ReflectionHelper.IsFieldTypeIncluded(field, desc.includeTypes))
                {
                    validFields.Add(field);
                }
            }
            
            fieldInfos = validFields.ToArray();
            ListPool<FieldInfo>.Release(validFields);
            _FieldCacher[type] = fieldInfos;
            return fieldInfos;
        }

        
        
        private static bool IsFieldTypeIncluded(FieldInfo field, Type[] includeTypes)
        {
            foreach (Type includeType in includeTypes)
            {
                if (includeType.IsAssignableFrom(field.FieldType))
                {
                    return true;
                }
            }
            
            return false;
        }


        public static FieldAccessor GetAccessor(FieldInfo fieldInfo)
        {
            if (_DelegateCacher.Value.TryGetValue(fieldInfo, out FieldAccessor accessor))
            {
                return accessor;
            }

            accessor = new FieldAccessor(CreateGetter(fieldInfo), CreateSetter(fieldInfo), fieldInfo.FieldType);
            _DelegateCacher.Value[fieldInfo] = accessor;
            return accessor;
        }


        private static Getter CreateGetter(FieldInfo fieldInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"Get_{fieldInfo.Name}", typeof(object), _GetterParamTypes, fieldInfo.DeclaringType.Module);

            ILGenerator il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
            il.Emit(OpCodes.Ldfld, fieldInfo);

            if (fieldInfo.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Box, fieldInfo.FieldType);
            }

            il.Emit(OpCodes.Ret);

            return (Getter)dynamicMethod.CreateDelegate(typeof(Getter));
        }


        private static Setter CreateSetter(FieldInfo fieldInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"Set_{fieldInfo.Name}", typeof(void), _SetterParamTypes, fieldInfo.DeclaringType.Module);

            ILGenerator il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
            il.Emit(OpCodes.Ldarg_1);


            if (fieldInfo.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, fieldInfo.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, fieldInfo.FieldType);
            }

            il.Emit(OpCodes.Stfld, fieldInfo);
            il.Emit(OpCodes.Ret);

            return (Setter)dynamicMethod.CreateDelegate(typeof(Setter));
        }
    }
}