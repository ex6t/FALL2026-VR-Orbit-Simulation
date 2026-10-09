using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEditor.XR.Interaction.Toolkit.Filtering;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using Object = UnityEngine.Object;

namespace UnityEditor.XR.Interaction.Toolkit
{
    class XRInteractionDebuggerWindow : EditorWindow
    {
        [SerializeField]
        Vector2 m_ScrollPosition;
        [SerializeField]
        bool m_ShowInputDevices; // Default off since the focus of this window is XRI.
        [SerializeField]
        bool m_ShowInteractors = true;
        [SerializeField]
        bool m_ShowInteractables = true;
        [SerializeField]
        bool m_ShowTargetFilters;

        [SerializeField]
        TreeViewState<int> m_InputDevicesTreeState;
        [SerializeField]
        MultiColumnHeaderState m_InputDevicesTreeHeaderState;

        [SerializeField]
        TreeViewState<int> m_InteractablesTreeState;
        [SerializeField]
        MultiColumnHeaderState m_InteractablesTreeHeaderState;

        [SerializeField]
        TreeViewState<int> m_InteractorsTreeState;
        [SerializeField]
        MultiColumnHeaderState m_InteractorsTreeHeaderState;

        [SerializeField]
        TreeViewState<int> m_FiltersTreeState;
        [SerializeField]
        MultiColumnHeaderState m_FiltersTreeHeaderState;

        [SerializeField]
        TreeViewState<int> m_EvaluatorsScoreTreeState;
        [SerializeField]
        MultiColumnHeaderState m_EvaluatorsScoreTreeHeaderState;

        XRInputDevicesTreeView m_InputDevicesTree;
        XRInteractorsTreeView m_InteractorsTree;
        XRInteractablesTreeView m_InteractablesTree;
        XRTargetFiltersTreeView m_FiltersTree;
        XRTargetEvaluatorsScoreTreeView m_EvaluatorsScoreTree;

        static XRInteractionDebuggerWindow s_Instance;

        static readonly List<string> s_Names = new List<string>();

        // Row IDs are synthetic integers. Native object IDs retain their full EntityId value.
        static readonly Dictionary<EntityId, int> s_NativeTreeViewIds = new Dictionary<EntityId, int>();
        static readonly Dictionary<object, int> s_ManagedTreeViewIds = new Dictionary<object, int>(new ReferenceComparer());
        static readonly Dictionary<int, object> s_TreeViewTargets = new Dictionary<int, object>();
        static int s_NextTreeViewId = 1; // Zero is reserved for each tree's invisible root.

        sealed class ReferenceComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

            int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }

        [MenuItem("Window/Analysis/XR Interaction Debugger", false, 2100)]
        public static void Init()
        {
            if (s_Instance == null)
            {
                s_Instance = GetWindow<XRInteractionDebuggerWindow>();
                s_Instance.Show();
                s_Instance.titleContent = Contents.titleContent;
            }
            else
            {
                s_Instance.Show();
                s_Instance.Focus();
            }
        }

        void UpdateInputDevicesTree()
        {
            if (m_InputDevicesTree == null)
            {
                m_InputDevicesTree = XRInputDevicesTreeView.Create(ref m_InputDevicesTreeState, ref m_InputDevicesTreeHeaderState);
                m_InputDevicesTree.ExpandAll();
            }
        }

        void UpdateInteractorsTree()
        {
            var activeManagers = XRInteractionManager.activeInteractionManagers;
            if (m_InteractorsTree == null)
            {
                m_InteractorsTree = XRInteractorsTreeView.Create(activeManagers, ref m_InteractorsTreeState, ref m_InteractorsTreeHeaderState);
                m_InteractorsTree.ExpandAll();
            }
            else
            {
                m_InteractorsTree.UpdateManagersList(activeManagers);
            }
        }

        void UpdateInteractablesTree()
        {
            var activeManagers = XRInteractionManager.activeInteractionManagers;
            if (m_InteractablesTree == null)
            {
                m_InteractablesTree = XRInteractablesTreeView.Create(activeManagers, ref m_InteractablesTreeState, ref m_InteractablesTreeHeaderState);
                m_InteractablesTree.ExpandAll();
            }
            else
            {
                m_InteractablesTree.UpdateManagersList(activeManagers);
            }
        }

