﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using RuriLib.Functions.Requests;
using RuriLib.Models;

namespace RuriLib
{
    /// <summary>
    /// Extension methods for lists.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Randomizes the elements in a list.
        /// </summary>
        /// <param name="list">The list to shuffle</param>
        /// <param name="rng">The random number generator</param>
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    /// <summary>
    /// Extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces literal values of \n, \r\n and \t with the actual escape codes.
        /// </summary>
        /// <param name="str">The string to unescape</param>
        /// <param name="useEnvNewLine">Whether to unescape both \n and \r\n with the Environment.NewLine</param>
        /// <returns>The string with unescaped escape sequences.</returns>
        public static string Unescape(this string str, bool useEnvNewLine = false)
        {
            // Unescape only \n etc. not \\n
            str = Regex.Replace(str, @"(?<!\\)\\r\\n", useEnvNewLine ? Environment.NewLine : "\r\n");
            str = Regex.Replace(str, @"(?<!\\)\\n", useEnvNewLine ? Environment.NewLine : "\n");
            str = Regex.Replace(str, @"(?<!\\)\\t", "\t");

            // Replace \\n with \n
            return new StringBuilder(str)
                .Replace(@"\\r\\n", @"\r\n")
                .Replace(@"\\n", @"\n")
                .Replace(@"\\t", @"\t")
                .ToString();
        }

