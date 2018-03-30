using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YoloTrain.Mvvm
{
    /// <summary>
    /// KeyBindingHelper
    /// </summary>
    public static class KeyBindingHelper
    {
        /// <summary>Sets the key bindings.</summary>
        /// <param name="target">The target <see cref="UIElement" />.</param>
        /// <param name="menuitems">The menu items.</param>
        public static void SetKeyBindings(UIElement target, ItemCollection menuitems)
        {
            if (menuitems == null)
                throw new ArgumentNullException(nameof(menuitems));

            foreach (var item in menuitems)
            {
                MenuItem menuItem = item as MenuItem;
                if (menuItem == null)
                    continue;

                string gestureText = menuItem.InputGestureText;
                if (!string.IsNullOrWhiteSpace(gestureText) && menuItem.Command != null)
                {
                    ModifierKeys modifiers = ModifierKeys.None;
                    string[] keyTexts = gestureText.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < keyTexts.Length; i++)
                    {
                        string keyText = keyTexts[i];

                        if (i == keyTexts.Length - 1)
                        {
                            if (char.IsDigit(keyText[0]))
                            {
                                keyText = "D" + keyText;
                            }

                            const string arrowText = " Arrow";
                            if (keyText.EndsWith(arrowText))
                            {
                                keyText = keyText.Substring(0, keyText.Length - arrowText.Length);
                            }

                            if (Enum.TryParse(keyText, true, out Key key))
                            {
                                //KeyGestureConverter x = new KeyGestureConverter(); // TODO: Might be able to use this instead
                                target.InputBindings.Add(new KeyBinding(menuItem.Command, key, modifiers));
                                Debug.WriteLine($"{menuItem.Header} = {modifiers}+{key}"); // TODO: Take out
                            }
                            else
                            {
                                throw new InvalidDataException($"'{gestureText}' cannot be parsed.");
                            }
                        }
                        else
                        {
                            if (keyText == "Ctrl")
                                keyText = "Control";

                            if (Enum.TryParse(keyText, true, out ModifierKeys modifierKey))
                            {
                                modifiers |= modifierKey;
                            }
                            else
                            {
                                throw new InvalidDataException($"'{gestureText}' cannot be parsed.");
                            }
                        }
                    }
                }

                SetKeyBindings(target, menuItem.Items);
            }
        }
    }
}
