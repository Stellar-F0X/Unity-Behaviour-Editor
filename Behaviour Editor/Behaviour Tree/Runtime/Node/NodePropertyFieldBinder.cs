using System;
using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    /// <summary>
    /// 노드의 블랙보드 프로퍼티를 처리하는 유틸리티 클래스
    /// </summary>
    public static class NodePropertyFieldBinder
    {
        private readonly static Type _PropertyType = typeof(IBlackboardProperty);
        
        private readonly static Type _ConditionType = typeof(BlackboardBasedCondition);
        
        private readonly static Type _ConditionListType = typeof(ICollection<BlackboardBasedCondition>);


        /// <summary> 노드의 블랙보드 프로퍼티를 할당합니다. </summary>
        /// <param name="node">대상 노드</param>
        /// <param name="blackboard">참조할 블랙보드</param>
        public static void BindNodeProperties(NodeBase node, Blackboard blackboard)
        {
            if (node is null || blackboard is null)
            {
                return;
            }

            var fields = ReflectionHelper.GetCachedFieldInfo(node.GetType(), _PropertyType, _ConditionType, _ConditionListType);

            if (fields.Length == 0)
            {
                return;
            }
            
            foreach (var field in fields)
            {
                var accessor = ReflectionHelper.GetAccessor(field);

                if (_PropertyType.IsAssignableFrom(field.FieldType))
                {
                    ProcessPropertyField(node, blackboard, accessor);
                }
                else if (_ConditionListType.IsAssignableFrom(field.FieldType))
                {
                    ProcessConditionListField(node, blackboard, accessor);
                }
                else if (_ConditionType.IsAssignableFrom(field.FieldType))
                {
                    ProcessConditionField(node, blackboard, accessor);
                }
            }
        }

        
        /// <summary> 블랙보드 프로퍼티 필드를 처리합니다. </summary>
        private static void ProcessPropertyField(NodeBase node, Blackboard blackboard, ReflectionHelper.FieldAccessor accessor)
        {
            if (accessor.getter(node) is IBlackboardProperty property)
            {
                var foundProperty = blackboard.FindProperty(property.key);

                if (foundProperty != null)
                {
                    accessor.setter(node, foundProperty);
                }
            }
        }
        

        /// <summary> 컨디션 리스트 필드를 처리합니다.  </summary>
        private static void ProcessConditionListField(NodeBase node, Blackboard blackboard, ReflectionHelper.FieldAccessor accessor)
        {
            if (accessor.getter(node) is ICollection<BlackboardBasedCondition> conditionList)
            {
                foreach (var condition in conditionList)
                {
                    UpdateConditionProperties(condition, blackboard);
                }
            }
        }

        
        /// <summary> 단일 컨디션 필드를 처리합니다. </summary>
        private static void ProcessConditionField(NodeBase node, Blackboard blackboard, ReflectionHelper.FieldAccessor accessor)
        {
            if (accessor.getter(node) is BlackboardBasedCondition condition)
            {
                UpdateConditionProperties(condition, blackboard);
            }
        }


        /// <summary> 블랙보드 기반 조건의 프로퍼티를 업데이트합니다. </summary>
        private static void UpdateConditionProperties(BlackboardBasedCondition condition, Blackboard blackboard)
        {
            if (condition.property is not null && condition.property.key is not null)
            {
                IBlackboardProperty foundProperty = blackboard.FindProperty(condition.property.key);
                
                if (foundProperty is not null)
                {
                    condition.property = foundProperty;
                }
            }

            if (condition.comparableValue is not null && condition.comparableValue.key is not null)
            {
                IBlackboardProperty foundProperty = blackboard.FindProperty(condition.comparableValue.key);
                
                if (foundProperty is not null)
                {
                    condition.comparableValue = foundProperty;
                }
            }
        }
    }
}