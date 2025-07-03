using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine.Pool;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UObject = UnityEngine.Object;

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

        public static FieldInfo[] GetCachedFieldInfo(Type type, params Type[] includeTypes)
        {
            if (_FieldCacher.TryGetValue(type, out FieldInfo[] fieldInfos))
            {
                return fieldInfos;
            }

            List<FieldInfo> validFields = ListPool<FieldInfo>.Get();
            FieldInfo[] allFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in allFields)
            {
                if (ReflectionHelper.IsFieldTypeIncluded(field, includeTypes))
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


        /// <summary>
        /// 중첩된 구조에서 특정 타입의 필드에 대한 FieldAccessor들을 찾습니다.
        /// </summary>
        /// <param name="node">탐색을 시작할 루트 노드</param>
        /// <param name="targetFieldType">찾고자 하는 필드의 타입 (예: IBlackboardProperty)</param>
        /// <param name="maxDepth">최대 탐색 깊이</param>
        /// <returns>찾은 FieldAccessor들의 리스트</returns>
        public static List<FieldAccessor> FindNestedAccessors(NodeBase node, Type targetFieldType, int maxDepth = 4)
        {
            Stack<FieldTraversalInfo> stack = new Stack<FieldTraversalInfo>();
            List<FieldAccessor> result = new List<FieldAccessor>();
            HashSet<object> visited = new HashSet<object>();

            const BindingFlags flagSettings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            stack.Push(new FieldTraversalInfo(node, node.GetType(), 0));

            while (stack.TryPop(out FieldTraversalInfo fieldInfo))
            {
                if (fieldInfo.value is null || visited.Contains(fieldInfo.value) || fieldInfo.depth > maxDepth)
                {
                    continue;
                }

                visited.Add(fieldInfo.value);

                foreach (FieldInfo field in fieldInfo.type.GetFields(flagSettings))
                {
                    if (targetFieldType.IsAssignableFrom(field.FieldType))
                    {
                        FieldAccessor accessor = GetAccessor(field);
                        result.Add(accessor);
                        continue;
                    }
                    
                    object fieldValue = field.GetValue(fieldInfo.value); 
                    
                    if (ShouldContinueTraversal(field.FieldType, fieldValue))
                    {
                        stack.Push(new FieldTraversalInfo(fieldValue, field.FieldType, fieldInfo.depth + 1));
                    }
                }
            }

            return result;
        }

        /// <summary> 재귀 탐색을 계속할지 결정합니다. </summary>
        private static bool ShouldContinueTraversal(Type fieldType, object fieldValue)
        {
            if (fieldValue is null)
            {
                return false;
            }
            
            if (typeof(UObject).IsAssignableFrom(fieldType))
            {
                return false;
            }
            
            if (fieldType == typeof(string))
            {
                return false;
            }

            return true;
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