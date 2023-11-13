﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Media;
using Humanizer;
using Microsoft.IdentityModel.Tokens;
using RuriLib.Functions.Crypto;
using RuriLib.Functions.EvalString;
using RuriLib.Functions.Formats;
using RuriLib.Functions.NTLM;
using RuriLib.Functions.Time;
using RuriLib.Functions.UserAgent;
using RuriLib.Functions.WordToNum;
using RuriLib.LS;

namespace RuriLib
{
    /// <summary>
    /// A block that can execute a specific function on one or multiple inputs.
    /// </summary>
    public class BlockFunction : BlockBase
    {
        /// <summary>
        /// The function name.
        /// </summary>
        public enum Function
        {
            /// <summary>Simply replaced the variables of the input.</summary>
            Constant,

            /// <summary>Encodes an input as a base64 string.</summary>
            Base64Encode,

            /// <summary>Decodes the string from a base64-encoded input.</summary>
            Base64Decode,

            /// <summary>Hashes an input string.</summary>
            Hash,

            /// <summary>Generates a HMAC for a given string.</summary>
            HMAC,

            /// <summary>Translates words in a given string.</summary>
            Translate,

            ///<summary>DateTime Now</summary>
            CurrentDate,

            ///<summary>time</summary>
            CurrentTime,

            ///<summary>week</summary>
            DayOfWeek,

            ///<summary>day</summary>
            CurrentDay,

            ///<summary>month</summary>
            CurrentMonth,

            ///<summary>year</summary>
            CurrentYear,

            /// <summary>Converts a formatted date to a unix timestamp.</summary>
            DateToUnixTime,

            /// <summary>convert date miladi to shamsi</summary>
            DateToSolar,

            /// <summary>convert date shamsi to miladi</summary>
            DateToGregorian,

            ///<summary></summary>
            GetRemainingDay,

            /// <summary>Gets the length of a string.</summary>
            Length,

            /// <summary>Converts all uppercase caracters in a string to lowercase.</summary>
            ToLowercase,

            /// <summary>Converts all lowercase characters in a string to uppercase.</summary>
            ToUppercase,

            /// <summary>
            /// Find and extract a letter from a string
            /// </summary>
            ToLetter,

            /// <summary>
            /// Find and extract a number from a string
            /// </summary>
            ToDigit,

            /// <summary>
            /// Find and extract a letter Or digit from a string
            /// </summary>
            ToLetterOrDigit,

            /// <summary>Convert integers to written numbers</summary>
            NumberToWords,

            /// <summary>Convert word to number</summary>
            WordsToNumber,

            /// <summary>Replaces some text with something else, with or without using regex.</summary>
            Replace,

            /// <summary>Gets the first match for a specific regex pattern.</summary>
            RegexMatch,

            /// <summary>Encodes the input to be used in a URL.</summary>
            URLEncode,

            /// <summary>Decodes a URL-encoded input.</summary>
            URLDecode,

            /// <summary>Unescapes characters in a string.</summary>
            Unescape,

            /// <summary>Encodes the input to be displayed in HTML or XML.</summary>
            HTMLEntityEncode,

            /// <summary>Decoded an input containing HTML or XML entities.</summary>
            HTMLEntityDecode,

            ///<summary>character encoding</summary>
            Encoding,

            /// <summary>Converts a unix timestamp to a formatted date.</summary>
            UnixTimeToDate,

            /// <summary>Retrieves the current time as a unix timestamp.</summary>
            CurrentUnixTime,

            /// <summary>Converts a unix timestamp to the ISO8601 format.</summary>
            UnixTimeToISO8601,

            /// <summary>Generates a random integer.</summary>
            RandomNum,

            /// <summary>Generates a random string based on a mask.</summary>
            RandomString,

            /// <summary>Evaluating string "3 * 5 + Pow(2,3)" yield int 23</summary>
            EvaluateMathString,

            /// <summary>Rounds a decimal input to the upper integer.</summary>
            Ceil,

            /// <summary>Rounds a decimal input to the lower integer.</summary>
            Floor,

            /// <summary>Rounds a decimal input to the nearest integer.</summary>
            Round,

            /// <summary>the absolute value of a double-precision floating-point number</summary>
            Abs,

            /// <summary>Computes mathematical operations between decimal numbers.</summary>
            Compute,

            /// <summary>Counts the occurrences of a string in another string.</summary>
            CountOccurrences,

            /// <summary>Clears the cookie jar used for HTTP requests.</summary>
            ClearCookies,

            /// <summary>Encrypts a string with RSA.</summary>
            RSAEncrypt,

            // <summary>Decrypts a string with RSA.</summary>
            // RSADecrypt,

            /// <summary>Encrypts a string with RSA PKCS1PAD2.</summary>
            RSAPKCS1PAD2,

            /// <summary>Waits a given amount of milliseconds.</summary>
            Delay,

            /// <summary>Retrieves the character at a given index in the input string.</summary>
            CharAt,

            /// <summary>
            ///Splits a string into substrings based on the strings in an array. You can specify
            ///whether the substrings include empty array elements.
            /// </summary>
            Split,

            ///<summary></summary>
            Remove,

            /// <summary>Gets a substring of the input.</summary>
            Substring,

            /// <summary>Reverses the input string.</summary>
            ReverseString,

            /// <summary>Removes leading or trailing whitespaces from a string.</summary>
            Trim,

            /// <summary>Gets a valid random User-Agent header.</summary>
            GetRandomUA,

            /// <summary>Encrypts a string with AES.</summary>
            AESEncrypt,

            /// <summary>Decrypts an AES-encrypted string.</summary>
            AESDecrypt,

            /// <summary>Generates a key using a password based KDF.</summary>
            PBKDF2PKCS5,

            /// <summary>Generates an OAuth Verfier.</summary>
            GenerateOAuthVerifier,

            ///<summary>Generates an OAuth Challenge using the Verifer as Input.</summary>
            GenerateOAuthChallenge,

            ///<summary>Generates a GUID</summary>
            GenerateGUID,

            ///<summary>Generates a certain amount of bytes based on input</summary>
            GenerateBytes,

            /// <summary>Generates NTLM hash</summary>
            Ntlm,

            /// <summary>Encrypts a string with Scrypt.</summary>
            SCrypt,

            /// <summary>Encrypts a string with Bcrypt.</summary>
            BCrypt,
        }

        /// <summary>
        /// Date to unix time 
        /// </summary>
        public enum DateToUnixTimeType
        {
            /// <summary>Converts a formatted date to a unix timestamp Seconds</summary>
            Seconds,
            /// <summary>Converts a formatted date to a unix timestamp Miliseconds</summary>
            Miliseconds,
        }

        /// <summary>
        /// Encoding methods
        /// </summary>
        public enum EncodingMethods
        {
            ///<summary>encodes all the characters in the specified character array into a sequence of bytes.</summary>
            GetBytes,
            ///<summary>decodes a specified number of bytes starting at a specified address into a string.</summary>
            GetString,
        }

        /// <summary>
        /// Scrypt methods
        /// </summary>
        public enum ScryptMethods
        {
            /// <summary>Hash a password using the scrypt scheme</summary>
            Encode,
            /// <summary>Compares a password against a hashed password.</summary>
            Compare,
            /// <summary>Checks if the given hash is a valid scrypt hash</summary>
            IsValid
        }