        void UpdateFiltersTree()
        {
            var enabledFilters = XRTargetFilter.enabledFilters;
            if (m_FiltersTree == null)
            {
                m_FiltersTree = XRTargetFiltersTreeView.Create(enabledFilters, ref m_FiltersTreeState, ref m_FiltersTreeHeaderState);
                m_FiltersTree.ExpandAll();
            }
            else
            {
                m_FiltersTree.UpdateFilterList(enabledFilters);
            }
        }

        void UpdateEvaluatorsScoreTree()
        {
            var selectedFilter = m_FiltersTree?.selectedFilter as XRTargetFilter;
            if (m_EvaluatorsScoreTree == null)
            {
                m_EvaluatorsScoreTree = XRTargetEvaluatorsScoreTreeView.Create(selectedFilter, ref m_EvaluatorsScoreTreeState, ref m_EvaluatorsScoreTreeHeaderState);
                m_EvaluatorsScoreTree.ExpandAll();
            }
            else if (selectedFilter != null && (m_EvaluatorsScoreTree.filter != selectedFilter || m_EvaluatorsScoreTree.EnabledEvaluatorListHasChanged()))
            {
                m_EvaluatorsScoreTree.Release();
                m_EvaluatorsScoreTree = XRTargetEvaluatorsScoreTreeView.Create(selectedFilter, ref m_EvaluatorsScoreTreeState, ref m_EvaluatorsScoreTreeHeaderState);
                m_EvaluatorsScoreTree.ExpandAll();
            }
        }

        public void OnEnable()
        {
            s_Instance = this;
            ReleaseTrees();
            ResetTreeViewIds();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            ReleaseTrees();
            ResetTreeViewIds();
            if (s_Instance == this)
                s_Instance = null;
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode && state != PlayModeStateChange.EnteredPlayMode)
                return;

