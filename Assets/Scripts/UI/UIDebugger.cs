using UnityEngine;
using UnityEngine.UIElements;
using System.Text;

namespace VertexVanguard.UI
{
    public class UIDebugger : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        
        private void Start()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
                
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument not found!");
                return;
            }
            
            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("Root VisualElement is null!");
                return;
            }
            
            Debug.Log($"UIDocument root element found: {root.name}");
            
            // Print the UI hierarchy
            PrintUIHierarchy(root);
            
            // Try to find the buttons directly
            var startButton = root.Q("start-game-button");
            Debug.Log($"Direct Q search - start-game-button found: {startButton != null}");
            
            var settingsButton = root.Q("settings-button");
            Debug.Log($"Direct Q search - settings-button found: {settingsButton != null}");
            
            var exitButton = root.Q("exit-button");
            Debug.Log($"Direct Q search - exit-button found: {exitButton != null}");
            
            // Try different approaches to find buttons
            var allButtons = root.Query<Button>().ToList();
            Debug.Log($"Found {allButtons.Count} buttons in total using Query<Button>()");
            
            foreach (var button in allButtons)
            {
                Debug.Log($"Button found: name='{button.name}', text='{button.text}'");
            }
        }
        
        private void PrintUIHierarchy(VisualElement element, int depth = 0)
        {
            var indent = new StringBuilder();
            for (int i = 0; i < depth; i++)
                indent.Append("  ");
                
            Debug.Log($"{indent}Element: {element.name} (Type: {element.GetType().Name}, Class: {string.Join(", ", element.GetClasses())})");
            
            foreach (var child in element.Children())
            {
                PrintUIHierarchy(child, depth + 1);
            }
        }
    }
} 