        /// <summary>
        /// BCrypt methods
        /// </summary>
        public enum BCryptMethods
        {
            /// <summary>Hash a password using the bcrypt scheme</summary>
            Encode,
            GenerateSalt,
            /// <summary>Verify a password against a hashed password.</summary>
            Verify,
        }

        #region General Properties
        private string variableName = "";
        /// <summary>The name of the output variable.</summary>
        public string VariableName { get { return variableName; } set { variableName = value; OnPropertyChanged(); } }

        private bool isCapture = false;
        /// <summary>Whether the output variable should be marked for Capture.</summary>
        public bool IsCapture { get { return isCapture; } set { isCapture = value; OnPropertyChanged(); } }

        private string inputString = "";
        /// <summary>The input string on which the function will be executed (not always needed).</summary>
        public string InputString { get { return inputString; } set { inputString = value; OnPropertyChanged(); } }

        private Function functionType = Function.Constant;
        /// <summary>The function to execute.</summary>
        public Function FunctionType { get { return functionType; } set { functionType = value; OnPropertyChanged(); } }
        #endregion

        #region Function Specific Properties
        // -- Hash & Hmac
        private Hash hashType = Hash.SHA512;
        /// <summary>The hashing function to use.</summary>
        public Hash HashType { get { return hashType; } set { hashType = value; OnPropertyChanged(); } }

        private bool inputBase64 = false;
        /// <summary>Whether the input is a base64-encoded string instead of UTF8.</summary>
        public bool InputBase64 { get { return inputBase64; } set { inputBase64 = value; OnPropertyChanged(); } }

        // -- Hmac
        private string hmacKey = "";
        /// <summary>The key used to authenticate the message.</summary>
        public string HmacKey { get { return hmacKey; } set { hmacKey = value; OnPropertyChanged(); } }

        private bool hmacBase64 = false;
        /// <summary>Whether to output the message as a base64-encoded string instead of a hex-encoded string.</summary>
        public bool HmacBase64 { get { return hmacBase64; } set { hmacBase64 = value; OnPropertyChanged(); } }

        private bool keyBase64 = false;
        /// <summary>Whether the HMAC Key is a base64-encoded string instead of UTF8.</summary>
        public bool KeyBase64 { get { return keyBase64; } set { keyBase64 = value; OnPropertyChanged(); } }

        // -- Translate
        private bool stopAfterFirstMatch = true;
        /// <summary>Whether to stop translating after the first match.</summary>
        public bool StopAfterFirstMatch { get { return stopAfterFirstMatch; } set { stopAfterFirstMatch = value; OnPropertyChanged(); } }

        private bool useVar;
        /// <summary>
        /// use variable 
        /// </summary>
        public bool UseVar { get { return useVar; } set { useVar = value; OnPropertyChanged(); } }

        /// <summary>The dictionary containing the words and their translation.</summary>
        public Dictionary<string, string> TranslationDictionary { get; set; } = new Dictionary<string, string>();

        // -- Date to unix
        private string dateFormat = "yyyy-MM-dd:HH-mm-ss";
        /// <summary>The format of the date (y = year, M = month, d = day, H = hour, m = minute, s = second).</summary>
        public string DateFormat { get { return dateFormat; } set { dateFormat = value; OnPropertyChanged(); } }

        // -- string replace
        private string replaceWhat = "";
        /// <summary>The text to replace.</summary>
        public string ReplaceWhat { get { return replaceWhat; } set { replaceWhat = value; OnPropertyChanged(); } }

        private string replaceWith = "";
        /// <summary>The replacement text.</summary>
        public string ReplaceWith { get { return replaceWith; } set { replaceWith = value; OnPropertyChanged(); } }

        private bool useRegex = false;
        /// <summary>Whether to use regex for replacing.</summary>
        public bool UseRegex { get { return useRegex; } set { useRegex = value; OnPropertyChanged(); } }

        // -- Regex Match
        private string regexMatch = "";
        /// <summary>The regex pattern to match.</summary>
        public string RegexMatch { get { return regexMatch; } set { regexMatch = value; OnPropertyChanged(); } }

        // -- Random Number
        private string randomMin = "0";
        /// <summary>The minimum random number that can be generated (inclusive).</summary>
        public string RandomMin { get { return randomMin; } set { randomMin = value; OnPropertyChanged(); } }

        private string randomMax = "0";
        /// <summary>The maximum random number that can be generated (exclusive).</summary>
        public string RandomMax { get { return randomMax; } set { randomMax = value; OnPropertyChanged(); } }

        private bool randomZeroPad = false;
        /// <summary>Whether to pad with zeros on the left to match the length of the maximum provided.</summary>
        public bool RandomZeroPad { get { return randomZeroPad; } set { randomZeroPad = value; OnPropertyChanged(); } }

        // -- CountOccurrences
        private string stringToFind = "";
        /// <summary>The string to count the occurrences of.</summary>
        public string StringToFind { get { return stringToFind; } set { stringToFind = value; OnPropertyChanged(); } }

        // -- RSA
        private string rsaN = "";
        /// <summary>The modulus of the RSA public key as a base64 string.</summary>
        public string RsaN { get { return rsaN; } set { rsaN = value; OnPropertyChanged(); } }

        private string rsaE = "";
        /// <summary>The exponent of the RSA public key as a base64 string.</summary>
        public string RsaE { get { return rsaE; } set { rsaE = value; OnPropertyChanged(); } }

        private string rsaD = "";
        /// <summary>The exponent of the RSA private key as a base64 string.</summary>
        public string RsaD { get { return rsaD; } set { rsaD = value; OnPropertyChanged(); } }

        private bool rsaOAEP = true;
        /// <summary>Whether to use OAEP padding instead of PKCS v1.5.</summary>
        public bool RsaOAEP { get { return rsaOAEP; } set { rsaOAEP = value; OnPropertyChanged(); } }

        // --- CharAt
        private string charIndex = "0";
        /// <summary>The index of the wanted character.</summary>
        public string CharIndex { get { return charIndex; } set { charIndex = value; OnPropertyChanged(); } }

        private string separator;
        public string Separator
        {
            get => separator;
            set
            {
                separator = value;
                OnPropertyChanged();
            }
        }

        private int splitIndex = 1;
        public int SplitIndex { get => splitIndex; set { splitIndex = value; OnPropertyChanged(); } }

        private StringSplitOptions stringSplitOption;
        public StringSplitOptions StringSplitOption
        {
            get => stringSplitOption;
            set
            {
                stringSplitOption = value; OnPropertyChanged();
            }
        }

        // -- RemoveSIndex

        private string removeSIndex;
        public string RemoveSIndex
        {
            get => removeSIndex;
            set
            {
                removeSIndex = value;
                OnPropertyChanged();
            }
        }

        private string removeCount;
        public string RemoveCount { get => removeCount; set { removeCount = value; OnPropertyChanged(); } }

        // -- Substring
        private string substringIndex = "0";
        /// <summary>The starting index for the substring.</summary>
        public string SubstringIndex { get { return substringIndex; } set { substringIndex = value; OnPropertyChanged(); } }

        private string substringLength = "1";
        /// <summary>The length of the wanted substring.</summary>
        public string SubstringLength { get { return substringLength; } set { substringLength = value; OnPropertyChanged(); } }

        // -- User Agent
        private bool userAgentSpecifyBrowser = false;
        /// <summary>Whether to only limit the UA generation to a certain browser.</summary>
        public bool UserAgentSpecifyBrowser { get { return userAgentSpecifyBrowser; } set { userAgentSpecifyBrowser = value; OnPropertyChanged(); } }

