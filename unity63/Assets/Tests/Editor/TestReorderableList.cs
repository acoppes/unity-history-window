using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TestReorderableList : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/TestReorderableList")]
    public static void ShowExample()
    {
        TestReorderableList wnd = GetWindow<TestReorderableList>();
        wnd.titleContent = new GUIContent("TestReorderableList");
    }

    public void CreateGUI()
    {
        var listOfElements = new List<string>()
        {
            "Heellllo11",
            "Yeah"
        };
        
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        
        VisualElement mainFromXml = m_VisualTreeAsset.Instantiate();
        root.Add(mainFromXml);

        var listView = mainFromXml.Q<ListView>("MainList");
        // listView.makeItem = () => new Label("Hello1");
        listView.itemsSource = listOfElements;
        listView.bindItem = (element, i) =>
        {
            element.Q<Label>().text = listOfElements[i];
        };
        
        // Set up list view so that you can add or remove items dynamically.
        listView.showAddRemoveFooter = true;
        listView.allowAdd = true;
        // listView.fixedItemHeight = 16;
        
        var listView2 = mainFromXml.Q<ListView>("MainList2");
        listView2.itemsSource = listOfElements;
        
        listView2.bindItem = (element, i) =>
        {
            element.Q<Label>("Favorite").text = listOfElements[i];
        };
    }
}