            ReleaseTrees();
            ResetTreeViewIds();
        }

        void ReleaseTrees()
        {
            m_InputDevicesTree?.Release();
            m_InteractorsTree?.Release();
            m_InteractablesTree?.Release();
            m_FiltersTree?.Release();
            m_EvaluatorsScoreTree?.Release();
            m_InputDevicesTree = null;
            m_InteractorsTree = null;
            m_InteractablesTree = null;
            m_FiltersTree = null;
            m_EvaluatorsScoreTree = null;

            // Serialized row state belongs to the previous synthetic ID map.
            m_InputDevicesTreeState = null;
            m_InteractorsTreeState = null;
            m_InteractablesTreeState = null;
            m_FiltersTreeState = null;
            m_EvaluatorsScoreTreeState = null;
        }

        public void OnInspectorUpdate()
        {
            UpdateInputDevicesTree();
            UpdateInteractorsTree();
            UpdateInteractablesTree();

            UpdateFiltersTree();
            UpdateEvaluatorsScoreTree();

            m_InputDevicesTree?.Repaint();
            m_InteractorsTree?.Repaint();
            m_InteractablesTree?.Repaint();

            m_FiltersTree?.Repaint();
            if (m_EvaluatorsScoreTree != null)
            {
                m_EvaluatorsScoreTree.Reload();
                m_EvaluatorsScoreTree.Repaint();
            }

            Repaint();
        }

        public void OnGUI()
        {
            DrawToolbarGUI();
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            if (m_ShowInputDevices && m_InputDevicesTree != null)
                DrawInputDevicesGUI();
            if (m_ShowInteractors && m_InteractorsTree != null)
                DrawInteractorsGUI();
            if (m_ShowInteractables && m_InteractablesTree != null)
                DrawInteractablesGUI();
            if (m_ShowTargetFilters && m_FiltersTree != null)
                DrawFiltersGUI();

            EditorGUILayout.EndScrollView();
        }

        void DrawInputDevicesGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(Contents.inputDevices, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_InputDevicesTree.OnGUI(rect);
        }

        void DrawInteractorsGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(Contents.interactors, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_InteractorsTree.OnGUI(rect);
        }

        void DrawInteractablesGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(Contents.interactables, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_InteractablesTree.OnGUI(rect);
        }

        void DrawFiltersGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(Contents.targetFilters, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_FiltersTree.OnGUI(rect);

            if (m_EvaluatorsScoreTree != null)
                DrawEvaluatorsScoreGUI();
        }

        void DrawEvaluatorsScoreGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(Contents.score, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_EvaluatorsScoreTree.OnGUI(rect);
        }

        void DrawToolbarGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            m_ShowInputDevices
                = GUILayout.Toggle(m_ShowInputDevices, Contents.inputDevices, EditorStyles.toolbarButton);
            m_ShowInteractors
                = GUILayout.Toggle(m_ShowInteractors, Contents.interactors, EditorStyles.toolbarButton);
            m_ShowInteractables
                = GUILayout.Toggle(m_ShowInteractables, Contents.interactables, EditorStyles.toolbarButton);
            m_ShowTargetFilters
                = GUILayout.Toggle(m_ShowTargetFilters, Contents.targetFilters, EditorStyles.toolbarButton);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        internal static string JoinNames<T>(string separator, List<T> objects)
        {
            s_Names.Clear();
            foreach (var obj in objects)
            {
                var name = GetDisplayName(obj);
                s_Names.Add(name);
            }

            return string.Join(separator, s_Names);
        }

        internal static string GetLayerMaskDisplay(int layerSize, int interactionLayerMaskValue, string maskOn, string maskOff)
        {            
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < layerSize; i++)
            {
                var layerMaskValue = 1 << i;
                var maskString = (layerMaskValue & interactionLayerMaskValue) != 0? maskOn : maskOff;
                stringBuilder.Append(maskString);
            }

            return stringBuilder.ToString();
        }

        internal static List<int> GetActiveLayers(int layerSize, int interactionLayerMaskValue)
        {
            var layers = new List<int>();
            for (var i = 0; i < layerSize; i++)
            {
                var layerMaskValue = 1 << i;
                if ((layerMaskValue & interactionLayerMaskValue) != 0)
                {
                    layers.Add(i);
                }
            }
            return layers;
        }

        internal static string GetDisplayName(object obj)
        {
            if (obj is Object unityObject)
            {
                return unityObject != null ? unityObject.name : "<Destroyed>";
            }

            return obj.GetType().Name;
        }

        internal static string GetDisplayType(object obj)
        {
            if (obj is Object unityObject)
            {
                return unityObject != null ? unityObject.GetType().Name : "<Destroyed>";
            }

            return obj != null ? obj.GetType().Name : "<null>";
        }

        internal static int GetUniqueTreeViewId(object obj)
        {
            if (ReferenceEquals(obj, null))
                return 0;

            if (obj is Object unityObject)
            {
                var entityId = unityObject.GetEntityId();
                if (!s_NativeTreeViewIds.TryGetValue(entityId, out var nativeRowId))
                {
                    nativeRowId = s_NextTreeViewId++;
                    s_NativeTreeViewIds.Add(entityId, nativeRowId);
                    s_TreeViewTargets.Add(nativeRowId, obj);
                }

                return nativeRowId;
            }

            if (!s_ManagedTreeViewIds.TryGetValue(obj, out var id))
            {
                id = s_NextTreeViewId++;
                s_ManagedTreeViewIds.Add(obj, id);
                s_TreeViewTargets.Add(id, obj);
            }

            return id;
        }

        internal static object GetTreeViewTarget(int id)
        {
            return s_TreeViewTargets.TryGetValue(id, out var target) ? target : null;
        }

        internal static void SelectTreeViewObject(int id)
        {
            var target = GetTreeViewTarget(id) as Object;
            if (target == null)
                return;

            EditorGUIUtility.PingObject(target);
            Selection.activeObject = target;
        }

        static void ResetTreeViewIds()
        {
            s_NativeTreeViewIds.Clear();
            s_ManagedTreeViewIds.Clear();
            s_TreeViewTargets.Clear();
            s_NextTreeViewId = 1;
        }

        static class Contents
        {
            public static GUIContent titleContent = EditorGUIUtility.TrTextContent("XR Interaction Debugger");
            public static GUIContent inputDevices = EditorGUIUtility.TrTextContent("Input Devices");
            public static GUIContent interactables = EditorGUIUtility.TrTextContent("Interactables");
            public static GUIContent interactors = EditorGUIUtility.TrTextContent("Interactors");
            public static GUIContent targetFilters = EditorGUIUtility.TrTextContent("Target Filters");
            public static GUIContent score = EditorGUIUtility.TrTextContent("Score");
        }
    }
}