        private UserAgent.Browser userAgentBrowser = UserAgent.Browser.Chrome;
        /// <summary>The browser for which the User Agent should be generated.</summary>
        public UserAgent.Browser UserAgentBrowser { get { return userAgentBrowser; } set { userAgentBrowser = value; OnPropertyChanged(); } }

        // -- AES
        private string aesKey = "";
        /// <summary>The keys used for AES encryption and decryption as a base64 string.</summary>
        public string AesKey { get { return aesKey; } set { aesKey = value; OnPropertyChanged(); } }

        private string aesIV = "";
        /// <summary>The initial value as a base64 string.</summary>
        public string AesIV { get { return aesIV; } set { aesIV = value; OnPropertyChanged(); } }

        private CipherMode aesMode = CipherMode.CBC;
        /// <summary>The cipher mode.</summary>
        public CipherMode AesMode { get { return aesMode; } set { aesMode = value; OnPropertyChanged(); } }

        private PaddingMode aesPadding = PaddingMode.None;
        /// <summary>The padding mode.</summary>
        public PaddingMode AesPadding { get { return aesPadding; } set { aesPadding = value; OnPropertyChanged(); } }

        private bool hexKeys;
        /// <summary>
        /// String keys to hex
        /// </summary>
        public bool HexKeys { get => hexKeys; set { hexKeys = value; OnPropertyChanged(); } }

        // -- PBKDF2PKCS5
        private string kdfSalt = "";
        /// <summary>The KDF's salt as a base64 string.</summary>
        public string KdfSalt { get { return kdfSalt; } set { kdfSalt = value; OnPropertyChanged(); } }

        private int kdfSaltSize = 8;
        /// <summary>The size of the generated salt (in bytes) in case none is specified.</summary>
        public int KdfSaltSize { get { return kdfSaltSize; } set { kdfSaltSize = value; OnPropertyChanged(); } }

        private int kdfIterations = 1;
        /// <summary>The number of times to perform the algorithm.</summary>
        public int KdfIterations { get { return kdfIterations; } set { kdfIterations = value; OnPropertyChanged(); } }

        private int kdfKeySize = 16;
        /// <summary>The size of the generated key (in bytes).</summary>
        public int KdfKeySize { get { return kdfKeySize; } set { kdfKeySize = value; OnPropertyChanged(); } }

        private Hash kdfAlgorithm = Hash.SHA1;
        /// <summary>The size of the generated salt (in bytes) in case none is specified.</summary>
        public Hash KdfAlgorithm { get { return kdfAlgorithm; } set { kdfAlgorithm = value; OnPropertyChanged(); } }

        private DateToUnixTimeType unixTimeType = DateToUnixTimeType.Seconds;
        /// <summary>
        /// Unix time type
        /// </summary>
        public DateToUnixTimeType UnixTimeType
        {
            get => unixTimeType;
            set { unixTimeType = value; OnPropertyChanged(); }
        }

        private object getEncoding;
        /// <summary>
        /// Encoding name/codepage
        /// </summary>
        public object GetEncoding
        {
            get { return getEncoding; }
            set { getEncoding = value; OnPropertyChanged(); }
        }

        private EncodingMethods encFunc;
        /// <summary>
        /// Encoding method
        /// </summary>
        public EncodingMethods EncFunc
        {
            get { return encFunc; }
            set { encFunc = value; OnPropertyChanged(); }
        }

        private ScryptMethods scryptMeth;
        /// <summary>
        /// Scrypt method
        /// </summary>
        public ScryptMethods ScryptMeth
        {
            get { return scryptMeth; }
            set { scryptMeth = value; OnPropertyChanged(); }
        }

        private string scryptSalt = "";
        public string ScryptSalt
        {
            get { return scryptSalt; }
            set { scryptSalt = value; OnPropertyChanged(); }
        }

        private int scryptCost = 1024;
        public int ScryptCost
        {
            get { return scryptCost; }
            set { scryptCost = value; OnPropertyChanged(); }
        }

        private int scryptBlockSize = 1;
        public int ScryptBlockSize
        {
            get { return scryptBlockSize; }
            set { scryptBlockSize = value; OnPropertyChanged(); }
        }

        private int scryptOutputLength = 16;
        public int ScryptOutputLength
        {
            get { return scryptOutputLength; }
            set { scryptOutputLength = value; OnPropertyChanged(); }
        }

        private bool scryptBase64Output = false;
        /// <summary>
        /// Encode scrypt output to base64
        /// </summary>
        public bool Base64Output
        {
            get { return scryptBase64Output; }
            set { scryptBase64Output = value; OnPropertyChanged(); }
        }

        private string scryptHashedPassword;
        /// <summary>
        /// hashed password with scrypt
        /// </summary>
        public string ScryptHashedPassword
        {
            get { return scryptHashedPassword; }
            set
            {
                scryptHashedPassword = value;
                OnPropertyChanged();
            }
        }

        private BCryptMethods bcryptMeth;
        /// <summary>
        /// BCrypt method
        /// </summary>
        public BCryptMethods BCryptMeth
        {
            get { return bcryptMeth; }
            set { bcryptMeth = value; OnPropertyChanged(); }
        }

        private string bcryptHashedPassword = "";
        /// <summary>
        /// hashed password with bcrypt
        /// </summary>
        public string BCryptHashedPassword
        {
            get { return bcryptHashedPassword; }
            set
            {
                bcryptHashedPassword = value;
                OnPropertyChanged();
            }
        }

        private int bcryptWorkFactor;

        public int BCryptWorkFactor
        {
            get { return bcryptWorkFactor; }
            set
            {
                bcryptWorkFactor = value;
                OnPropertyChanged();
            }
        }

        private string bcryptSalt = "";

        public string BCryptSalt
        {
            get { return bcryptSalt; }
            set
            {
                bcryptSalt = value;
                OnPropertyChanged();
            }
        }

        private bool useBCryptWorkFactor;
        /// <summary>
        /// bcrypt work factor
        /// </summary>
        public bool UseWorkFactor
        {
            get { return useBCryptWorkFactor; }
            set
            {
                useBCryptWorkFactor = value;
                OnPropertyChanged();
            }
        }


        #endregion

        #region RandomString Properties
        private static readonly string _lowercase = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string _uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string _digits = "0123456789";
        private static readonly string _symbols = "\\!\"£$%&/()=?^'{}[]@#,;.:-_*+";
        private static readonly string _hex = _digits + "abcdef";
        private static readonly string _udChars = _uppercase + _digits;
        private static readonly string _ldChars = _lowercase + _digits;
        private static readonly string _upperlwr = _lowercase + _uppercase;
        private static readonly string _ludChars = _lowercase + _uppercase + _digits;
        private static readonly string _allChars = _lowercase + _uppercase + _digits + _symbols;

        #endregion

        /// <summary>
        /// Creates a Function block.
        /// </summary>
        public BlockFunction()
        {
            Label = "FUNCTION";
        }

        /// <inheritdoc />
        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            /*
             * Syntax:
             * FUNCTION Name [ARGUMENTS] ["INPUT STRING"] [-> VAR/CAP "NAME"]
             * */

            // Parse the function
            FunctionType = (Function)LineParser.ParseEnum(ref input, "Function Name", typeof(Function));

