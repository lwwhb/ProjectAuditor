using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Profiling;

namespace Unity.ProjectAuditor.Editor.UI
{
    internal struct AnalysisViewDescriptor
    {
        public IssueCategory category;
        public string name;
        public bool groupByDescription;
        public bool showAssemblySelection;
        public bool showCritical;
        public bool showInvertedCallTree;
        public bool showFilenameColumn;
        public bool showAssemblyColumn;
    }

    internal class AnalysisView
    {
        private readonly ProjectAuditorConfig m_Config;
        private readonly AnalysisViewDescriptor m_Desc;
        private readonly IIssuesFilter m_Filter;

        public IssueTable m_Table;

        public AnalysisView(AnalysisViewDescriptor desc, ProjectAuditorConfig config, IIssuesFilter filter)
        {
            m_Desc = desc;
            m_Config = config;
            m_Filter = filter;
            m_Table = null;
        }

        public AnalysisViewDescriptor desc
        {
            get { return m_Desc; }
        }

        public void CreateTable()
        {
            if (m_Table != null)
                return;

            var state = new TreeViewState();
            var columnsList = new List<MultiColumnHeaderState.Column>();
            var numColumns = (int)IssueTable.Column.Count;
            for (var i = 0; i < numColumns; i++)
            {
                var width = 0;
                var minWidth = 0;
                switch ((IssueTable.Column)i)
                {
                    case IssueTable.Column.Description:
                        width = 300;
                        minWidth = 100;
                        break;
                    case IssueTable.Column.Priority:
                        if (m_Desc.showCritical)
                        {
                            width = 22;
                            minWidth = 22;
                        }
                        break;
                    case IssueTable.Column.Area:
                        width = 60;
                        minWidth = 50;
                        break;
                    case IssueTable.Column.Filename:
                        if (m_Desc.showFilenameColumn)
                        {
                            width = 180;
                            minWidth = 100;
                        }

                        break;
                    case IssueTable.Column.Assembly:
                        if (m_Desc.showAssemblyColumn)
                        {
                            width = 180;
                            minWidth = 100;
                        }

                        break;
                }

                columnsList.Add(new MultiColumnHeaderState.Column
                {
                    headerContent = Styles.ColumnHeaders[i],
                    width = width,
                    minWidth = minWidth,
                    autoResize = true
                });
            }

            m_Table = new IssueTable(state,
                new MultiColumnHeader(new MultiColumnHeaderState(columnsList.ToArray())),
                m_Desc.groupByDescription,
                m_Config,
                m_Filter);
        }

        public void AddIssues(IEnumerable<ProjectIssue> issues)
        {
            m_Table.AddIssues(issues.Where(i => i.category == m_Desc.category).ToArray());
        }

        public void OnGUI()
        {
            var r = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));

            Profiler.BeginSample("IssueTable.OnGUI");
            m_Table.OnGUI(r);
            Profiler.EndSample();
            var selectedItems = m_Table.GetSelectedItems();
            var selectedIssues = selectedItems.Select(i => i.ProjectIssue).ToArray();
            var info = selectedIssues.Length + " / " + m_Table.GetNumMatchingIssues() + " issues";

            EditorGUILayout.LabelField(info, GUILayout.ExpandWidth(true), GUILayout.Width(200));
        }

        private static class Styles
        {
            public static readonly GUIContent[] ColumnHeaders =
            {
                new GUIContent("Issue", "Issue description"),
                new GUIContent(" ! ", "Issue priority"),
                new GUIContent("Area", "The area the issue might have an impact on"),
                new GUIContent("Filename", "Filename and line number"),
                new GUIContent("Assembly", "Managed Assembly name")
            };
        }
    }
}
