using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace UTJ.VariantLogger
{
    public class VariantLoggerWindow : EditorWindow
    {

        private const string MenuName = "Tools/UTJ/ShaderVariantLogger";

        public abstract class UIMenuItem
        {
            public abstract string toolbar { get; }
            public abstract int order { get; }
            VisualElement _rootVisualElement = new VisualElement();
            public VisualElement rootVisualElement {
                get {
                    return _rootVisualElement;
                }
            }
            public abstract void OnEnable();

            public override int GetHashCode()
            {
                return this.GetType().GetHashCode();
            }
            public override bool Equals(object obj)
            {
                UIMenuItem item = obj as UIMenuItem;
                if(item == null) { return false; }
                if (this.GetType() != obj.GetType())
                {
                    return false;
                }
                return true;
            }
        }

        private static List<System.Type> menuItemTypes = null;

        private static List<System.Type> ConstructMenuItemTypes(){
            var itemTypes = new List<System.Type>();

            var asms = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach( var asm in asms)
            {
                var types = asm.GetTypes();
                foreach( var type in types)
                {
                    if(type.IsSubclassOf(typeof(UIMenuItem) ) ){
                        itemTypes.Add(type);
                    }
                }
            }
            return itemTypes;
        }

        private List<UIMenuItem> uiMenuItems;
        private List<ToolbarToggle> uiToolbarToggles;
        private Dictionary<ToolbarToggle, VisualElement> uiItemBody;

        private void OnEnable()
        {
            this.titleContent = new GUIContent("ShaderVariantLogger");

            if (menuItemTypes == null)
            {
                menuItemTypes = ConstructMenuItemTypes();
            }
            this.uiMenuItems = new List<UIMenuItem>();
            foreach ( var menuType in menuItemTypes)
            {
                var menu = System.Activator.CreateInstance(menuType) as UIMenuItem;
                uiMenuItems.Add(menu);

                menu.OnEnable();
            }
            var toolBar = new Toolbar();
            this.rootVisualElement.Add(toolBar);

            this.uiToolbarToggles = new List<ToolbarToggle>();
            this.uiItemBody = new Dictionary<ToolbarToggle, VisualElement>();

            int idx = 0;
            foreach (var menu in uiMenuItems)
            {
                var toggle = new ToolbarToggle( );
                toggle.text = menu.toolbar;
                toggle.userData = menu;
                toggle.RegisterValueChangedCallback(OnChangeToolBar);
                toolBar.Add(toggle);
                uiToolbarToggles.Add(toggle);
                toggle.SetValueWithoutNotify(idx == 0);

                menu.rootVisualElement.SetEnabled(idx == 0);
                this.rootVisualElement.Add(menu.rootVisualElement);
                uiItemBody.Add(toggle, menu.rootVisualElement);

                ++idx;
            }
        }

        private void OnChangeToolBar(ChangeEvent<bool> itemValue)
        {
            var target = itemValue.target as ToolbarToggle;
            if (!itemValue.newValue)
            {
                target.SetValueWithoutNotify(true);
                return;
            }
            foreach (var toolbar in uiToolbarToggles)
            {
                if(target == toolbar) { continue; }
                toolbar.SetValueWithoutNotify(false);
                var body = uiItemBody[toolbar];
                if (body != null) { body.visible = false; }
            }
            uiItemBody[target].visible = true;


        }

        [MenuItem(MenuName)]
        public static void Create()
        {
            EditorWindow.GetWindow<VariantLoggerWindow>();
        }



    }
}