            // Parse specific function parameters
            switch (FunctionType)
            {
                case Function.Hash:
                    HashType = LineParser.ParseEnum(ref input, "Hash Type", typeof(Hash));
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.HMAC:
                    HashType = LineParser.ParseEnum(ref input, "Hash Type", typeof(Hash));
                    HmacKey = LineParser.ParseLiteral(ref input, "HMAC Key");
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.Translate:
                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    TranslationDictionary = new Dictionary<string, string>();
                    while (input != string.Empty && LineParser.Lookahead(ref input) == TokenType.Parameter)
                    {
                        LineParser.EnsureIdentifier(ref input, "KEY");
                        var k = LineParser.ParseLiteral(ref input, "Key");
                        LineParser.EnsureIdentifier(ref input, "VALUE");
                        var v = LineParser.ParseLiteral(ref input, "Value");
                        TranslationDictionary[k] = v;
                    }
                    break;

                case Function.DateToUnixTime:
                    {
                        DateFormat = LineParser.ParseLiteral(ref input, "DATE FORMAT");
                        var tempInput = input;
                        try { UnixTimeType = LineParser.ParseEnum(ref input, "UnixTimeType", typeof(DateToUnixTimeType)); } catch { input = tempInput; }
                    }
                    break;

                case Function.UnixTimeToDate:
                    {
                        DateFormat = LineParser.ParseLiteral(ref input, "DATE FORMAT");
                        // a little backward compatability with the old line format.
                        if (LineParser.Lookahead(ref input) != TokenType.Literal)
                        {
                            InputString = DateFormat;
                            DateFormat = "yyyy-MM-dd:HH-mm-ss";
                        }
                        var tempInput = input;
                        try { UnixTimeType = LineParser.ParseEnum(ref input, "UnixTimeType", typeof(DateToUnixTimeType)); } catch { input = tempInput; }
                    }
                    break;

                case Function.Replace:
                    ReplaceWhat = LineParser.ParseLiteral(ref input, "What");
                    ReplaceWith = LineParser.ParseLiteral(ref input, "With");
                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.RegexMatch:
                    RegexMatch = LineParser.ParseLiteral(ref input, "Pattern");
                    break;

                case Function.RandomNum:
                    if (LineParser.Lookahead(ref input) == TokenType.Literal)
                    {
                        RandomMin = LineParser.ParseLiteral(ref input, "Minimum");
                        RandomMax = LineParser.ParseLiteral(ref input, "Maximum");
                    }
                    // Support for old integer definition of Min and Max
                    else
                    {
                        RandomMin = LineParser.ParseInt(ref input, "Minimum").ToString();
                        RandomMax = LineParser.ParseInt(ref input, "Maximum").ToString();
                    }

                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.CountOccurrences:
                    StringToFind = LineParser.ParseLiteral(ref input, "string to find");
                    break;

                case Function.CharAt:
                    CharIndex = LineParser.ParseLiteral(ref input, "Index");
                    break;

                case Function.Split:
                    Separator = LineParser.ParseLiteral(ref input, nameof(Separator));
                    SplitIndex = LineParser.ParseInt(ref input, "Split Index");
                    if (input.StartsWith($"{nameof(StringSplitOptions.RemoveEmptyEntries)} \""))
                    {
                        try { StringSplitOption = LineParser.ParseEnum(ref input, "String Split Option", typeof(StringSplitOptions)); } catch { }
                    }
                    break;

                case Function.Remove:
                    RemoveSIndex = LineParser.ParseLiteral(ref input, "SIndex");
                    RemoveCount = LineParser.ParseLiteral(ref input, "Count");
                    break;

                case Function.Substring:
                    SubstringIndex = LineParser.ParseLiteral(ref input, "Index");
                    SubstringLength = LineParser.ParseLiteral(ref input, "Length");
                    break;

                case Function.RSAEncrypt:
                    RsaN = LineParser.ParseLiteral(ref input, "Public Key Modulus");
                    RsaE = LineParser.ParseLiteral(ref input, "Public Key Exponent");
                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                /*
            case Function.RSADecrypt:
                RsaN = LineParser.ParseLiteral(ref input, "Public Key Modulus");
                RsaD = LineParser.ParseLiteral(ref input, "Private Key Exponent");
                if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                    LineParser.SetBool(ref input, this);
                break;
                */

                case Function.RSAPKCS1PAD2:
                    RsaN = LineParser.ParseLiteral(ref input, "Public Key Modulus");
                    RsaE = LineParser.ParseLiteral(ref input, "Public Key Exponent");
                    break;

                case Function.GetRandomUA:
                    if (LineParser.ParseToken(ref input, TokenType.Parameter, false, false) == "BROWSER")
                    {
                        LineParser.EnsureIdentifier(ref input, "BROWSER");
                        UserAgentSpecifyBrowser = true;
                        UserAgentBrowser = LineParser.ParseEnum(ref input, "BROWSER", typeof(UserAgent.Browser));
                    };
                    break;

                case Function.AESDecrypt:
                case Function.AESEncrypt:
                    AesKey = LineParser.ParseLiteral(ref input, "Key");
                    AesIV = LineParser.ParseLiteral(ref input, "IV");
                    AesMode = LineParser.ParseEnum(ref input, "Cipher mode", typeof(CipherMode));
                    AesPadding = LineParser.ParseEnum(ref input, "Padding mode", typeof(PaddingMode));
                    if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);
                    break;

                case Function.PBKDF2PKCS5:
                    if (LineParser.Lookahead(ref input) == TokenType.Literal) KdfSalt = LineParser.ParseLiteral(ref input, "Salt");
                    else KdfSaltSize = LineParser.ParseInt(ref input, "Salt size");
                    KdfIterations = LineParser.ParseInt(ref input, "Iterations");
                    KdfKeySize = LineParser.ParseInt(ref input, "Key size");
                    KdfAlgorithm = LineParser.ParseEnum(ref input, "Algorithm", typeof(Hash));
                    break;

                case Function.Encoding:
                    GetEncoding = LineParser.ParseLiteral(ref input, "Encoding name/codepage");
                    EncFunc = LineParser.ParseEnum(ref input, "Encoding Methods", typeof(EncodingMethods));
                    break;

                case Function.SCrypt:
                    ScryptMeth = LineParser.ParseEnum(ref input, "Scrypt Methods", typeof(ScryptMethods));

                    if (ScryptMeth == ScryptMethods.Encode)
                    {
                        ScryptSalt = LineParser.ParseLiteral(ref input, "Scrypt salt");
                        ScryptCost = LineParser.ParseInt(ref input, "Scrypt cost");
                        ScryptBlockSize = LineParser.ParseInt(ref input, "Scrypt block size");
                        ScryptOutputLength = LineParser.ParseInt(ref input, "Scrypt Output Length");

                        if (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        {
                            LineParser.SetBool(ref input, this);
                        }
                    }

                    if (ScryptMeth == ScryptMethods.Compare)
                    {
                        ScryptHashedPassword = LineParser.ParseLiteral(ref input, "Hashed Password");
                    }
                    break;

                case Function.BCrypt:
                    BCryptMeth = LineParser.ParseEnum(ref input, "BCrypt Methods", typeof(BCryptMethods));

                    BCryptSalt = LineParser.ParseLiteral(ref input, "Salt");

                    while (LineParser.Lookahead(ref input) == TokenType.Boolean)
                        LineParser.SetBool(ref input, this);

                    if (UseWorkFactor)
                    {
                        BCryptWorkFactor = LineParser.ParseInt(ref input, "BCrypt Work Factor");
                    }

                    if (BCryptMeth == BCryptMethods.Verify)
                    {
                        BCryptHashedPassword = LineParser.ParseLiteral(ref input, "Hashed Password");
                    }

                    break;

                default:
                    break;
            }

