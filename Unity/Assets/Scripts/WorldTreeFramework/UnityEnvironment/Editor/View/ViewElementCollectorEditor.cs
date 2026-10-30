using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WorldTree;

namespace EditorTool
{
	/// <summary>
	/// 视图元素收集器编辑器：反射 View 字段并重建 prefab 元素映射。
	/// </summary>
	[CustomEditor(typeof(ViewElementCollector))]
	public class ViewElementCollectorEditor : Editor
	{
		private const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.Instance;

		/// <summary>
		/// 是否允许手动编辑 Target
		/// </summary>
		private bool editTargets;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script", "viewTypeName", "entries");

			bool viewTypeChanged = DrawViewTypePopup();
			serializedObject.ApplyModifiedProperties();

			ViewElementCollector collector = (ViewElementCollector)target;

			if (viewTypeChanged)
			{
				Rebuild(collector);
				serializedObject.Update();
			}

			EditorGUILayout.Space();
			if (GUILayout.Button("重建收集列表"))
			{
				Rebuild(collector);
				serializedObject.Update();
			}

			DrawEntries();
			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space();
			EditorGUILayout.HelpBox(
				"自动填充只按字段名查找同名节点；找不到时打开手动编辑后拖入 Target。",
				MessageType.Info);
		}

		/// <summary>
		/// 绘制只读收集列表
		/// </summary>
		private void DrawEntries()
		{
			SerializedProperty entries = serializedObject.FindProperty("entries");

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("收集列表", EditorStyles.boldLabel);
			editTargets = EditorGUILayout.ToggleLeft("允许手动编辑 Target", editTargets);

			if (entries.arraySize == 0)
			{
				EditorGUILayout.LabelField("空");
				return;
			}

			for (int i = 0; i < entries.arraySize; i++)
			{
				SerializedProperty item = entries.GetArrayElementAtIndex(i);
				SerializedProperty key = item.FindPropertyRelative("Key");
				SerializedProperty target = item.FindPropertyRelative("Target");

				Color oldColor = GUI.color;
				Color oldBackground = GUI.backgroundColor;
				if (target.objectReferenceValue == null)
				{
					GUI.color = new Color(1f, 0.35f, 0.35f);
					GUI.backgroundColor = new Color(1f, 0.55f, 0.55f);
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.SelectableLabel(
					key.stringValue,
					GUILayout.Width(180),
					GUILayout.Height(EditorGUIUtility.singleLineHeight));

				using (new EditorGUI.DisabledScope(!editTargets))
				{
					UnityEngine.Object targetValue = EditorGUILayout.ObjectField(
						target.objectReferenceValue,
						typeof(UnityEngine.Object),
						false);
					if (editTargets) target.objectReferenceValue = targetValue;
				}

				EditorGUILayout.EndHorizontal();
				GUI.color = oldColor;
				GUI.backgroundColor = oldBackground;
			}
		}

		/// <summary>
		/// 绘制 View 类型选择
		/// </summary>
		private bool DrawViewTypePopup()
		{
			List<Type> viewTypes = TypeCache.GetTypesDerivedFrom<ViewObject>()
				.Where(type => !type.IsAbstract)
				.OrderBy(type => type.FullName)
				.ToList();

			List<string> options = new List<string> { "自动（使用 prefab 名）" };
			options.AddRange(viewTypes.Select(type => type.FullName));

			SerializedProperty viewTypeProperty = serializedObject.FindProperty("viewTypeName");
			int current = 0;
			if (!string.IsNullOrEmpty(viewTypeProperty.stringValue))
			{
				int index = viewTypes.FindIndex(type =>
					type.FullName == viewTypeProperty.stringValue ||
					type.Name == viewTypeProperty.stringValue);
				if (index >= 0) current = index + 1;
			}

			int selected = EditorGUILayout.Popup("View 类型", current, options.ToArray());
			bool changed = selected != current;
			if (selected == 0) viewTypeProperty.stringValue = string.Empty;
			else viewTypeProperty.stringValue = viewTypes[selected - 1].FullName;

			// 选择“自动”时只改字段，不立即反射；具体类型选择后自动重建。
			return changed && selected > 0;
		}

		private static void Rebuild(ViewElementCollector collector)
		{
			Type viewType = ResolveViewType(collector);
			if (viewType == null)
			{
				Debug.LogError($"找不到 View 类型：{collector.ViewTypeName ?? collector.name}");
				return;
			}

			List<ViewElementCollector.Entry> entries = new List<ViewElementCollector.Entry>();
			FieldInfo[] fields = viewType.GetFields(FieldFlags);

			foreach (FieldInfo field in fields)
			{
				if (!TryGetWidgetType(field.FieldType, out Type widgetType)) continue;

				string key = field.Name;
				Type elementType = GetDefaultElementType(widgetType);
				if (elementType == null)
				{
					Debug.LogWarning($"字段 {viewType.Name}.{field.Name} 没有默认平台组件类型，需要手动挂载。");
					entries.Add(new ViewElementCollector.Entry { Key = key });
					continue;
				}

				Transform targetTransform = collector.transform.Find(key);
				if (targetTransform == null)
				{
					Debug.LogWarning($"字段 {viewType.Name}.{field.Name} 找不到同名节点，需要手动挂载。");
					entries.Add(new ViewElementCollector.Entry { Key = key });
					continue;
				}

				Component component = targetTransform.GetComponent(elementType);
				if (component == null)
				{
					Debug.LogWarning($"节点 {key} 找不到组件：{elementType.Name}，需要手动挂载。");
					entries.Add(new ViewElementCollector.Entry { Key = key });
					continue;
				}

				entries.Add(new ViewElementCollector.Entry
				{
					Key = key,
					Target = component,
				});
			}

			if (entries.Count == 0)
			{
				Debug.LogWarning($"View {viewType.Name} 没有收集到任何字段。");
			}

			Undo.RecordObject(collector, "Rebuild View Element Collector");

			SerializedObject serializedObject = new SerializedObject(collector);
			SerializedProperty viewTypeProperty = serializedObject.FindProperty("viewTypeName");
			if (string.IsNullOrEmpty(viewTypeProperty.stringValue))
			{
				viewTypeProperty.stringValue = viewType.FullName;
			}

			SerializedProperty entriesProperty = serializedObject.FindProperty("entries");
			entriesProperty.arraySize = entries.Count;
			for (int i = 0; i < entries.Count; i++)
			{
				SerializedProperty item = entriesProperty.GetArrayElementAtIndex(i);
				item.FindPropertyRelative("Key").stringValue = entries[i].Key;
				item.FindPropertyRelative("Target").objectReferenceValue = entries[i].Target;
			}

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(collector);
		}

		private static Type ResolveViewType(ViewElementCollector collector)
		{
			string typeName = collector.ViewTypeName;
			if (string.IsNullOrEmpty(typeName)) typeName = collector.name;

			foreach (Type type in TypeCache.GetTypesDerivedFrom<ViewObject>())
			{
				if (type.Name == typeName || type.FullName == typeName) return type;
			}

			// 允许只填短名时，与 prefab 名匹配。
			foreach (Type type in TypeCache.GetTypesDerivedFrom<ViewObject>())
			{
				if (type.Name == collector.name) return type;
			}

			return null;
		}

		private static bool TryGetWidgetType(Type fieldType, out Type widgetType)
		{
			widgetType = null;
			if (!fieldType.IsGenericType) return false;
			if (fieldType.GetGenericTypeDefinition() != typeof(NodeRef<>)) return false;

			Type valueType = fieldType.GetGenericArguments()[0];
			if (!typeof(ViewWidget).IsAssignableFrom(valueType)) return false;
			widgetType = valueType;
			return true;
		}

		private static Type GetDefaultElementType(Type widgetType)
		{
			if (widgetType == typeof(ViewText)) return typeof(Text);
			if (widgetType == typeof(ViewButton)) return typeof(Button);
			if (widgetType == typeof(ViewImage)) return typeof(Image);
			return null;
		}
	}
}
