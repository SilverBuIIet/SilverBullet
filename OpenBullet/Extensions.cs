﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using PluginFramework;
using RuriLib;

namespace OpenBullet
{
    public static class BlocksExtensions
    {
        public static IEnumerable<BlockBase> OnlyPlugins(this IEnumerable<BlockBase> blocks)
        {
            return blocks.Where(b => b.IsPlugin());
        }

        public static bool IsPlugin(this BlockBase block)
        {
            return block.GetType().GetInterface(nameof(IBlockPlugin)) == typeof(IBlockPlugin);
        }
    }

    public static class EnumerableExtensions
    {
        public static void SaveToFile<T>(this IEnumerable<T> items, string fileName, Func<T, string> mapping)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("The filename must not be empty");
            }

            File.WriteAllLines(fileName, items.Select(i => mapping(i)));
        }

        public static void CopyToClipboard<T>(this IEnumerable<T> items, Func<T, string> mapping)
        {
            Clipboard.SetText(string.Join(Environment.NewLine, items.Select(i => mapping(i))));
        }
    }

    public static class RichTextBoxExtensions
    {

        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
        }

        public static string[] Lines(this RichTextBox box)
        {
            var textRange = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);
            return textRange.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string GetText(this RichTextBox box)
        {
            var textRange = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);
            return textRange.Text;
        }

        public static string GetTextFromLines(this RichTextBox box)
        {
            return box.Lines().Aggregate((current, next) => current + next);
        }

        public static string Select(this RichTextBox rtb, int offset, int length, Color color)
        {
            // Get text selection:
            TextSelection textRange = rtb.Selection;

            // Get text starting point:
            TextPointer start = rtb.Document.ContentStart;

            // Get begin and end requested:
            TextPointer startPos = GetTextPointAt(start, offset);
            TextPointer endPos = GetTextPointAt(start, offset + length);

            // New selection of text:
            textRange.Select(startPos, endPos);

            // Apply property to the selection:
            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(color));

            // Return selection text:
            return rtb.Selection.Text;
        }

        public static TextPointer GetTextPointAt(TextPointer from, int pos)
        {
            TextPointer ret = from;
            int i = 0;

            while ((i < pos) && (ret != null))
            {
                if ((ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text) || (ret.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.None))
                    i++;

                if (ret.GetPositionAtOffset(1, LogicalDirection.Forward) == null)
                    return ret;

                ret = ret.GetPositionAtOffset(1, LogicalDirection.Forward);
            }

            return ret;
        }
    }

    public static class TabControlExtensions
    {
        public static TabItem GetItemByItemName(this IEnumerable<TabItem> tabItems,
            string name)
        {
            return tabItems.FirstOrDefault(i => i.Header?.ToString() == name);
        }

        public static int GetIndexByItemName(this TabControl tabControl,
            string name)
        {
            return tabControl.Items.IndexOf(GetItemByItemName(tabControl.Items.OfType<TabItem>(), name));
        }

        public static int SelectIndexByHeaderName(this TabControl tabControl,
            string headerName)
        {
            return tabControl.SelectedIndex = GetIndexByItemName(tabControl, headerName);
        }

    }

    public static class ColorExtensions
    {
        public static uint ColorToUInt(this Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | (color.B << 0));
        }

        public static String ConvertToString(this Color c)
        {
            return c.R.ToString() + "," + c.G.ToString() + "," + c.B.ToString();
        }
    }

    public static class AnimationExtensions
    {
        /// <summary>
        /// Turning blur on
        /// </summary>
        public static void BlurApply(this UIElement element, double from, double to,
            TimeSpan duration, bool autoReverse = false)
        {
            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = duration,
                AutoReverse = autoReverse
            };
            var effect = new BlurEffect();
            element.Effect = effect;
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(storyboard, element.Effect);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath("Radius"));
            storyboard.Begin();
        }

        /// <summary>
        /// Turning blur off
        /// </summary>
        /// <param name="element">bluring element</param>
        /// <param name="duration">blur animation duration</param>
        public static void BlurDisable(this UIElement element, TimeSpan duration)
        {
            BlurEffect blur = element.Effect as BlurEffect;
            if (blur == null || blur.Radius == 0)
            {
                return;
            }
            DoubleAnimation blurDisable = new DoubleAnimation(blur.Radius, 0, duration);
            blur.BeginAnimation(BlurEffect.RadiusProperty, blurDisable);
        }
    }
}