            // Try to parse the input string
            if (LineParser.Lookahead(ref input) == TokenType.Literal)
                InputString = LineParser.ParseLiteral(ref input, "INPUT");

            // Try to parse the arrow, otherwise just return the block as is with default var name and var / cap choice
            if (LineParser.ParseToken(ref input, LS.TokenType.Arrow, false) == string.Empty)
                return this;

            // Parse the VAR / CAP
            try
            {
                var varType = LineParser.ParseToken(ref input, TokenType.Parameter, true);
                if (varType.ToUpper() == "VAR" || varType.ToUpper() == "CAP")
                    IsCapture = varType.ToUpper() == "CAP";
            }
            catch { throw new ArgumentException("Invalid or missing variable type"); }

            // Parse the variable/capture name
            try { VariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);
            writer
                .Label(Label)
                .Token("FUNCTION")
                .Token(FunctionType);

            switch (FunctionType)
            {
                case Function.Hash:
                    writer
                        .Token(HashType)
                        .Boolean(InputBase64, nameof(InputBase64));
                    break;

                case Function.HMAC:
                    writer
                        .Token(HashType)
                        .Literal(HmacKey)
                        .Boolean(InputBase64, nameof(InputBase64))
                        .Boolean(HmacBase64, nameof(HmacBase64))
                        .Boolean(KeyBase64, nameof(KeyBase64));
                    break;

                case Function.Translate:
                    writer.Boolean(StopAfterFirstMatch, nameof(StopAfterFirstMatch))
                        .Boolean(UseVar, nameof(UseVar));
                    foreach (var t in TranslationDictionary)
                        writer
                            .Indent()
                            .Token("KEY")
                            .Literal(t.Key)
                            .Token("VALUE")
                            .Literal(t.Value);

                    writer
                        .Indent();
                    break;

                case Function.UnixTimeToDate:
                case Function.DateToUnixTime:
                    {
                        writer.Literal(DateFormat);
                        if (UnixTimeType != DateToUnixTimeType.Seconds)
                            writer.Token(UnixTimeType);
                    }
                    break;

                case Function.Replace:
                    writer
                        .Literal(ReplaceWhat)
                        .Literal(ReplaceWith)
                        .Boolean(UseRegex, nameof(UseRegex));
                    break;

                case Function.RegexMatch:
                    writer
                        .Literal(RegexMatch, nameof(RegexMatch));
                    break;

                case Function.RandomNum:
                    writer
                        .Literal(RandomMin)
                        .Literal(RandomMax)
                        .Boolean(RandomZeroPad, nameof(RandomZeroPad));
                    break;

                case Function.CountOccurrences:
                    writer
                        .Literal(StringToFind);
                    break;

                case Function.CharAt:
                    writer.Literal(CharIndex);
                    break;

                case Function.Split:
                    if (StringSplitOption == StringSplitOptions.None)
                    {
                        writer.Literal(Separator)
                         .Integer(SplitIndex);
                    }
                    else
                        writer.Literal(Separator)
                            .Integer(SplitIndex)
                            .Token(StringSplitOption, nameof(StringSplitOption));
                    break;

                case Function.Remove:
                    writer.Literal(RemoveSIndex)
                        .Literal(RemoveCount);
                    break;

                case Function.Substring:
                    writer
                        .Literal(SubstringIndex)
                        .Literal(SubstringLength);
                    break;

                case Function.RSAEncrypt:
                    writer
                        .Literal(RsaN)
                        .Literal(RsaE)
                        .Boolean(RsaOAEP, nameof(RsaOAEP));
                    break;

                /*
            case Function.RSADecrypt:
                writer
                    .Literal(RsaN)
                    .Literal(RsaD)
                    .Boolean(RsaOAEP, "RsaOAEP");
                break;
                */

                case Function.RSAPKCS1PAD2:
                    writer
                        .Literal(RsaN)
                        .Literal(RsaE);
                    break;

                case Function.GetRandomUA:
                    if (UserAgentSpecifyBrowser)
                    {
                        writer
                            .Token("BROWSER")
                            .Token(UserAgentBrowser);
                    }
                    break;

                case Function.AESDecrypt:
                case Function.AESEncrypt:
                    writer
                        .Literal(AesKey)
                        .Literal(AesIV)
                        .Token(AesMode)
                        .Token(AesPadding)
                        .Boolean(HexKeys, nameof(HexKeys));
                    break;

                case Function.PBKDF2PKCS5:
                    if (KdfSalt != string.Empty) writer.Literal(KdfSalt);
                    else writer.Integer(KdfSaltSize);
                    writer
                        .Integer(KdfIterations)
                        .Integer(KdfKeySize)
                        .Token(KdfAlgorithm);
                    break;

                case Function.Encoding:
                    writer.Literal((GetEncoding ?? string.Empty).ToString())
                        .Token(EncFunc);
                    break;

                case Function.SCrypt:
                    writer.Token(ScryptMeth);

                    if (ScryptMeth == ScryptMethods.Encode)
                    {
                        writer.Literal(ScryptSalt)
                            .Integer(ScryptCost)
                            .Integer(ScryptBlockSize)
                            .Integer(ScryptOutputLength);
                        if (Base64Output)
                        {
                            writer.Boolean(Base64Output, "Base64Output");
                        }
                    }

                    else if (ScryptMeth == ScryptMethods.Compare)
                    {
                        writer.Literal(ScryptHashedPassword);
                    }
                    break;

                case Function.BCrypt:
                    writer.Token(BCryptMeth)
                        .Literal(BCryptSalt);
                    if (UseWorkFactor)
                    {
                        writer.Boolean(UseWorkFactor, nameof(UseWorkFactor))
                           .Integer(BCryptWorkFactor, nameof(BCryptWorkFactor));
                    }
                    if (BCryptMeth == BCryptMethods.Verify)
                    {
                        writer.Literal(BCryptHashedPassword);
                    }
                    break;
            }

            writer
                .Literal(InputString, "InputString");
            if (!writer.CheckDefault(VariableName, "VariableName"))
                writer
                    .Arrow()
                    .Token(IsCapture ? "CAP" : "VAR")
                    .Literal(VariableName);

            return writer.ToString();
        }

        private static readonly NumberStyles _style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
        private static readonly IFormatProvider _provider = new CultureInfo("en-US");

