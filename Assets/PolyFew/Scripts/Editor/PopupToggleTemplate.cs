#if UNITY_EDITOR

namespace BrainFailProductions.PolyFew
{


    using UnityEngine;
    using UnityEditor;
    using System;

    public class PopupToggleTemplate : PopupWindowContent
    {

        public ToggleDefinition[] togglesDefinitions;
        public Vector2 windowSize;
        public Action OnPopupOpen;
        public Action OnPopupClose;


        public PopupToggleTemplate(ToggleDefinition[] togglesDefinitions, Vector2 windowSize, Action OnPopupOpen, Action OnPopupClose)
        {
            this.togglesDefinitions = togglesDefinitions;
            this.windowSize = windowSize;
            this.OnPopupOpen = OnPopupOpen;
            this.OnPopupClose = OnPopupClose;
        }



        public class ToggleDefinition
        {
            public GUIContent content;
            public int rightPadding;
            public int bottomPadding;
            public Action<bool> Setter;
            public Func<bool> Getter;

            public ToggleDefinition(GUIContent content, int rightPadding, int bottomPadding, Action<bool> setter, Func<bool> getter)
            {
                this.content = content;
                this.rightPadding = rightPadding;
                this.bottomPadding = bottomPadding;
                Setter = setter;
                Getter = getter;
            }
        }




        public override Vector2 GetWindowSize()
        {
            return windowSize;
        }

        public override void OnGUI(Rect rect)
        {

            if (togglesDefinitions != null && togglesDefinitions.Length > 0)
            {

                GUILayout.BeginArea(new Rect(rect), EditorStyles.miniButton);


                for (int a = 0; a < togglesDefinitions.Length; a++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);


                    ToggleDefinition definition = togglesDefinitions[a];

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(definition.content, GUILayout.Width(definition.rightPadding));
                    bool result = EditorGUILayout.Toggle("", definition.Getter(), GUILayout.Width(25));
                    GUILayout.EndHorizontal();

                    definition.Setter(result);


                    EditorGUILayout.EndVertical();

                    if (a != togglesDefinitions.Length - 1) { GUILayout.Space(definition.bottomPadding); }
                }


                GUILayout.EndArea();

            }

        }



        public override void OnOpen() { OnPopupOpen?.Invoke(); }

        public override void OnClose() { OnPopupClose?.Invoke(); }

    }

}


#endif