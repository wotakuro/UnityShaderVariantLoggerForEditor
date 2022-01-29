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
        public abstract class MenuItem
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
                    if(type.IsSubclassOf(typeof(MenuItem) ) ){
                        itemTypes.Add(type);
                    }
                }
            }
            return itemTypes;
        }

        private List<MenuItem> menuItems;

        private void OnEnable()
        {
            if(menuItemTypes == null)
            {
                menuItemTypes = ConstructMenuItemTypes();
            }
            menuItems = new List<MenuItem>();
            foreach ( var menuType in menuItemTypes)
            {
                var menu = System.Activator.CreateInstance(menuType) as MenuItem;
                menuItems.Add(menu);

                menu.OnEnable();
            }
        }


    }
}