        /// <inheritdoc />
        public override void Process(BotData data)
        {
            base.Process(data);

            var localInputStrings = ReplaceValuesRecursive(inputString, data);
            var outputs = new List<string>();

            for (int i = 0; i < localInputStrings.Count; i++)
            {
                var localInputString = localInputStrings[i];
                var outputString = "";

                switch (FunctionType)
                {
                    case Function.Constant:
                        outputString = localInputString;
                        break;

                    case Function.Base64Encode:
                        outputString = localInputString.ToBase64();
                        break;

                    case Function.Base64Decode:
                        outputString = localInputString.FromBase64();
                        break;

                    case Function.HTMLEntityEncode:
                        outputString = WebUtility.HtmlEncode(localInputString);
                        break;

                    case Function.HTMLEntityDecode:
                        outputString = WebUtility.HtmlDecode(localInputString);
                        break;

                    case Function.Hash:
                        outputString = GetHash(localInputString, hashType, InputBase64).ToLower();
                        break;

                    case Function.HMAC:
                        outputString = Hmac(localInputString, hashType, ReplaceValues(hmacKey, data), InputBase64, KeyBase64, HmacBase64);
                        break;

                    case Function.Translate:
                        outputString = localInputString;
                        foreach (var entry in TranslationDictionary.OrderBy(e => e.Key.Length).Reverse())
                        {
                            if (outputString.Contains(entry.Key))
                            {
                                if (UseVar)
                                    outputString = outputString.Replace(ReplaceValues(entry.Key, data), ReplaceValues(entry.Value, data));
                                else
                                    outputString = outputString.Replace(entry.Key, entry.Value);
                                if (StopAfterFirstMatch) break;
                            }
                        }
                        break;

                    case Function.DateToUnixTime:
                        switch (UnixTimeType)
                        {
                            case DateToUnixTimeType.Seconds:
                                if (!string.IsNullOrEmpty(localInputString))
                                    outputString = localInputString.ToDateTime(DateFormat).ToUnixTimeSeconds().ToString();
                                else
                                    outputString = DateTime.Now.ToUnixTimeSeconds().ToString();
                                break;
                            case DateToUnixTimeType.Miliseconds:
                                if (!string.IsNullOrEmpty(localInputString))
                                    outputString = localInputString.ToDateTime(DateFormat).ToUnixTimeMilliseconds().ToString();
                                else
                                    outputString = DateTime.Now.ToUnixTimeMilliseconds().ToString();
                                break;
                        }
                        break;

                    case Function.DateToSolar:
                        {
                            var persian = new PersianCalendar();
                            if (DateTime.TryParse(localInputString, out DateTime date))
                            {
                                outputString = string.Format("{0}/{1}/{2}", persian.GetYear(date),
                                     persian.GetMonth(date),
                                     persian.GetDayOfMonth(date));
                            }
                        }
                        break;

                    case Function.DateToGregorian:
                        {
                            PersianCalendar pc = new PersianCalendar();

                            var persianDate = Convert.ToDateTime(localInputString);
                            DateTime dateTime = new DateTime(persianDate.Year, persianDate.Month, persianDate.Day, pc);
                            outputString = DateTime.Parse(dateTime.ToString(CultureInfo.CreateSpecificCulture("en-US")))
                                                            .ToShortDateString();
                        }
                        break;

                    case Function.Length:
                        outputString = localInputString.Length.ToString();
                        break;

                    case Function.ToLowercase:
                        outputString = localInputString.ToLower();
                        break;

                    case Function.ToUppercase:
                        outputString = localInputString.ToUpper();
                        break;

                    case Function.Replace:
                        if (useRegex)
                            outputString = Regex.Replace(localInputString, ReplaceValues(replaceWhat, data), ReplaceValues(replaceWith, data));
                        else
                            outputString = localInputString.Replace(ReplaceValues(replaceWhat, data), ReplaceValues(replaceWith, data));
                        break;

                    case Function.RegexMatch:
                        outputString = Regex.Match(localInputString, ReplaceValues(regexMatch, data)).Value;
                        break;

                    case Function.Unescape:
                        outputString = Regex.Unescape(localInputString);
                        break;

                    case Function.URLEncode:
                        // The maximum allowed Uri size is 2083 characters, we use 2080 as a precaution
                        outputString = string.Join("", SplitInChunks(localInputString, 2080).Select(s => Uri.EscapeDataString(s)));
                        break;

                    case Function.URLDecode:
                        outputString = Uri.UnescapeDataString(localInputString);
                        break;

                    case Function.UnixTimeToDate:
                        outputString = double.Parse(localInputString).ToDateTime().ToString(dateFormat);
                        break;

                    case Function.CurrentDate:
                        outputString = DateTime.Now.ToShortDateString();
                        break;

                    case Function.CurrentDay:
                        outputString = DateTime.Now.Day.ToString();
                        break;

                    case Function.CurrentMonth:
                        outputString = DateTime.Now.Month.ToString();
                        break;

                    case Function.CurrentYear:
                        outputString = DateTime.Now.Year.ToString();
                        break;

                    case Function.GetRemainingDay:
                        {
                            var date = Convert.ToDateTime(localInputString, new CultureInfo("en-US"));
                            var dateNow = DateTime.Now;
                            outputString = (date - dateNow).Days.ToString();
                        }
                        break;

                    case Function.CurrentTime:
                        outputString = DateTime.Now.ToShortTimeString();
                        break;

                    case Function.DayOfWeek:
                        outputString = DateTime.Now.DayOfWeek.ToString();
                        break;

                    case Function.CurrentUnixTime:
                        outputString = DateTime.UtcNow.ToUnixTimeSeconds().ToString();
                        break;

                    case Function.UnixTimeToISO8601:
                        outputString = double.Parse(localInputString).ToDateTime().ToISO8601();
                        break;

                    case Function.RandomNum:
                        {
                            long LongRandom(long min, long max, Random rand)
                            {
                                long result = rand.Next((int)(min >> 32), (int)(max >> 32));
                                result = result << 32;
                                result = result | (Int64)rand.Next((int)min, (int)max);
                                return result;
                            }
                            var myMin = long.Parse(ReplaceValues(randomMin, data));
                            var myMax = long.Parse(ReplaceValues(randomMax, data));
                            var randomNumString = LongRandom(myMin, myMax, new Random()).ToString();
                            outputString = randomZeroPad ? randomNumString.PadLeft(myMax.ToString().Length, '0') : randomNumString;
                        }
                        break;

                    case Function.RandomString:
                        outputString = localInputString;
                        outputString = Regex.Replace(outputString, @"\?l", m => _lowercase[data.random.Next(_lowercase.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?u", m => _uppercase[data.random.Next(_uppercase.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?d", m => _digits[data.random.Next(_digits.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?s", m => _symbols[data.random.Next(_symbols.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?h", m => _hex[data.random.Next(_hex.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?a", m => _allChars[data.random.Next(_allChars.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?m", m => _udChars[data.random.Next(_udChars.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?n", m => _ldChars[data.random.Next(_ldChars.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?i", m => _ludChars[data.random.Next(_ludChars.Length)].ToString());
                        outputString = Regex.Replace(outputString, @"\?f", m => _upperlwr[data.random.Next(_upperlwr.Length)].ToString());
                        break;

                    case Function.Ceil:
                        outputString = Math.Ceiling(Decimal.Parse(localInputString, _style, _provider)).ToString();
                        break;

                    case Function.Floor:
                        outputString = Math.Floor(Decimal.Parse(localInputString, _style, _provider)).ToString();
                        break;

                    case Function.Round:
                        outputString = Math.Round(Decimal.Parse(localInputString, _style, _provider), 0, MidpointRounding.AwayFromZero).ToString();
                        break;

                    case Function.Abs:
                        outputString = Math.Abs(Decimal.Parse(localInputString, _style, _provider)).ToString();
                        break;

                    case Function.Compute:
                        outputString = new DataTable().Compute(localInputString.Replace(',', '.'), null).ToString();
                        break;

                    case Function.CountOccurrences:
                        outputString = CountStringOccurrences(localInputString, stringToFind).ToString();
                        break;

                    case Function.ClearCookies:
                        data.Cookies.Clear();
                        break;

                    case Function.RSAEncrypt:
                        outputString = Crypto.RSAEncrypt(
                            localInputString,
                            ReplaceValues(RsaN, data),
                            ReplaceValues(RsaE, data),
                            RsaOAEP
                            );
                        break;

                    /*
                case Function.RSADecrypt:
                    outputString = Crypto.RSADecrypt(
                        localInputString,
                        ReplaceValues(RsaN, data),
                        ReplaceValues(RsaD, data),
                        RsaOAEP
                        );
                    break;
                    */

                    case Function.RSAPKCS1PAD2:
                        outputString = Crypto.RSAPkcs1Pad2(
                            localInputString,
                            ReplaceValues(RsaN, data),
                            ReplaceValues(RsaE, data)
                            );
                        break;

                    case Function.Delay:
                        try { Thread.Sleep(int.Parse(localInputString)); } catch { }
                        break;

                    case Function.CharAt:
                        outputString = localInputString.ToCharArray()[int.Parse(ReplaceValues(charIndex, data))].ToString();
                        break;

                    case Function.Split:
                        outputString = localInputString.Split(new[] { ReplaceValues(Separator, data) }, StringSplitOption)[int.Parse(ReplaceValues(SplitIndex.ToString(), data)) - 1];
                        break;

                    case Function.Remove:
                        if (string.IsNullOrEmpty(RemoveCount)) outputString = localInputString.Remove(int.Parse(ReplaceValues(removeSIndex, data)));
                        else outputString = localInputString.Remove(int.Parse(ReplaceValues(RemoveSIndex, data)), int.Parse(ReplaceValues(RemoveCount, data)));
                        break;

                    case Function.Substring:
                        outputString = localInputString.Substring(int.Parse(ReplaceValues(substringIndex, data)), int.Parse(ReplaceValues(substringLength, data)));
                        break;

                    case Function.ReverseString:
                        char[] charArray = localInputString.ToCharArray();
                        Array.Reverse(charArray);
                        outputString = new string(charArray);
                        break;

                    case Function.Trim:
                        outputString = localInputString.Trim();
                        break;

                    case Function.GetRandomUA:
                        if (!string.IsNullOrEmpty(localInputString))
                        {
                            outputString = UserAgent.RandomFromList(localInputString);
                        }
                        else if (UserAgentSpecifyBrowser)
                        {
                            outputString = UserAgent.ForBrowser(UserAgentBrowser);
                        }
                        else
                        {
                            outputString = UserAgent.Random(data.random);
                        }
                        break;

                    case Function.AESEncrypt:
                        outputString = Crypto.AESEncrypt(localInputString, ReplaceValues(aesKey, data), ReplaceValues(aesIV, data), AesMode, AesPadding, HexKeys);
                        break;

                    case Function.AESDecrypt:
                        outputString = Crypto.AESDecrypt(localInputString, ReplaceValues(aesKey, data), ReplaceValues(aesIV, data), AesMode, AesPadding);
                        break;

                    case Function.PBKDF2PKCS5:
                        outputString = Crypto.PBKDF2PKCS5(localInputString, ReplaceValues(KdfSalt, data), KdfSaltSize, KdfIterations, KdfKeySize, KdfAlgorithm);
                        break;

                    case Function.ToLetter:
                        outputString = new string(localInputString.Where(Char.IsLetter).ToArray());
                        break;

                    case Function.ToDigit:
                        outputString = new string(localInputString.Where(Char.IsDigit).ToArray());
                        break;

                    case Function.ToLetterOrDigit:
                        outputString = new string(localInputString.Where(Char.IsLetterOrDigit).ToArray());
                        break;

                    case Function.EvaluateMathString:
                        {
                            var objResult = new CodeDomCalculator(localInputString)
                                .Calculate();
                            outputString = objResult.ToString();
                        }
                        break;

                    case Function.NumberToWords:
                        {
                            if (int.TryParse(localInputString, out int someNumber))
                            {
                                outputString = someNumber.ToWords(new CultureInfo("en-US"));
                            }
                        }
                        break;

                    case Function.WordsToNumber:
                        outputString = WordToNumber.ToLong(localInputString)
                            .ToString();
                        break;

                    case Function.GenerateOAuthVerifier:
                        byte[] number = new byte[32];
                        RandomNumberGenerator rng = RandomNumberGenerator.Create();
                        rng.GetBytes(number);
                        var bytes = BitConverter.ToString(number);
                        string encodedStr = Base64UrlEncoder.Encode(bytes);
                        outputString = encodedStr;
                        break;

                    case Function.GenerateOAuthChallenge:
                        var HashedVerifier = localInputString = GetHash(localInputString, Hash.SHA256, false).ToLower();
                        var encodedHash = Base64UrlEncoder.Encode(HashedVerifier);
                        outputString = encodedHash;
                        break;

                    case Function.GenerateGUID:
                        outputString = Guid.NewGuid().ToString();
                        break;

                    case Function.GenerateBytes:
                        int z = 0;
                        try
                        {
                            z = Convert.ToInt32(localInputString);
                            byte[] genbyte = new byte[z];
                            RandomNumberGenerator rando = RandomNumberGenerator.Create();
                            rando.GetBytes(genbyte);
                            var end = BitConverter.ToString(genbyte);
                            outputString = end.ToString();
                        }
                        catch (FormatException ex)
                        {
                            data.Status = BotStatus.ERROR;
                            data.LogBuffer.Add(new LogEntry("ERROR: " + ex.Message, Colors.Tomato));
                            outputString = "INTEGERS ONLY";
                        }
                        catch (OverflowException ex)
                        {
                            data.Status = BotStatus.ERROR;
                            data.LogBuffer.Add(new LogEntry("ERROR: " + ex.Message, Colors.Tomato));
                            outputString = "BYTE SIZE TOO LARGE FOR 32BIT INTEGER";
                        }
                        break;

                    case Function.Encoding:
                        switch (EncFunc)
                        {
                            case EncodingMethods.GetBytes:
                                if (int.TryParse(ReplaceValues(GetEncoding.ToString(), data), out int bCodePage))
                                {
                                    outputString = Encoding.GetEncoding(bCodePage).GetBytes(ReplaceValues(localInputString, data)).ConvertToString();
                                }
                                else
                                    outputString = Encoding.GetEncoding(ReplaceValues(GetEncoding.ToString(), data)).GetBytes(ReplaceValues(localInputString, data)).ConvertToString();
                                break;
                            case EncodingMethods.GetString:
                                if (int.TryParse(ReplaceValues(GetEncoding.ToString(), data), out int sCodePage))
                                {
                                    outputString = Encoding.GetEncoding(sCodePage).GetString(ReplaceValues(localInputString, data).ConvertToByteArray());
                                }
                                else
                                    outputString = Encoding.GetEncoding(GetEncoding.ToString()).GetString(ReplaceValues(localInputString, data).ConvertToByteArray());
                                break;
                        }
                        break;

                    case Function.Ntlm:
                        outputString = Ntlm.Generate(ReplaceValues(localInputString, data));
                        break;

                    case Function.SCrypt:
                        switch (scryptMeth)
                        {
                            case ScryptMethods.Encode:
                                outputString = Crypto.ScryptEncoder(ReplaceValues(localInputString, data), ScryptSalt, ScryptCost, ScryptBlockSize, 1, ScryptOutputLength, Base64Output);
                                break;
                            case ScryptMethods.Compare:
                                outputString = Crypto.ScryptCompare(ReplaceValues(localInputString, data), ReplaceValues(ScryptHashedPassword, data))
                                    .ToString();
                                break;
                            case ScryptMethods.IsValid:
                                outputString = Crypto.ScryptIsValid(ReplaceValues(localInputString, data))
                                    .ToString();
                                break;
                            default:
                                break;
                        }
                        break;

                    case Function.BCrypt:
                        switch (BCryptMeth)
                        {
                            case BCryptMethods.Encode:
                                if (UseWorkFactor) outputString = Crypto.BcryptEncoder(ReplaceValues(localInputString, data), BCryptWorkFactor, BCryptSalt);
                                else outputString = Crypto.BcryptEncoder(ReplaceValues(localInputString, data), null, BCryptSalt);
                                break;
                            case BCryptMethods.GenerateSalt:
                                if (UseWorkFactor)
                                    outputString = Crypto.BcryptGenerateSalt(BCryptWorkFactor);
                                else outputString = Crypto.BcryptGenerateSalt(null);
                                break;
                            case BCryptMethods.Verify:
                                outputString = Crypto.BcryptVerify(ReplaceValues(localInputString, data), ReplaceValues(BCryptHashedPassword, data))
                                    .ToString();
                                break;
                            default:
                                break;
                        }

                        break;

                }

                data.Log(new LogEntry(string.Format("Executed function {0} on input {1} with outcome {2}", functionType, localInputString, outputString), Colors.GreenYellow));

                // Add to the outputs
                outputs.Add(outputString);
            }

            var isList = outputs.Count > 1 || InputString.Contains("[*]") || InputString.Contains("(*)") || InputString.Contains("{*}");
            InsertVariable(data, isCapture, isList, outputs, variableName, "", "", false, true);
        }

        /// <summary>
        /// Hashes a string using the specified hashing function.
        /// </summary>
        /// <param name="baseString">The string to hash</param>
        /// <param name="type">The hashing function</param>
        /// <param name="inputBase64">Whether the base string should be treated as base64 encoded (if false, it will be treated as UTF8 encoded)</param>
        /// <returns>The hash digest as a hex-encoded uppercase string.</returns>
        public static string GetHash(string baseString, Hash type, bool inputBase64)
        {
            var rawInput = inputBase64 ? Convert.FromBase64String(baseString) : Encoding.UTF8.GetBytes(baseString);
            byte[] digest;

            switch (type)
            {
                case Hash.MD2:
                    digest = Crypto.MD2(rawInput);
                    break;

                case Hash.MD4:
                    digest = Crypto.MD4(rawInput);
                    break;

                case Hash.MD5:
                    digest = Crypto.MD5(rawInput);
                    break;

                case Hash.SHA1:
                    digest = Crypto.SHA1(rawInput);
                    break;

                case Hash.SHA256:
                    digest = Crypto.SHA256(rawInput);
                    break;

                case Hash.SHA384:
                    digest = Crypto.SHA384(rawInput);
                    break;

                case Hash.SHA512:
                    digest = Crypto.SHA512(rawInput);
                    break;

                case Hash.SHA3_224:
                    digest = Crypto.SHA3_224(rawInput);
                    break;

                case Hash.SHA3_256:
                    digest = Crypto.SHA3_256(rawInput);
                    break;

                case Hash.SHA3_384:
                    digest = Crypto.SHA3_384(rawInput);
                    break;

                case Hash.SHA3_512:
                    digest = Crypto.SHA3_512(rawInput);
                    break;

                default:
                    throw new NotSupportedException("Unsupported algorithm");
            }

            return digest.ToHex();
        }

        /// <summary>
        /// Gets the HMAC signature of a message given a key and a hashing function.
        /// </summary>
        /// <param name="baseString">The message to sign</param>
        /// <param name="type">The hashing function</param>
        /// <param name="key">The HMAC key</param>
        /// <param name="inputBase64">Whether the input string should be treated as base64 encoded (if false, it will be treated as UTF8 encoded)</param>
        /// <param name="keyBase64">Whether the key string should be treated as base64 encoded (if false, it will be treated as UTF8 encoded)</param>
        /// <param name="outputBase64">Whether the output should be encrypted as a base64 string</param>
        /// <returns>The HMAC signature</returns>
        public static string Hmac(string baseString, Hash type, string key, bool inputBase64, bool keyBase64, bool outputBase64)
        {
            byte[] rawInput = inputBase64 ? Convert.FromBase64String(baseString) : Encoding.UTF8.GetBytes(baseString);
            byte[] rawKey = keyBase64 ? Convert.FromBase64String(key) : Encoding.UTF8.GetBytes(key);
            byte[] signature;

            switch (type)
            {
                case Hash.MD5:
                    signature = Crypto.HMACMD5(rawInput, rawKey);
                    break;

                case Hash.SHA1:
                    signature = Crypto.HMACSHA1(rawInput, rawKey);
                    break;

                case Hash.SHA256:
                    signature = Crypto.HMACSHA256(rawInput, rawKey);
                    break;

                case Hash.SHA384:
                    signature = Crypto.HMACSHA384(rawInput, rawKey);
                    break;

                case Hash.SHA512:
                    signature = Crypto.HMACSHA512(rawInput, rawKey);
                    break;

                default:
                    throw new NotSupportedException("Unsupported algorithm");
            }

            return outputBase64 ? Convert.ToBase64String(signature) : signature.ToHex();
        }

        #region Translation

        /// <summary>
        /// Builds a string containing translation keys.
        /// </summary>
        /// <returns>One translation key per line, with name and value separated by a colon</returns>
        public string GetDictionary()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pair in TranslationDictionary)
            {
                sb.Append($"{pair.Key}: {pair.Value}");
                if (!pair.Equals(TranslationDictionary.Last())) sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Sets translation keys from an array of lines.
        /// </summary>
        /// <param name="lines">The lines containing the colon-separated name and value of the translation keys</param>
        public void SetDictionary(string[] lines)
        {
            TranslationDictionary.Clear();
            foreach (var line in lines)
            {
                if (line.Contains(':'))
                {
                    var split = line.Split(new[] { ':' }, 2);
                    var key = split[0];
                    var val = split[1].TrimStart();
                    TranslationDictionary[key] = val;
                }
            }
        }
        #endregion

        #region Count Occurrences
        /// <summary>
        /// Counts how many times a string occurs inside another string.
        /// </summary>
        /// <param name="input">The long string</param>
        /// <param name="text">The text to search</param>
        /// <returns>How many times the text appears in the long string</returns>
        public static int CountStringOccurrences(string input, string text)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = input.IndexOf(text, i)) != -1)
            {
                i += text.Length;
                count++;
            }
            return count;
        }
        #endregion

        #region Others
        /// <summary>
        /// Splits a string in chunks of a given size.
        /// </summary>
        /// <param name="str">The string to split</param>
        /// <param name="chunkSize">The maximum chunk size</param>
        /// <returns>An array of strings where the last one might be shorter than the maximum chunk size.</returns>
        public static string[] SplitInChunks(string str, int chunkSize)
        {
            if (str.Length < chunkSize) return new string[] { str };
            return Enumerable.Range(0, (int)Math.Ceiling((double)str.Length / (double)chunkSize))
                .Select(i => str.Substring(i * chunkSize, Math.Min(str.Length - i * chunkSize, chunkSize)))
                .ToArray();
        }
        #endregion
    }
}