        /// <summary>
        /// Returns true if <paramref name="path"/> starts with the path <paramref name="baseDirPath"/>.
        /// The comparison is case-insensitive, handles / and \ slashes as folder separators and
        /// only matches if the base dir folder name is matched exactly ("c:\foobar\file.txt" is not a sub path of "c:\foo").
        /// </summary>
        public static bool IsSubPathOf(this string path, string baseDirPath)
        {
            string normalizedPath = Path.GetFullPath(path.Replace('/', '\\')
                .WithEnding("\\"));

            string normalizedBaseDirPath = Path.GetFullPath(baseDirPath.Replace('/', '\\')
                .WithEnding("\\"));

            return normalizedPath.StartsWith(normalizedBaseDirPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns <paramref name="str"/> with the minimal concatenation of <paramref name="ending"/> (starting from end) that
        /// results in satisfying .EndsWith(ending).
        /// </summary>
        /// <example>"hel".WithEnding("llo") returns "hello", which is the result of "hel" + "lo".</example>
        public static string WithEnding(this string str, string ending)
        {
            if (str == null)
                return ending;

            string result = str;

            // Right() is 1-indexed, so include these cases
            // * Append no characters
            // * Append up to N characters, where N is ending length
            for (int i = 0; i <= ending.Length; i++)
            {
                string tmp = result + ending.Right(i);
                if (tmp.EndsWith(ending))
                    return tmp;
            }

            return result;
        }

        /// <summary>Gets the rightmost <paramref name="length" /> characters from a string.</summary>
        /// <param name="value">The string to retrieve the substring from. Must not be null.</param>
        /// <param name="length">The number of characters to retrieve.</param>
        /// <returns>The substring.</returns>
        public static string Right(this string value, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", length, "Length is less than zero");
            }

            return (length < value.Length) ? value.Substring(value.Length - length) : value;
        }

        /// <summary>
        /// string is url
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsUrl(this string str)
        {
            Uri uriResult;
            return Uri.TryCreate(str, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static string TrySpaceSplit(this string value, int returnIndex = 0)
        {
            try
            {
                return value.Split(' ')[returnIndex];
            }
            catch { return value; }
        }

        public static string DoFormat(this double myNumber)
        {
            var s = string.Format("{0:0.00}", myNumber);

            if (s.EndsWith("00"))
            {
                return ((int)myNumber).ToString();
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Converts a string to byte array
        /// </summary>
        /// <param name="input">The string</param>
        /// <returns>The byte array</returns>
        public static byte[] ConvertToByteArray(this string input)
        {
            return input.Select(Convert.ToByte).ToArray();
        }

        /// <summary>
        /// Converts a byte array to a string
        /// </summary>
        /// <param name="bytes">the byte array</param>
        /// <returns>The string</returns>
        public static string ConvertToString(this byte[] bytes)
        {
            return new string(bytes.Select(Convert.ToChar).ToArray());
        }

        public static string GetUntilOrEmpty(this string text, string stopAt)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return String.Empty;
        }
    }

    /// <summary>
    /// Extension methods for SecurityProtocol enum.
    /// </summary>
    public static class SecurityProtocolExtensions
    {
        /// <summary>
        /// Converts the SecurityProtocol to an SslProtocols enum. Multiple protocols are not supported and SystemDefault is None.
        /// </summary>
        /// <param name="protocol">The SecurityProtocol</param>
        /// <returns>The converted SslProtocols.</returns>
        public static SslProtocols ToSslProtocols(this SecurityProtocol protocol)
        {
            return protocol.ToString().ToEnum(SslProtocols.None);
            //switch (protocol)
            //{
            //    case SecurityProtocol.SystemDefault:
            //        return SslProtocols.None;

            //    case SecurityProtocol.SSL3:
            //        return SslProtocols.Ssl3;

            //    case SecurityProtocol.TLS10:
            //        return SslProtocols.Tls;

            //    case SecurityProtocol.TLS11:
            //        return SslProtocols.Tls11;

            //    case SecurityProtocol.TLS12:
            //        return SslProtocols.Tls12;

            //    case SecurityProtocol.TLS13:
            //        return SslProtocols.Tls13;

            //    default:
            //        throw new Exception("Protocol not supported");
            //}
        }
    }

    /// <summary>
    /// Extension methods for Dispatcher.
    /// </summary>
    public static class DispatcherExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="action"></param>
        public static void InvokeIfRequired(this Dispatcher dispatcher, Action action)
        {
            if (dispatcher == null)
            {
                return;
            }
            if (!dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(action, DispatcherPriority.ContextIdle);
                return;
            }
            action();
        }
    }

    /// <summary>
    ///  Extension methods for bitmap
    /// </summary>
    public static class BitmapExtensions
    {
        public static System.Drawing.Imaging.ImageFormat GetImageFormat(this System.Drawing.Image img)
        {
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                return System.Drawing.Imaging.ImageFormat.Jpeg;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                return System.Drawing.Imaging.ImageFormat.Bmp;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                return System.Drawing.Imaging.ImageFormat.Png;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Emf))
                return System.Drawing.Imaging.ImageFormat.Emf;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Exif))
                return System.Drawing.Imaging.ImageFormat.Exif;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                return System.Drawing.Imaging.ImageFormat.Gif;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon))
                return System.Drawing.Imaging.ImageFormat.Icon;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp))
                return System.Drawing.Imaging.ImageFormat.MemoryBmp;
            if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
                return System.Drawing.Imaging.ImageFormat.Tiff;
            else
                return System.Drawing.Imaging.ImageFormat.Wmf;
        }

        public static Bitmap ConvertPixelFormat(this Image image, PixelFormat pixelFormat)
        {
            Bitmap orig = new Bitmap(image);
            Bitmap clone = new Bitmap(orig.Width, orig.Height,
                pixelFormat);

            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(orig, new Rectangle(0, 0, clone.Width, clone.Height));
            }
            return clone;
        }

    }

    /// <summary>
    /// extension to the System.Enum-enum. 
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///  Converts the string representation of the name or numeric value of one or more
        ///  enumerated constants to an equivalent enumerated object.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="strEnumValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue, bool ignoreCase = true)
        {
            //if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
            //    return defaultValue;
            try
            {
                return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue, ignoreCase);
            }
            catch (Exception ex)
            {
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// extension to the System.Type-type.
    /// </summary>
    public static class TypeExtension
    {
        /// <summary>
        /// This Methode extends the System.Type-type to get all extended methods. It searches hereby in all assemblies which are known by the current AppDomain.
        /// </summary>
        /// <remarks>
        /// Insired by Jon Skeet from his answer on http://stackoverflow.com/questions/299515/c-sharp-reflection-to-identify-extension-methods
        /// </remarks>
        /// <returns>returns MethodInfo[] with the extended Method</returns>

        //public static MethodInfo GetFiltersExtensionMethod(Type t, string methodName)
        //{
        //    List<Type> AssTypes = new List<Type>();

        //    AssTypes.AddRange(typeof(Extensions).Assembly.GetTypes());

        //    var query = from type in AssTypes
        //                where type.IsSealed && !type.IsGenericType && !type.IsNested
        //                from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        //                where method.IsDefined(typeof(ExtensionAttribute), false)
        //                where method.GetParameters()[0].ParameterType == t
        //                select method;

        //    var mi = from methode in query.ToArray<MethodInfo>()
        //             where methode.Name == methodName
        //             select methode;
        //    return mi.FirstOrDefault();

        //}

        public static MethodInfo[] GetExtensionMethods<T>(this Type t)
        {
            List<Type> AssTypes = new List<Type>();

            AssTypes.AddRange(typeof(T).Assembly.GetTypes());

            var query = from type in AssTypes
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == t
                        select method;
            return query.ToArray<MethodInfo>();
        }

        ///// <summary>
        ///// Extends the System.Type-type to search for a given extended MethodeName.
        ///// </summary>
        ///// <param name="t">type</param>
        ///// <param name="MethodeName">Name of the Methode</param>
        ///// <returns>the found Methode or null</returns>
        //public static MethodInfo GetExtensionMethod<T>(this Type t, string MethodeName)
        //{
        //    var mi = from methode in t.GetExtensionMethods<T>()
        //             where methode.Name == MethodeName
        //             select methode;
        //    return mi.FirstOrDefault();
        //    //if (mi.Count<MethodInfo>() <= 0)
        //    //    return null;
        //    //else
        //    //    return mi.First<MethodInfo>();
        //}

        public static ConstructorInfo GetByParamsCount(this ConstructorInfo[] constructorInfos,
            int paramsCount)
        {
            return constructorInfos
                .FirstOrDefault(c => c.GetParameters().Length == paramsCount);
        }

        public static bool IsColor(this Type type)
        {
            return type == typeof(Color);
        }

        /// <summary>
        /// check if a Type is a nullable enum
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsNullableEnum(this Type t, out Type enumType)
        {
            Type u = Nullable.GetUnderlyingType(t);
            enumType = u;
            return (u != null) && u.IsEnum;
        }

        public static bool IsNullable(this Type type, out Type nullableType)
        {
            var ty = Nullable.GetUnderlyingType(type);
            nullableType = ty;
            return ty != null;
        }

        public static IEnumerable<Type> GetEnumTypes(this Type type, BindingFlags bindingAttr, bool getCanWrite = true)
        {
            return type.GetProperties(bindingAttr).Where(p => p.PropertyType.IsEnum && p.CanWrite == getCanWrite)
                .Select(e => e.PropertyType);
        }
    }

    /// <summary>
    ///  Extension methods for method info
    /// </summary>
    public static class MethodInfoExtensions
    {
        public static Delegate CreateDelegate(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (!method.IsStatic)
            {
                throw new ArgumentException("The provided method must be static.", "method");
            }

            if (method.IsGenericMethod)
            {
                throw new ArgumentException("The provided method must not be generic.", "method");
            }

            return method.CreateDelegate(Expression.GetDelegateType(
                (from parameter in method.GetParameters()
                 select parameter.ParameterType)
                .Concat(new[] { method.ReturnType })
                .ToArray()));
        }
    }

    /// <summary>
    ///  Extension methods for loliscript
    /// </summary>
    public static class ScriptExtension
    {
        /// <summary>
        /// Has block 'blockName' in config
        /// </summary>
        /// <param name="script"></param>
        /// <param name="blockName"></param>
        /// <param name="contains"></param>
        /// <returns></returns>
        public static bool HasBlock(string script, string blockName, bool contains = false)
        {
            if (string.IsNullOrWhiteSpace(script) ||
                string.IsNullOrWhiteSpace(blockName))
                return false;
            if (contains)
            {
                return script.Contains(blockName);
            }

            try
            {
                return script.Split(new char[] { '\n' },
                    StringSplitOptions.RemoveEmptyEntries)
                    .Any(s => s.TrySpaceSplit(s.StartsWith("#") ? 1 : 0) == blockName);
            }
            catch { }
            return false;
        }

        public static bool HasScript(string script, string code, bool contains = false)
        {
            if (string.IsNullOrWhiteSpace(script) ||
                string.IsNullOrWhiteSpace(code))
                return false;
            if (contains)
            {
                return script.Contains(code);
            }

            try
            {
                return script.Split(new char[] { '\n' },
                   StringSplitOptions.RemoveEmptyEntries)
                   .Any(s => s.TrySpaceSplit(s.StartsWith("#") ? 3 : 2) == code);
            }
            catch { }
            return false;
        }

    }

    /// <summary>
    ///  Extension methods for path file
    /// </summary>
    public static class PathExtensions
    {
        public static string RemoveIllegalCharacters(this string illegal)
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(illegal, "");
        }

        public static string CreateFileName(this string fileName, string newFileName,
            bool removeIllegalCharacters = false)
        {
            if (removeIllegalCharacters) { newFileName = newFileName.RemoveIllegalCharacters(); }

            var text2 = Path.GetDirectoryName(fileName);
            if (!text2.EndsWith("\\"))
            {
                text2 += "\\";
            }
            return text2 + newFileName + Path.GetExtension(fileName);
        }

        public static string Rename(this string fullPath)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }
    }

    /// <summary>
    /// Extension methods for Directory
    /// </summary>
    public static class DirExtensions
    {
        public static string[] GetFiles(string sourceFolder, string filters, SearchOption searchOption)
        {
            return filters.Split('|')
                .SelectMany(f => Directory.GetFiles(sourceFolder, f, searchOption))
                .ToArray();
        }
    }

    /// <summary>
    /// Extension methods for object
    /// </summary>
    public static class ObjectExtensions
    {
        public static T[] Remove<T>(this T[] original, T itemToRemove)
        {
            int numIdx = Array.IndexOf(original, itemToRemove);
            if (numIdx == -1) return original;
            List<T> tmp = new List<T>(original);
            tmp.RemoveAt(numIdx);
            return tmp.ToArray();
        }

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        /*
         public static T[] RemoveAt<T> (this T[] arr, int index) {
    return arr.Where ((e, i) => i != index).ToArray ();
}
         */

    }
    /// <summary>
    /// Extension methods for convert time
    /// </summary>
    public static class ConvertTimeExtensions
    {
        public static double ConvertMillisecondsToNanoseconds(double milliseconds)
        {
            return milliseconds * 1000000;
        }

        public static double ConvertMicrosecondsToNanoseconds(double microseconds)
        {
            return microseconds * 1000;
        }

        public static double ConvertMillisecondsToMicroseconds(double milliseconds)
        {
            return milliseconds * 1000;
        }

        public static double ConvertNanosecondsToMilliseconds(double nanoseconds)
        {
            return nanoseconds * 0.000001;
        }

        public static double ConvertMicrosecondsToMilliseconds(double microseconds)
        {
            return microseconds * 0.001;
        }

        public static double ConvertNanosecondsToMicroseconds(double nanoseconds)
        {
            return nanoseconds * 0.001;
        }
    }

    /// <summary>
    /// Extension methods for accessing Microseconds and Nanoseconds of a
    /// DateTime object.
    /// </summary>
    public static class DateTimeExtensionMethods
    {
        /// <summary>
        /// The number of ticks per microsecond.
        /// </summary>
        public const int TicksPerMicrosecond = 10;
        /// <summary>
        /// The number of ticks per Nanosecond.
        /// </summary>
        public const int NanosecondsPerTick = 100;

        /// <summary>
        /// Gets the microsecond fraction of a DateTime.
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static int Microseconds(this DateTime self)
        {
            return (int)Math.Floor(
               (self.Ticks
               % TimeSpan.TicksPerMillisecond)
               / (double)TicksPerMicrosecond);
        }
        /// <summary>
        /// Gets the Nanosecond fraction of a DateTime.  Note that the DateTime
        /// object can only store nanoseconds at resolution of 100 nanoseconds.
        /// </summary>
        /// <param name="self">The DateTime object.</param>
        /// <returns>the number of Nanoseconds.</returns>
        public static int Nanoseconds(this DateTime self)
        {
            return (int)(self.Ticks % TimeSpan.TicksPerMillisecond % TicksPerMicrosecond)
               * NanosecondsPerTick;
        }
        /// <summary>
        /// Adds a number of microseconds to this DateTime object.
        /// </summary>
        /// <param name="self">The DateTime object.</param>
        /// <param name="microseconds">The number of milliseconds to add.</param>
        public static DateTime AddMicroseconds(this DateTime self, int microseconds)
        {
            return self.AddTicks(microseconds * TicksPerMicrosecond);
        }
        /// <summary>
        /// Adds a number of nanoseconds to this DateTime object.  Note: this
        /// object only stores nanoseconds of resolutions of 100 seconds.
        /// Any nanoseconds passed in lower than that will be rounded using
        /// the default rounding algorithm in Math.Round().
        /// </summary>
        /// <param name="self">The DateTime object.</param>
        /// <param name="nanoseconds">The number of nanoseconds to add.</param>
        public static DateTime AddNanoseconds(this DateTime self, int nanoseconds)
        {
            return self.AddTicks((int)Math.Round(nanoseconds / (double)NanosecondsPerTick));
        }
    }
    /// <summary>
    /// Extension methods for wordlist
    /// </summary>
    public static class WordlistExtensions
    {
        public static Wordlist Clone(this Wordlist wordlist)
        {
            return new Wordlist(wordlist.Name, wordlist.Path, wordlist.Type, wordlist.Purpose,
                temporary: wordlist.Temporary, subwordlists: wordlist.SubWordlists);
        }
    }
    /// <summary>
    /// Extension methods for task
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Blocks while condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The condition that will perpetuate the block.</param>
        /// <param name="frequency">The frequency at which the condition will be check, in milliseconds.</param>
        /// <param name="timeout">Timeout in milliseconds.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <returns></returns>
        public static async Task WaitWhile(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
                throw new TimeoutException();
        }

        /// <summary>
        /// Blocks until condition is true or timeout occurs.
        /// </summary>
        /// <param name="condition">The break condition.</param>
        /// <param name="frequency">The frequency at which the condition will be checked.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        /// <returns></returns>
        public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeout)))
                throw new TimeoutException();
        }
    }
    /// <summary>
    /// Extension methods for data size
    /// </summary>
    public static class SizeExtensions
    {
        static readonly string[] SizeSuffixes = { "MB", "GB" };
        public static string SizeSuffix(long value, int decimalPlaces = 0)
        {
            if (value < 0)
            {
                throw new ArgumentException("Bytes should not be negative", "value");
            }
            var mag = (int)Math.Max(0, Math.Log(value, 1024));
            var adjustedSize = Math.Round(value / Math.Pow(1024, mag), decimalPlaces);
            return String.Format("{0} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }

    /// <summary>
    /// Extension methods for notepad
    /// </summary>
    public static class NotepadExtensions
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowText")]
        private static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        /// <summary>
        /// show text in notepad
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        public static void ShowText(string text = null, string title = "Log")
        {
            Process notepad = Process.Start(new ProcessStartInfo("notepad.exe"));
            if (notepad != null)
            {
                notepad.WaitForInputIdle();

                if (!string.IsNullOrEmpty(title))
                    SetWindowText(notepad.MainWindowHandle, title);

                if (!string.IsNullOrEmpty(text))
                {
                    IntPtr child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
                    SendMessage(child, 0x000C, 0, text);
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for notepad++
    /// </summary>
    public static class NotepadPlusExtensions
    {
        [DllImport("User32.dll")]
        static extern int SendMessageW(IntPtr hWnd, int uMsg, int wParam, string lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        private static IntPtr notepadPlus;

        private static Process notepadPlusProc;

        public static void ShowText(string text)
        {
            var npp = Start();
            if (npp != null && (notepadPlus = npp.MainWindowHandle) != IntPtr.Zero)
            {
                var handle = FindWindowEx(notepadPlus, new IntPtr(0), "Scintilla", null);

                SendMessageW(handle, 0x00C2, 0, text);
                //0x00B1 -> select all
            }
        }

        public static void Clear()
        {
            if (notepadPlus == null && notepadPlus != IntPtr.Zero)
            {
                Process npp;
                if ((npp = Start()) == null)
                    return;
                notepadPlus = npp.MainWindowHandle;
            }
            var handle = FindWindowEx(notepadPlus, new IntPtr(0), "Scintilla", null);
            SendMessageW(handle, 0x000C, 0, string.Empty);
        }

        private static Process Start()
        {
            if (notepadPlusProc == null || notepadPlusProc.HasExited || notepadPlusProc?.MainWindowHandle == IntPtr.Zero)
            {
                ProcessStartInfo processStart = new ProcessStartInfo("notepad++")
                {
                    WindowStyle = ProcessWindowStyle.Minimized,
                    Arguments = "-multiInst -nosession"
                };

                notepadPlusProc = Process.Start(processStart);
                notepadPlusProc.WaitForInputIdle();
            }
            return notepadPlusProc;
        }

        public static void Close()
        {
            try { notepadPlusProc?.Kill(); } catch { }
        }

    }

    public static class LinearGradientBrushExtensions
    {
        public static System.Windows.Media.LinearGradientBrush GetLinearGradientBrush(this System.Windows.Media.Color color)
        {
            return new System.Windows.Media.LinearGradientBrush(
                new System.Windows.Media.GradientStopCollection() {
                    new System.Windows.Media.GradientStop() {
                        Color = color } });
        }

        public static System.Windows.Media.Color ColorConverter(this string color)
        {
            return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color);
        }

    }

    public static class CookieCollectionExtensions
    {
        public static IEnumerable<Cookie> GetAllCookies(this CookieContainer c)
        {
            Hashtable k = (Hashtable)c.GetType().GetField("m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(c);
            foreach (DictionaryEntry element in k)
            {
                SortedList l = (SortedList)element.Value.GetType().GetField("m_list", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(element.Value);
                foreach (var e in l)
                {
                    var cl = (CookieCollection)((DictionaryEntry)e).Value;
                    foreach (Cookie fc in cl)
                    {
                        yield return fc;
                    }
                }
            }
        }
    }
}
