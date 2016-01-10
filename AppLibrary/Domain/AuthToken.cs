using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Web;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;

using MS.Utility;
using MS.WebUtility;

namespace App.Library
{
#if false
    public struct AuthCode
    {
        internal enum StringFormat
        {
            Storage,
            Display,
            Url,
        }

        // 0-9 literal number
        // a-z, A-Z literal letter
        // ^^^-### would be the template for ABC-123
        private const char anyLetterLowerOrDigit = '&';
        private const char anyLetterUpperOrDigit = '%';
        private const char anyDigit = '#';
        private const char anyLetterLower = '@';
        private const char anyLetterUpper = '^';
        private static readonly char[] allPlaceholders = { anyLetterLowerOrDigit, anyLetterUpperOrDigit, anyDigit, anyLetterLower, anyLetterUpper };

        public static AuthCode Invalid = new AuthCode(string.Empty, null);

        private static object cryptoProviderSyncRoot = new object();
        private static RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider();
        private static byte[] cryptoBuffer = new byte[4];

        // http://www.crockford.com/wrmg/base32.html
        private static readonly char[] base32EncodeSymbolUpper = 
        {
            '0','1','2','3','4','5','6','7',
            '8','9','A','B','C','D','E','F',
            'G','H','J','K','M','N','P','Q',
            'R','S','T','V','W','X','Y','Z'
        };

        // http://www.crockford.com/wrmg/base32.html
        private static readonly char[] base32EncodeSymbolLower = 
        {
            '0','1','2','3','4','5','6','7',
            '8','9','a','b','c','d','e','f',
            'g','h','j','k','m','n','p','q',
            'r','s','t','v','w','x','y','z'
        };

        private static readonly char[] validDigitsAndLetters =
        {
            '0','1','2','3','4','5','6','7','8','9',
            'a','b','c','d','e','f','g','h','j','k','m','n','p','q','r','s','t','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','J','K','M','N','P','Q','R','S','T','V','W','X','Y','Z'
        };

        private static char[] digits = 
        {
            '0','1','2','3','4','5','6','7','8','9'
        };

        private string authCodeString;
        private AuthTokenType authTokenType;

        private AuthCode(string authCodeString, AuthTokenType authTokenType)
        {
            // (We expect at least string.Empty for an invalid AuthCode)
            Debug.Assert(authCodeString != null);
            Debug.Assert(authCodeString == string.Empty || authTokenType != null);

            this.authCodeString = authCodeString;
            this.authTokenType = authTokenType;
        }

        internal static AuthCode FromStorage(string authCodeString, AuthTokenType authTokenType)
        {
            return new AuthCode(authCodeString, authTokenType);
        }

        public static AuthCode Parse(string authCodeString, AuthTokenType authTokenType)
        {
            Debug.Assert(!string.IsNullOrEmpty(authCodeString));
            if (string.IsNullOrEmpty(authCodeString))
            {
                return AuthCode.Invalid;
            }

            StringBuilder sb = new StringBuilder(authCodeString.Length);

            foreach (char c in authCodeString)
            {
                if (c == 'l' || c == 'L' || c == 'i' || c == 'I')
                {
                    sb.Append('1');
                }
                else if (c == 'o' || c == 'O')
                {
                    sb.Append('0');
                }
                else
                {
                    if (c == 'u')
                    {
                        sb.Append('v');
                    }
                    else if (c == 'U')
                    {
                        sb.Append('V');
                    }
                    else if (authTokenType.RequireAuthCodeFormatting || char.IsLetterOrDigit(c))
                    {
                        sb.Append(c);
                    }
                }
            }

            if (sb.Length == 0)
            {
                return AuthCode.Invalid;
            }

            return new AuthCode(sb.ToString(), authTokenType);
        }

        public static AuthCode Create(AuthTokenType authTokenType)
        {
            Debug.Assert(authTokenType != null);

            string authCodePattern = authTokenType.AuthCodePattern;
            Debug.Assert(!string.IsNullOrEmpty(authCodePattern));

            // The AuthCode value we create should only include formatting characters if we require them
            if (authTokenType.AuthCodePatternContainsFormatting && !authTokenType.RequireAuthCodeFormatting)
            {
                var authCodePatternNonFormattingChars = authCodePattern
                    .ToCharArray()
                    .Where(c => !IsFormatCharacter(c))
                    .ToArray();

                authCodePattern = new string(authCodePatternNonFormattingChars);
            }

            int placeholders = authCodePattern.ToCharArray().Count(c => allPlaceholders.Contains(c));
            Debug.Assert(placeholders > 0);

            StringBuilder authCodeStringBuilder = new StringBuilder(authCodePattern);

            int placeholderIndex = authCodePattern.IndexOfAny(allPlaceholders);
            while (placeholderIndex >= 0)
            {
                char placeholder = authCodePattern[placeholderIndex];
                authCodeStringBuilder[placeholderIndex] = GetRandomChar(placeholder);
                placeholderIndex = authCodePattern.IndexOfAny(allPlaceholders, placeholderIndex + 1);
            }

            return new AuthCode(authCodeStringBuilder.ToString(), authTokenType);
        }

        internal static bool IsFormatCharacter(char c)
        {
            if (AuthCode.validDigitsAndLetters.Contains(c) || AuthCode.allPlaceholders.Contains(c))
            {
                return false;
            }

            if (char.IsLetterOrDigit(c))
            {
                // We disallow 'i', 'l', 'o' and 'u' in our patterns
                throw new ArgumentOutOfRangeException("Invalid pattern character");
            }

            return true;
        }

        private static char GetRandomChar(char placeholder)
        {
            uint randomNumber;
            lock (cryptoProviderSyncRoot)
            {
                // Fill the array with a random value.
                cryptoProvider.GetBytes(cryptoBuffer);
                randomNumber = BitConverter.ToUInt32(cryptoBuffer, 0);
            }

            switch (placeholder)
            {
                case AuthCode.anyLetterLowerOrDigit:
                    // we allow 32 unique characters
                    return base32EncodeSymbolLower[randomNumber % 32];
                case AuthCode.anyLetterUpperOrDigit:
                    // we allow 32 unique characters
                    return base32EncodeSymbolUpper[randomNumber % 32];
                case anyDigit:
                    return digits[randomNumber % 10];
                case anyLetterLower:
                    // we allow 22 unique letters
                    return base32EncodeSymbolLower[10 + (randomNumber % 22)];
                case anyLetterUpper:
                    // we allow 22 unique letters
                    return base32EncodeSymbolUpper[10 + (randomNumber % 22)];
                default:
                    Debug.Fail("Unexpected placeholder: " + placeholder);
                    return '.';
            }
        }

        internal string ToString(StringFormat format)
        {
            Debug.Assert(this.authCodeString != null);

            if (format == StringFormat.Storage || !this.authTokenType.AuthCodePatternContainsFormatting || this.authTokenType.RequireAuthCodeFormatting)
            {
                return this.authCodeString;
            }

            switch (format)
            {
                case StringFormat.Display:
                case StringFormat.Url:

                    // Expand out this.authCodeString to include any formatting
                    string formatPattern = this.authTokenType.AuthCodePattern;
                    StringBuilder sb = new StringBuilder(formatPattern.Length);
                    var authCodeStringEnumerator = this.authCodeString.GetEnumerator();
                    foreach (char c in formatPattern)
                    {
                        if (!AuthCode.IsFormatCharacter(c))
                        {
                            // replace placeholders with the actual code
                            authCodeStringEnumerator.MoveNext();
                            char codeValue = authCodeStringEnumerator.Current;
                            // If the pattern contains a literal code value (not a placeholder), then we should match that value in our code
                            Debug.Assert(AuthCode.allPlaceholders.Contains(c) || c == codeValue);
                            sb.Append(codeValue);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }

                    var formattedAuthCodeString = sb.ToString();
                    if (format == StringFormat.Url)
                    {
                        return HttpUtility.UrlPathEncode(formattedAuthCodeString);
                    }
                    return formattedAuthCodeString;

                case StringFormat.Storage:
                default:
                    Debug.Fail("Unexpected format: " + format);
                    return this.authCodeString;
            }
        }

        public override string ToString()
        {
            return this.ToString(StringFormat.Display);
        }
    }
#endif

#if false
    public class AuthTokenType
    {
        private static AuthTokenType[] registeredAuthTokenTypes =
            {    
                AuthTokenType.Create(typeof(LoginAuthToken), "/login/", "@@@##-&&&&-32#"),
                AuthTokenType.Create(typeof(PasswordResetAuthToken), "/pw/", "@##-&&&&-10#"),
                AuthTokenType.Create(typeof(ProjectAuthToken), "/proj/", "&&&&&&&&"),
                AuthTokenType.Create(typeof(ReportAuthToken), "/report/", "&26&&-&&&&-&&", true),
                // (Note, we can have multiple entries with different paths...)
                AuthTokenType.Create(typeof(CreateProjectAuthToken), "/getstarted/", "^^^-###", "/getstarted/default.aspx"),
                AuthTokenType.Create(typeof(CreateProjectAuthToken), "/", "^^^-###", "/getstarted/default.aspx"),
                AuthTokenType.Create(typeof(CreateUserAuthToken), "/invite/", "&23&&-&&&&-&&", true),

                AuthTokenType.Create(typeof(CreateUserSsoAuthorized), "CreateUserSsoAuth"),
                AuthTokenType.Create(typeof(CreateUserSsoRequested), "CreateUserSsoReq"),
            };

        private Type authTokenTypeType;
        internal string AuthTokenTypeString { get; private set; }
        internal string UrlPatternPrefix { get; private set; }
        internal string AuthCodePattern { get; private set; }
        internal bool AuthCodePatternContainsFormatting { get; private set; }
        internal bool RequireAuthCodeFormatting { get; private set; }
        internal string TokenNotFoundPage { get; private set; }

        static AuthTokenType()
        {
            Debug.Assert(registeredAuthTokenTypes.All(authTokenType => authTokenType != null));
        }

        private AuthTokenType(Type authTokenTypeType, string authTokenTypeString)
        {
            Debug.Assert(authTokenTypeType != null);

            this.authTokenTypeType = authTokenTypeType;
            this.AuthTokenTypeString = authTokenTypeString;
        }

        private AuthTokenType(Type authTokenTypeType, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage)
        {
            Debug.Assert(authTokenTypeType != null);
            Debug.Assert(authTokenTypeType.Name.EndsWith("AuthToken"));

            this.authTokenTypeType = authTokenTypeType;

            this.AuthTokenTypeString = authTokenTypeType.Name.TrimEnd("AuthToken");
            this.UrlPatternPrefix = urlPatternPrefix;
            this.AuthCodePattern = authCodePattern;
            this.AuthCodePatternContainsFormatting = authCodePattern.ToCharArray().Any(c => AuthCode.IsFormatCharacter(c));
            this.RequireAuthCodeFormatting = requireAuthCodeFormatting;
            this.TokenNotFoundPage = tokenNotFoundPage;
        }

        private static AuthTokenType Create(Type authCodeTypeType, string authTokenTypeString)
        {
            return new AuthTokenType(authCodeTypeType, authTokenTypeString);
        }

        public static AuthTokenType Create(Type authCodeTypeType, string urlPatternPrefix, string authCodePattern)
        {
            return Create(authCodeTypeType, urlPatternPrefix, authCodePattern, false, null);
        }

        public static AuthTokenType Create(Type authCodeTypeType, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting)
        {
            return Create(authCodeTypeType, urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, null);
        }

        public static AuthTokenType Create(Type authCodeTypeType, string urlPatternPrefix, string authCodePattern, string tokenNotFoundPage)
        {
            return Create(authCodeTypeType, urlPatternPrefix, authCodePattern, false, tokenNotFoundPage);
        }

        public static AuthTokenType Create(Type authCodeTypeType, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage)
        {
            Debug.Assert(authCodeTypeType != null);
            Debug.Assert(!string.IsNullOrEmpty(authCodePattern));

            // We can only ignore AuthCode formatting if the pattern actually has some formatting in it
            if (!authCodePattern.ToCharArray().Any(c => AuthCode.IsFormatCharacter(c)))
            {
                // no formatting to ignore
                Debug.Assert(!requireAuthCodeFormatting);
                requireAuthCodeFormatting = false;
            }

            return new AuthTokenType(authCodeTypeType, urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage);
        }

        public static AuthCode ParseAuthCode<T>(string authCodeString) where T : AuthToken
        {
            var authTokenType = AuthTokenType.Find(typeof(T));
            var authCode = authTokenType.ParseAuthCode(authCodeString);
            return authCode;
        }

        private AuthCode ParseAuthCode(string authCodeString)
        {
            return AuthCode.Parse(authCodeString, this);
        }

        public static AuthTokenType Find(Type authTokenTypeType)
        {
            var authTokenType = registeredAuthTokenTypes
                .FirstOrDefault(type => type.authTokenTypeType == authTokenTypeType);

            Debug.Assert(authTokenType != null);

            return authTokenType;
        }

        public static AuthTokenType Find(string authTokenTypeString)
        {
            var authTokenType = registeredAuthTokenTypes
                .FirstOrDefault(type => type.AuthTokenTypeString == authTokenTypeString);

            return authTokenType;
        }

        public static void ProcessRequest(HttpContext context)
        {
            string path = context.Request.Path;
            Debug.Assert(path.StartsWith("/"));

            foreach (AuthTokenType authTokenType in registeredAuthTokenTypes.Where(type => !string.IsNullOrEmpty(type.UrlPatternPrefix)))
            {
                Debug.Assert(authTokenType.UrlPatternPrefix.StartsWith("/"));

                if (path.StartsWith(authTokenType.UrlPatternPrefix))
                {
                    string pathSubString = path.TrimStart(authTokenType.UrlPatternPrefix);
                    Debug.Assert(!pathSubString.StartsWith("/"));

                    int secondSlashIndex = pathSubString.IndexOf('/', 0);
                    bool singlePathSegment = secondSlashIndex == -1 || secondSlashIndex == (pathSubString.Length - 1);

                    if (singlePathSegment && pathSubString.Length > 2 && !pathSubString.Contains('.'))
                    {
                        int slashCount = secondSlashIndex == -1 ? 0 : 1;
                        string authCodeString = pathSubString.Substring(0, pathSubString.Length - slashCount);
                        AuthCode authCode = authTokenType.ParseAuthCode(authCodeString);
                        AuthToken authToken = AuthToken.FirstOrDefault(authCode);
                        if (authToken != null)
                        {
                            authToken.Click(context);
                        }
                        else
                        {
                            // Avoid handling the "not found" case at the root - since some of our regular URL paths look like auth codes
                            //!! Consider beefing up ParseAuthCode to see if authCodeString matches the token pattern. If it does match the pattern,
                            //   we could consider allowing this case at the root again.
                            if (authTokenType.UrlPatternPrefix != "/")
                            {
                                authTokenType.OnTokenNotFound(context, authCodeString);
                            }
                        }
                        return;
                    }
                }
            }
        }

        protected void OnTokenNotFound(HttpContext context, string authCodeString)
        {
            //!! track the click in some click usage table

            if (!string.IsNullOrEmpty(this.TokenNotFoundPage))
            {
                string queryString = string.Format("authCode={0}",
                    /*0*/ authCodeString);
                context.RewritePath(this.TokenNotFoundPage, string.Empty, queryString);
            }
        }
    }

    public partial class AuthToken
    {
        protected static readonly int? NullProjectID = null;
        protected static readonly string NullSettings = null;
        public static readonly TimeSpan? InfiniteDuration = null;

        protected AuthToken(AuthCode authCode, int clientID, int? userID, int? projectID, string settingsData, TimeSpan? validDuration)
            : this()
        {
            this.AuthCodeString = authCode.ToString(AuthCode.StringFormat.Storage);
            this.AuthClientID = clientID;
            this.AuthUserID = userID;
            this.AuthProjectID = projectID;
            this.Settings = settingsData;

            DateTime utcNow = DateTime.UtcNow;
            this.CreateDate = utcNow;
            if (validDuration != null)
            {
                this.ExpireDate = this.CreateDate + validDuration;
            }
        }

        public TimeSpan? TotalDuration
        {
            get
            {
                if (this.ExpireDate != null)
                {
                    return this.ExpireDate - this.CreateDate;
                }
                return null;
            }
        }

        public AuthCode AuthCode
        {
            get
            {
                var authTokenType = AuthTokenType.Find(this.Type);
                return AuthCode.FromStorage(this.AuthCodeString, authTokenType);
            }
        }

        protected Uri ToUrl(string urlPatternPrefix)
        {
            return ToUrl(urlPatternPrefix, false);
        }

        protected Uri ToSecureUrl(string urlPatternPrefix)
        {
            return ToUrl(urlPatternPrefix, true);
        }

        private Uri ToUrl(string urlPatternPrefix, bool useSsl)
        {
            TorqContext torq = TorqContext.Current;

            var urlAuthCodeString = this.AuthCode.ToString(AuthCode.StringFormat.Url);

            UriBuilder uriBuilder = new UriBuilder(torq.SiteUrl);
            uriBuilder.Path = urlPatternPrefix + urlAuthCodeString;
            if (useSsl)
            {
                uriBuilder.Scheme = "https";
                uriBuilder.Port = -1;
            }

            return uriBuilder.Uri;
        }

        internal static AuthToken FirstOrDefault(AuthCode authCode)
        {
            try
            {
                string authCodeString = authCode.ToString(AuthCode.StringFormat.Storage);
                string authCodeStringLower = authCodeString.ToLower();
                string authCodeStringUpper = authCodeString.ToUpper();
                //!! hmm. I can't decide how best to handle casing for our tokens.
                // So the comparison here looks for either exact match or uppper or lower cases.
                // I'm doing the above because it's not clear if we can specify that SQL should perform a case insensative compare for us.
                // Also not clear if we'll ever be generating mixed case tokens, or if they'll always be entirely upper or lower.

                var matchedAuthToken = TorqDataContext.AccountsOnlyDCInstance.AuthTokens
                    // Not really sure if we need this OrderBy - does it speed up our search? Do we need an index on AuthCode?
                    .OrderByDescending(authToken => authToken.AuthTokenID)
                    .FirstOrDefault(authToken => authToken.AuthCodeString == authCodeString || authToken.AuthCodeString == authCodeStringUpper || authToken.AuthCodeString == authCodeStringLower);

                return matchedAuthToken;
            }
            catch (Exception e)
            {
                Diagnostic.TraceError(e);
            }

            return null;
        }

        public static T FirstOrDefault<T>(AuthCode authCode) where T : AuthToken
        {
            return FirstOrDefault(authCode) as T;
        }

        public virtual bool IsValid
        {
            get
            {
                DateTime utcNow = DateTime.UtcNow;
                return this.ExpireDate == null || (DateTime)this.ExpireDate >= utcNow;
            }
        }

        internal void Click(HttpContext context)
        {
            bool isValid = this.IsValid;
            OnClicked(context, IsValid);
        }

        protected virtual void OnClicked(HttpContext context, bool isValid)
        {
            Debug.Assert(this.AuthClientID.HasValue);

            TorqContext torq = TorqContext.Current;
            string siteName = torq.SiteName;

            var authTokenType = AuthTokenType.Find(this.Type);
            AuthTokenClickItem.LogClick(siteName, context, authTokenType, this.AuthTokenID, this.AuthClientID.Value, this.AuthUserID, isValid, this.AuthProjectID);
        }

        protected static IQueryable<T> FindValid<T>(int clientID) where T : AuthToken
        {
            //!! consider removing this method - and requiring an authorizedBy parameter

            //!! do we care that time marches on as we do this check?
            DateTime utcNow = DateTime.UtcNow;

            var authTokenQuery = Find<T>()
                .Where(authToken => authToken.ExpireDate == null || authToken.ExpireDate > utcNow)
                .Where(authToken => authToken.AuthClientID != null && authToken.AuthClientID == clientID);

            return authTokenQuery;
        }

        protected static IQueryable<T> Find<T>() where T : AuthToken
        {
            //!! consider removing this method - and requiring an authorizedBy parameter

            var authTokenType = AuthTokenType.Find(typeof(T));
            var authTokenTypeString = authTokenType.AuthTokenTypeString;

            var authTokenQuery = TorqDataContext.AccountsOnlyDCInstance.AuthTokens
                .OrderByDescending(authToken => authToken.AuthTokenID)
                .Where(authToken => authToken.Type == authTokenTypeString)
                .Cast<T>();

            return authTokenQuery;
        }

        protected static IQueryable<T> Find<T>(TorqIdentity authorizedBy) where T : AuthToken
        {
            Debug.Assert(authorizedBy != null);
            if (authorizedBy == null)
            {
                return new T[0].AsQueryable();
            }

            var authTokenType = AuthTokenType.Find(typeof(T));
            var authTokenTypeString = authTokenType.AuthTokenTypeString;

            var authTokenQuery = TorqDataContext.AccountsOnlyDCInstance.AuthTokens
                .OrderByDescending(authToken => authToken.AuthTokenID)
                .Where(authToken => authToken.Type == authTokenTypeString)
                .Cast<T>();

            if (authorizedBy.IsSystemAdmin)
            {
                // no restrictions
            }
            else if (authorizedBy.IsAccountAdmin)
            {
                // restrict to all clients w/in the account's hierarchy
                ClientInfo info = Client.GetCachedClientInfo(authorizedBy.ClientID);
                if (info != null && info.AncestorsAndSelf != null)
                {
                    List<int> clientIds = info.Root.DescendentsAndSelf.Select(c => c.ClientID).ToList();
                    authTokenQuery = authTokenQuery.Where(authToken => authToken.AuthClientID.HasValue && clientIds.Contains(authToken.AuthClientID.Value));
                }
            }
            else if (authorizedBy.IsGroupAdmin)
            {
                authTokenQuery = authTokenQuery
                    .Where(authToken => authToken.AuthClientID.HasValue && authToken.AuthClientID.Value == authorizedBy.ClientID);
            }
            else
            {
                return new T[0].AsQueryable();
            }

            return authTokenQuery;
        }

        /// <summary>
        /// Creates a new AuthToken, without consideration of any outstanding valid or expired AuthTokens
        /// </summary>
        protected static T CreateNew<T>(Func<AuthCode, T> authTokenConstructor) where T : AuthToken
        {
            var authTokenType = AuthTokenType.Find(typeof(T));
            Debug.Assert(authTokenType != null);

            while (true)
            {
                AuthCode authCode = AuthCode.Create(authTokenType);

                // Check we've got a unique AuthToken ...
                AuthToken existingToken = FirstOrDefault(authCode);
                if (existingToken == null)
                {
                    // ... Phew, it's unique.
                    T newAuthToken = authTokenConstructor(authCode);
                    Debug.Assert(newAuthToken != null);

                    // For LINQ purposes, we need to save the base type
                    AuthToken authToken = newAuthToken;
                    authToken.AccountsDBSave();

                    return newAuthToken;
                }
            }
        }

        /// <summary>
        /// Returns an existing valid AuthToken (after setting its lifespan to validDuration) if one is found, else creates and returns a new AuthToken.
        /// </summary>
        protected static T ExtendOrCreateNew<T>(Func<T> authTokenFindValid, Func<T> authTokenCreateNew, TimeSpan? validDuration) where T : AuthToken
        {
            Debug.Assert(authTokenFindValid != null);
            Debug.Assert(authTokenCreateNew != null);
            Debug.Assert(validDuration == null || ((TimeSpan)validDuration).Ticks > 0);

            T existingAuthToken = authTokenFindValid();
            if (existingAuthToken != null)
            {
                // We've already got an AuthToken. Fix up the expiry and reuse it.
                existingAuthToken.setRemainingDuration(validDuration);
                existingAuthToken.AccountsDBUpdate();
                return existingAuthToken;
            }

            T newAuthToken = authTokenCreateNew();
            Debug.Assert(newAuthToken != null);
            // (newly created tokens should already have the correct duration set)
            Debug.Assert(validDuration == null || newAuthToken.TotalDuration == validDuration);
            return newAuthToken;
        }

        protected static T ExpireAndCreateNew<T>(Func<T> authTokenFindValid, Func<T> authTokenCreateNew) where T : AuthToken
        {
            return ExpireAndCreateNew(authTokenFindValid, authTokenCreateNew, null);
        }

        /// <summary>
        /// Expires any currently valid AuthTokens, then creates and returns a new AuthToken.
        /// </summary>
        protected static T ExpireAndCreateNew<T>(Func<T> authTokenFindValid, Func<T> authTokenCreateNew, TimeSpan? validDuration) where T : AuthToken
        {
            Debug.Assert(authTokenFindValid != null);
            Debug.Assert(authTokenCreateNew != null);
            Debug.Assert(validDuration == null || ((TimeSpan)validDuration).Ticks > 0);

            T existingAuthToken;
            while((existingAuthToken = authTokenFindValid()) != null)
            {
                Debug.Assert(existingAuthToken.IsValid);

                // We've already got a valid AuthToken. Expire it before we create a new one.
                existingAuthToken.expireAndUpdateDB();
            }

            T newAuthToken = authTokenCreateNew();
            Debug.Assert(newAuthToken != null);
            // (newly created tokens should already have the correct duration set)
            Debug.Assert(validDuration == null || newAuthToken.TotalDuration == validDuration);
            return newAuthToken;
        }

        protected void expireAndUpdateDB()
        {
            this.ExpireDate = DateTime.UtcNow;
            UpdateDB();
        }

        protected void UpdateDB()
        {
            try
            {
                this.AccountsDBUpdate();
            }
            catch (Exception ex)
            {
                TorqContext.Current.EventLog.LogException(ex);
            }
        }

        private void setRemainingDuration(TimeSpan? validDuration)
        {
            if (validDuration != null)
            {
                DateTime utcNow = DateTime.UtcNow;
                this.ExpireDate = utcNow + validDuration;
            }
            else
            {
                this.ExpireDate = null;
            }
        }
    }
#endif

    //  AuthTokenType.Create(typeof(LoginAuthToken), "/login/", "@@@##-&&&&-32#"),

    //AuthTokenType.Create(typeof(PasswordResetAuthToken), "/pw/", "@##-&&&&-10#"),

    //AuthTokenType.Create(typeof(ProjectAuthToken), "/proj/", "&&&&&&&&"),
    //AuthTokenType.Create(typeof(ReportAuthToken), "/report/", "&26&&-&&&&-&&", true),
    // (Note, we can have multiple entries with different paths...)
    //AuthTokenType.Create(typeof(CreateProjectAuthToken), "/getstarted/", "^^^-###", "/getstarted/default.aspx"),
    //AuthTokenType.Create(typeof(CreateProjectAuthToken), "/", "^^^-###", "/getstarted/default.aspx"),
    //AuthTokenType.Create(typeof(CreateUserAuthToken), "/invite/", "&23&&-&&&&-&&", true),




    /// <summary>
    /// Supplies the ability to login to the application. Supports both "User" and "Object" logins
    /// </summary>
    public class LoginAuthTemplate : WebAuthTemplate
    {
        private const string authTokenTypeName = "Login";
        private const string authCodePattern = "@##-&&&&-10#";

        public static readonly TimeSpan? defaultDuration = null;

        public static LoginAuthTemplate Instance = new LoginAuthTemplate(defaultDuration, "/login/", authCodePattern, true, "/goober");

/*
        // We use XML to persist the fields in this Data class.
        public class Data : XmlSerializableClass<Data>
        {
            public string TargetTable { get; set; }
            public int TargetID { get; set; }

            public Data()
            {
                Debug.Assert(!string.IsNullOrEmpty(this.TargetTable));
                Debug.Assert(this.TargetID > 0);
            }
        }
*/

        protected LoginAuthTemplate(TimeSpan? defaultDuration, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage) :
            base(defaultDuration, urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage)
        { }

        public static Uri RevokeAndCreateNewUserUrl(WebUtilityDC dc, int userID, int usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need a permission check?

            var userLoginAuthToken = WebAuthTemplate.RevokeAndCreateNewAuthToken(dc, () => QueryValidUsers(dc, userID), () => CreateAuthToken(dc, typeof(User), userID, usageQuota, validDuration));

            var url = Instance.GetUrl(userLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }

        //!! think we can switch over from "Project" to "Event"!!!!
#if false
        public static Uri RevokeAndCreateNewProjectUrl(AppDC accountsDC, int projectID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?
            var authCheckProject = Project.Get(accountsDC, projectID);
            if (authCheckProject == null)
            {
                return null;
            }

            var projectLoginAuthToken = WebAuthTemplate.RevokeAndCreateNewAuthToken(accountsDC, () => QueryValidProjects(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration));

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }

        public static Uri ExtendOrCreateNewProjectUrl(AppDC accountsDC, int projectID, int? usageQuota, TimeSpan? validDuration)
        {
            var utilityContext = UtilityContext.Current;

            //!! nextgen - do we need this access check?
            //!! nextgen - does it help to pass in a Project so we can directly access check it?
            //!! nextgen - do we need to change the Project.Get() call to pass a DC?
            var authCheckProject = Project.Get(accountsDC, projectID);
            if (authCheckProject == null)
            {
                return null;
            }

            var projectLoginAuthToken = WebAuthTemplate.ExtendOrCreateNewAuthToken(accountsDC, () => QueryValidProjects(accountsDC, projectID), () => CreateAuthToken(accountsDC, typeof(Project), projectID, usageQuota, validDuration), validDuration);

            var url = Instance.GetUrl(projectLoginAuthToken, utilityContext.RequireSslLogin);
            return url;
        }
#endif

        private static AuthToken CreateAuthToken(WebUtilityDC dc, Type targetTableType, int targetID, int? usageQuota, TimeSpan? validDuration)
        {
            // Login tokens are defined within a Group scope (either TenantGroup or UserGroup)
            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

            var issuedByUserID = dc.TransactionAuthorizedBy.UserIDOrNull;

            var authToken = AuthToken.Create(dc, authCodePattern, authCode => AuthToken.Create(dc, teamEPScope, LoginAuthTemplate.authTokenTypeName, authCode, targetTableType, targetID, null, usageQuota, validDuration, issuedByUserID));
            return authToken;
        }


        public static IQueryable<AuthToken> Query(UtilityDC dc)
        {
            return AuthTemplate.QueryAuthTokens(dc, LoginAuthTemplate.authTokenTypeName);
        }
        public static IQueryable<AuthToken> QueryValid(UtilityDC dc)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, LoginAuthTemplate.authTokenTypeName);
        }


/*

        public static Uri ExpireAndCreateNewUserUrl(int clientID, int userID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNewUser(clientID, userID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
        }

        public static Uri ExpireAndCreateNewProjectUrl(int clientID, int projectID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNewProject(clientID, projectID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
        }

        private static LoginAuthToken ExpireAndCreateNewUser(int clientID, int userID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValidUser(clientID, userID), () => CreateNew(clientID, userID, null, validDuration));
        }

        private static AuthToken ExpireAndCreateNewProject(UtilityDC dc, int clientID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValidProject(dc, projectID), () => CreateNew(clientID, null, projectID, validDuration));
        }

        private static AuthToken CreateNew(int clientID, int? userID, int? projectID, TimeSpan? validDuration)
        {
            return AuthToken.CreateNew(authCode => new LoginAuthToken(authCode, clientID, userID, projectID, validDuration));
        }
*/
        public static IQueryable<AuthToken> QueryValidUsers(UtilityDC dc, int userID)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, LoginAuthTemplate.authTokenTypeName)
                .Where(authToken => authToken.TargetTable == typeof(User).Name)
                .Where(authToken => authToken.TargetID == userID);
        }

#if false
        public static IQueryable<AuthToken> QueryValidProjects(UtilityDC dc, int projectID)
        {
            return AuthTemplate.QueryValidAuthTokens(dc, LoginAuthTemplate.authTokenTypeName)
                .Where(authToken => authToken.TargetTable == typeof(Project).Name)
                .Where(authToken => authToken.TargetID == projectID);
        }
#endif

/*
        public Uri CreateSingle(WebUtilityDC dc, int userID)
        {
            var utilityContext = UtilityContext.Current;

            var passwordResetAuthToken = CreateSingleAuthToken(dc, userID);

            if (utilityContext.RequireSslLogin)
            {
                var secureUrl = GetSecureUrl(utilityContext.SiteUrl, passwordResetAuthToken.AuthCode, this.UrlPatternPrefix);
                return secureUrl;
            }

            var url = GetUrl(utilityContext.SiteUrl, passwordResetAuthToken.AuthCode, this.UrlPatternPrefix);
            return url;
        }
*/



        protected override void OnTokenClicked(HttpContext context, WebUtilityDC dc, AuthToken authToken)
        {
            UtilityContext utilityContext = UtilityContext.Current;

            //!! nextgen - do we require an indication on the AuthToken if it requires SSL?

            if (utilityContext.RequireSslLogin)
            {
                // (If we redirect here, the link the user clicked on will remain in the browser URL)
                context.RedirectToSecureConnection();
                Debug.Assert(context.Request.IsSecureConnection);
            }

            base.OnTokenClicked(context, dc, authToken);


            // (this page handles both valid and invalid clicks)

            string queryString = string.Format("authCode={0}",
                /*0*/ FormatAuthCode(authToken.AuthCode, AuthCodeFormat.Url));

            context.RewritePath("/ResetPassword.aspx", string.Empty, queryString);
        }




/*
        private LoginAuthToken(AuthCode authCode, int clientID, int? userID, int? projectID, TimeSpan? validDuration)
            : base(authCode, clientID, userID, projectID, NullSettings, validDuration)
        {
            // Login as a user or to a project, but not both
            Debug.Assert(userID.HasValue ^ projectID.HasValue);
        }
*/

/*



        public Uri SecureUrl
        {
            get { return base.ToSecureUrl(AuthTokenType.UrlPatternPrefix); }
        }
*/
/*
        protected override void OnClicked(HttpContext context, bool isValid)
        {
            TorqContext torq = TorqContext.Current;

            base.OnClicked(context, isValid);

            if (isValid)
            {
                if (torq.RequireSslLogin)
                {
                    context.RedirectToSecureConnection();
                    Debug.Assert(context.Request.IsSecureConnection);
                }

                // Expire - as this is a one-shot AuthToken
                this.expireAndUpdateDB();

                Debug.Assert(this.AuthUserID.HasValue ^ this.AuthProjectID.HasValue);

                if (this.AuthUserID.HasValue)
                {
                    TorqUserAuthentication.LoginAuthTokenSession(this.AuthUser, Product.cTORQ);

                    //!! is this really where we want to redirect?
                    // Redirect - so the client can save the AuthCookie
                    string qs = context.Request.QueryString.ToString();
                    context.Response.Redirect("/project/" + qs);
                    Debug.Fail("Shouldn't get here...");
                }

                if (this.AuthProjectID.HasValue)
                {
                    TorqProjectAuthentication.StartSession(context, this.AuthProject, AccessLicense.Unlimited, Product.cTORQ, this.AuthTokenID);

                    //!! is this really where we want to redirect?
                    // Redirect - so the client can save the AuthCookie
                    string qs = context.Request.QueryString.ToString();
                    if (qs.Length > 0)
                    {
                        qs = "&" + qs;
                    }
                    context.Response.Redirect("/project/JobCounselor.aspx?pid=" + this.AuthProjectID.Value.ToString() + qs);
                    Debug.Fail("Shouldn't get here...");
                }
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_url&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }
*/
/*
        public override bool IsValid
        {
            get
            {
                if (this.AuthProject == null && this.AuthUser == null)
                {
                    return false;
                }
                return base.IsValid;
            }
        }
*/
    }






    public class CreateSsoUserAuthTemplate : WebAuthTemplate
    {
        private const string authTokenTypeName = "CreateSsoUser";

        //!! nextgen - fix the pattern
        private const string authCodePattern = "@##-&&&&-10#";

        public static CreateSsoUserAuthTemplate Instance = new CreateSsoUserAuthTemplate(defaultDuration, "/createuser/", authCodePattern, true, "/goober");



        private static readonly TimeSpan defaultDuration = TimeSpan.FromDays(3650.0);
        //public static readonly CreateUserSsoAuthorized[] EmptyArray = new CreateUserSsoAuthorized[0];

        // We use XML to persist the fields in this Data class.
        public class Data : XmlSerializableClass<Data>
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public AppRole AppRole { get; set; }
            //!! public Product Product { get; set; }
            public string PreferredState { get; set; }
            public string PreferredLaborMarketArea { get; set; }
        }

        //private Data data;
#if false
        partial void OnLoaded()
        {
            this.data = XmlSerializer<Data>.Load(this.Settings);
        }


        public MailAddress MailAddress
        {
            get
            {
                Debug.Assert(!string.IsNullOrEmpty(this.data.Email));
                return this.data.Email.ParseMailAddress();
            }
        }

        public string Email
        {
            get
            {
                string email = string.Empty;
                if (!string.IsNullOrEmpty(this.data.Email))
                {
                    email = this.data.Email;
                }
                return email;
            }
        }

        public string FirstName
        {
            get
            {
                string name = string.Empty;
                if (!string.IsNullOrEmpty(this.data.FirstName))
                {
                    name = this.data.FirstName;
                }
                return name;
            }
        }

        public string LastName
        {
            get
            {
                string name = string.Empty;
                if (!string.IsNullOrEmpty(this.data.LastName))
                {
                    name = this.data.LastName;
                }
                return name;
            }
        }

        public AppRole AppRole
        {
            get
            {
                return this.data.app.AccessPermission;
            }
        }

        public Product Product
        {
            get
            {
                return this.data.Product;
            }
        }

        //public CreateUserAuthTokenStatus Status
        //{
        //    get
        //    {
        //        CreateUserAuthTokenStatus status = CreateUserAuthTokenStatus.Expired;

        //        // see if the email is being used
        //        if (this.MailAddress != null)
        //        {
        //            User user = User.GetByEmail(this.MailAddress);
        //            if (user != null)
        //            {
        //                status = CreateUserAuthTokenStatus.Accepted;
        //            }
        //            else if (this.IsValid)
        //            {
        //                status = CreateUserAuthTokenStatus.Pending;
        //            }
        //        }

        //        return status;
        //    }
        //}

        public string PreferredState
        {
            get
            {
                string lms = string.Empty;
                if (!string.IsNullOrEmpty(this.data.PreferredState))
                {
                    lms = this.data.PreferredState;
                }
                return lms;
            }
        }

        public string PreferredLaborMarketArea
        {
            get
            {
                string lma = string.Empty;
                if (!string.IsNullOrEmpty(this.data.PreferredLaborMarketArea))
                {
                    lma = this.data.PreferredLaborMarketArea;
                }
                return lma;
            }
        }

        //!! Hmm. Any writable properties in the XML blob that we change need to be persisted back to the DB in the Settings string.
        //   What's the best symantics for that write? This simple mechanism assumes we should write back on each property change.
        private void updateSettings()
        {
            string settingsString = this.data.ToString();
            this.Settings = settingsString;
            base.UpdateDB();
        }
#endif
        protected CreateSsoUserAuthTemplate(TimeSpan? defaultDuration, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage) :
            base(defaultDuration, urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage)
        { }



        private static AuthToken CreateAuthToken(WebUtilityDC dc, Data data, TimeSpan? validDuration)
        {
            // Login tokens are defined within a Group scope (either TenantGroup or UserGroup)
            var teamEPScope = dc.TransactionAuthorizedBy.TeamEPScopeOrThrow;

            var propertiesDataString = data.ToString();

            var issuedByUserID = dc.TransactionAuthorizedBy.UserIDOrNull;

            var authToken = AuthToken.Create(dc, authCodePattern, authCode => AuthToken.Create(dc, teamEPScope, CreateSsoUserAuthTemplate.authTokenTypeName, authCode, null, null, propertiesDataString, 1, validDuration, issuedByUserID));
            return authToken;
        }


        private static AuthToken CreateAuthToken(WebUtilityDC dc, int authorizedByUserID, int clientID, AppRole appRole, string lms, string lma, MailAddress mailAddress, TimeSpan duration)
        {
            Debug.Assert(mailAddress != null);

            Data data = new Data();

            // Save Email
            data.Email = mailAddress.Address;

            // Access
            data.AppRole = appRole;

            // LMA
            data.PreferredState = lms;
            data.PreferredLaborMarketArea = lma;

            // Save First and Last name, if we have DisplayName
            string displayName = mailAddress.DisplayName;
            if (displayName.Length > 0)
            {
                string[] names = displayName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 1)
                {
                    string first = string.Empty;
                    string last = string.Empty;

                    if (names[0].EndsWith(","))
                    {
                        // starts with the last name (eg. Freeman, Aaron)
                        last = names[0].Remove(names[0].Length - 1);
                        first = names[1];
                    }
                    else
                    {
                        first = names[0];
                        last = names[names.Length - 1];
                    }

                    // names
                    data.FirstName = first;
                    data.LastName = last;
                }
            }

            var authToken = CreateAuthToken(dc, data, duration);
            Debug.Assert(authToken.ID > 0);

            // For LINQ purposes, we need to save the base type
            //!! AuthToken authToken = result;
            //!! dc.Save(authToken);

            return authToken;
        }

        internal static AuthToken FindValid(AppDC dc, int clientID, MailAddress mailAddress)
        {
            var validAuthToken = AuthTemplate.QueryValidAuthTokens(dc, CreateSsoUserAuthTemplate.authTokenTypeName)
                .AsEnumerable()

                .Select(authToken => new { authToken, properties = XmlSerializer<Data>.LoadString(authToken.Properties) })
                .Where(authTokenData => authTokenData.properties.Email == mailAddress.Address)
                .Select(authTokenData => authTokenData.authToken)

                .FirstOrDefault();

            return validAuthToken;
        }

#if false
        public static CreateUserAuthToken ExpireAndCreateNewUrl(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress)
        {
            return ExpireAndCreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, defaultDuration);
        }

        public static CreateUserAuthToken ExpireAndCreateNew(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress, TimeSpan duration)
        {
            CreateUserAuthToken authToken = AuthToken.ExpireAndCreateNew(() => FindValid(newUserClientID, newUserMailAddress), () => CreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, duration));
            return authToken;
        }


        public static bool Expire(TorqIdentity authorizedBy, AuthCode authCode)
        {
            bool expired = false;
            CreateUserAuthToken existingAuthToken = AuthToken.FirstOrDefault<CreateUserAuthToken>(authCode);

            //!! Need to validate that caller is authorized to expire this authCode.

            if (existingAuthToken != null)
            {
                // Expire it 
                existingAuthToken.expireAndUpdateDB();

                expired = true;
            }

            return expired;
        }
#endif

#if false
        public static IEnumerable<CreateUserAuthToken> GetInvites(TorqIdentity identity, int? clientId)
        {
            IEnumerable<CreateUserAuthToken> invitesQuery = AuthToken.Find<CreateUserAuthToken>(identity);

            // if present, Client Group must match
            if (clientId.HasValue && clientId.Value != -1)
            {
                ClientInfo info = Client.GetCachedClientInfo(clientId.Value);
                if (info.HasDescendents)
                {
                    // get all client ids for this parent client
                    List<int> ids = info.Descendents.Select(c => c.ClientID).ToList();
                    invitesQuery = invitesQuery.Where(invite => invite.AuthClientID.HasValue && ids.Contains(invite.AuthClientID.Value));
                }
                else
                {
                    invitesQuery = invitesQuery.Where(invite => clientId == invite.AuthClientID);
                }
            }

            return invitesQuery;
        }
#endif

#if false
        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                string queryString = string.Format("authCode={0}&cid={1}&email={2}",
                    /*0*/ this.AuthCode,
                    /*1*/ this.AuthClientID,
                    /*2*/ this.MailAddress.Address);
                context.RewritePath("/CreateNewUser.aspx", string.Empty, queryString);
                return;
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_user&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }
#endif

#if false
        public bool TrackUsage()
        {
            bool isValid = this.IsValid;
            if (isValid)
            {
                //!! these guys are one-shots. How best to track this?
                this.expireAndUpdateDB();
            }
            return isValid;
        }
#endif

#if false
        public override bool IsValid
        {
            get
            {
                // verify we have an AuthUserID, and it represents a valid user
                if (this.AuthUser == null)
                {
                    return false;
                }

                // Make sure user that created this link still has authority to do this action
                TorqIdentity authUserIdentity = this.AuthUser.GetUserIdentity();
                if (!authUserIdentity.PermissionCheck(Permission.IssueCreateUserLink))
                {
                    return false;
                }

                if (this.AuthClientID == 0 || this.MailAddress == null)
                {
                    return false;
                }

                return base.IsValid;
            }
        }
#endif

    }






#if false

    /// <summary>
    /// Provides authority to one-time login to a user or project. Used by single-sign-on.
    /// </summary>
    public partial class LoginAuthToken
    {
        public static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(LoginAuthToken));

        private LoginAuthToken(AuthCode authCode, int clientID, int? userID, int? projectID, TimeSpan? validDuration)
            : base(authCode, clientID, userID, projectID, NullSettings, validDuration)
        {
            // Login as a user or to a project, but not both
            Debug.Assert(userID.HasValue ^ projectID.HasValue);
        }

        private static LoginAuthToken CreateNew(int clientID, int? userID, int? projectID, TimeSpan? validDuration)
        {
            return AuthToken.CreateNew(authCode => new LoginAuthToken(authCode, clientID, userID, projectID, validDuration));
        }

        public static LoginAuthToken FindValidUser(int clientID, int userID)
        {
            return AuthToken.FindValid<LoginAuthToken>(clientID)
                .Where(authToken => authToken.AuthUserID == userID)
                .FirstOrDefault();
        }

        public static LoginAuthToken FindValidProject(int clientID, int projectID)
        {
            return AuthToken.FindValid<LoginAuthToken>(clientID)
                .Where(authToken => authToken.AuthProjectID == projectID)
                .FirstOrDefault();
        }

        private static LoginAuthToken ExpireAndCreateNewUser(int clientID, int userID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValidUser(clientID, userID), () => CreateNew(clientID, userID, null, validDuration));
        }

        private static LoginAuthToken ExpireAndCreateNewProject(int clientID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValidProject(clientID, projectID), () => CreateNew(clientID, null, projectID, validDuration));
        }

        public static Uri ExpireAndCreateNewUserUrl(int clientID, int userID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNewUser(clientID, userID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
        }

        public static Uri ExpireAndCreateNewProjectUrl(int clientID, int projectID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNewProject(clientID, projectID, validDuration).ToSecureUrl(AuthTokenType.UrlPatternPrefix);
        }

        public Uri SecureUrl
        {
            get { return base.ToSecureUrl(AuthTokenType.UrlPatternPrefix); }
        }

        protected override void OnClicked(HttpContext context, bool isValid)
        {
            TorqContext torq = TorqContext.Current;

            base.OnClicked(context, isValid);

            if (isValid)
            {
                if (torq.RequireSslLogin)
                {
                    context.RedirectToSecureConnection();
                    Debug.Assert(context.Request.IsSecureConnection);
                }

                // Expire - as this is a one-shot AuthToken
                this.expireAndUpdateDB();

                Debug.Assert(this.AuthUserID.HasValue ^ this.AuthProjectID.HasValue);

                if (this.AuthUserID.HasValue)
                {
                    TorqUserAuthentication.LoginAuthTokenSession(this.AuthUser, Product.cTORQ);

                    //!! is this really where we want to redirect?
                    // Redirect - so the client can save the AuthCookie
                    string qs = context.Request.QueryString.ToString();
                    context.Response.Redirect("/project/" + qs);
                    Debug.Fail("Shouldn't get here...");
                }

                if (this.AuthProjectID.HasValue)
                {
                    TorqProjectAuthentication.StartSession(context, this.AuthProject, AccessLicense.Unlimited, Product.cTORQ, this.AuthTokenID);

                    //!! is this really where we want to redirect?
                    // Redirect - so the client can save the AuthCookie
                    string qs = context.Request.QueryString.ToString();
                    if (qs.Length > 0)
                    {
                        qs = "&" + qs;
                    }
                    context.Response.Redirect("/project/JobCounselor.aspx?pid=" + this.AuthProjectID.Value.ToString() + qs);
                    Debug.Fail("Shouldn't get here...");
                }
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_url&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }

        public override bool IsValid
        {
            get
            {
                if (this.AuthProject == null && this.AuthUser == null)
                {
                    return false;
                }
                return base.IsValid;
            }
        }
    }

#endif
#if false

    /// <summary>
    /// Provides authority to change a password in a specific User account.
    /// </summary>
    public partial class PasswordResetAuthToken
    {
        private static TimeSpan defaultDuration = TimeSpan.FromDays(1.0);
        private static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(PasswordResetAuthToken));

        private PasswordResetAuthToken(AuthCode authCode, int clientID, int userID)
            : base(authCode, clientID, userID, NullProjectID, NullSettings, defaultDuration)
        { }

        private static PasswordResetAuthToken CreateNew(int clientID, int userID)
        {
            return AuthToken.CreateNew(authCode => new PasswordResetAuthToken(authCode, clientID, userID));
        }

        private static PasswordResetAuthToken FindValid(int clientID, int userID)
        {
            return AuthToken.FindValid<PasswordResetAuthToken>(clientID)
                .Where(authToken => authToken.AuthUserID.HasValue && authToken.AuthUserID.Value == userID)
                .FirstOrDefault();
        }

        private static PasswordResetAuthToken ExpireAndCreateNew(int clientID, int userID)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValid(clientID, userID), () => CreateNew(clientID, userID));
        }

        public static PasswordResetAuthToken FindValid(AuthCode authCode)
        {
            return AuthToken.FirstOrDefault<PasswordResetAuthToken>(authCode);
        }

        public static Uri ExpireAndCreateNewUrl(int clientID, int userID)
        {
            PasswordResetAuthToken passwordResetAuthToken = ExpireAndCreateNew(clientID, userID);

            if (TorqContext.Current.RequireSslLogin)
            {
                return passwordResetAuthToken.SecureUrl;
            }

            return passwordResetAuthToken.Url;
        }

        public bool TrackUsage()
        {
            bool isValid = this.IsValid;
            if (isValid)
            {
                //!! these guys are one-shots. How best to track this?
                this.expireAndUpdateDB();
            }
            return isValid;
        }

        public Uri Url
        {
            get { return base.ToUrl(AuthTokenType.UrlPatternPrefix); }
        }

        public Uri SecureUrl
        {
            get { return base.ToSecureUrl(AuthTokenType.UrlPatternPrefix); }
        }

        protected override void OnClicked(HttpContext context, bool isValid)
        {
            TorqContext torq = TorqContext.Current;

            base.OnClicked(context, isValid);

            if (torq.RequireSslLogin)
            {
                context.RedirectToSecureConnection();
                Debug.Assert(context.Request.IsSecureConnection);
            }

            // (this page handles both valid and invalid clicks)

            string queryString = string.Format("authCode={0}",
                /*0*/ this.AuthCodeString);
            context.RewritePath("/ResetPassword.aspx", string.Empty, queryString);
        }

        public override bool IsValid
        {
            get
            {
                if (this.AuthUser == null)
                {
                    return false;
                }
                return base.IsValid;
            }
        }
    }

    /// <summary>
    /// Provides authority to view/edit one specific TORQ project
    /// </summary>
    public partial class ProjectAuthToken
    {
        public static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(ProjectAuthToken));

        private ProjectAuthToken(AuthCode authCode, int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
            : base(authCode, clientID, issuedByUserID, projectID, NullSettings, validDuration)
        { }

        private static ProjectAuthToken CreateNew(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.CreateNew(authCode => new ProjectAuthToken(authCode, clientID, issuedByUserID, projectID, validDuration));
        }

        public static ProjectAuthToken FindValid(int clientID, int projectID)
        {
            return AuthToken.FindValid<ProjectAuthToken>(clientID)
                .Where(authToken => authToken.AuthProjectID == projectID)
                .FirstOrDefault();
        }

        private static ProjectAuthToken ExtendOrCreateNew(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.ExtendOrCreateNew(() => FindValid(clientID, projectID), () => CreateNew(clientID, issuedByUserID, projectID, validDuration), validDuration);
        }

        private static ProjectAuthToken ExpireAndCreateNew(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return AuthToken.ExpireAndCreateNew(() => FindValid(clientID, projectID), () => CreateNew(clientID, issuedByUserID, projectID, validDuration));
        }

        public static Uri ExtendOrCreateNewUrl(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return ExtendOrCreateNew(clientID, issuedByUserID, projectID, validDuration).ToUrl(AuthTokenType.UrlPatternPrefix);
        }

        public static Uri ExpireAndCreateNewUrl(int clientID, int? issuedByUserID, int projectID, TimeSpan? validDuration)
        {
            return ExpireAndCreateNew(clientID, issuedByUserID, projectID, validDuration).ToUrl(AuthTokenType.UrlPatternPrefix);
        }

        public Uri Url
        {
            get { return base.ToUrl(AuthTokenType.UrlPatternPrefix); }
        }

        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                TorqProjectAuthentication.StartSession(context, this.AuthProject, AccessLicense.Unlimited, Product.cTORQ, this.AuthTokenID);

                //!! is this really where we want to redirect?
                // Redirect - so the client can save the AuthCookie
                string qs = context.Request.QueryString.ToString();
                if (qs.Length > 0)
                {
                    qs = "&" + qs;
                }
                context.Response.Redirect("/project/JobCounselor.aspx?pid=" + this.AuthProjectID.Value.ToString() + qs);
                Debug.Fail("Shouldn't get here...");
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_url&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }

        public override bool IsValid
        {
            get
            {
                if (this.AuthProject == null)
                {
                    return false;
                }
                return base.IsValid;
            }
        }
    }
#endif


#if true
    /// <summary>
    /// Provides authority to create a new TORQ user
    /// </summary>
    public partial class CreateUserAuthTemplate : MS.WebUtility.CreateUserAuthTemplate<CreateUserAuthTemplate.Data>
    {
        // We use XML to persist the fields in this Data class.
        public new class Data : MS.WebUtility.CreateUserAuthTemplate<CreateUserAuthTemplate.Data>.Data
        {
            //!! public User.AccessPermissions AccessPermission { get; set; }
            //public Product Product { get; set; }
            //public DateTime LastSentDate { get; set; }
            //public string PreferredState { get; set; }
            //public string PreferredLaborMarketArea { get; set; }
        }

        protected CreateUserAuthTemplate(TimeSpan? defaultDuration, string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage) :
            base(defaultDuration, urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage)
        { }


        public static AuthToken FindAuthToken(UtilityDC dc, AuthCode authCode)
        {
            return AuthTemplate.FindAuthToken<CreateUserAuthTemplate>(dc, authCode);
        }

        public static AuthToken FindAuthToken<T>(UtilityDC dc, AuthCode authCode, out T data)
        {
            return AuthTemplate.FindAuthToken<T, CreateUserAuthTemplate>(dc, authCode, out data);
        }


    }
#endif




#if false
    /// <summary>
    /// Provides authority to create a new TORQ user
    /// </summary>
    public partial class CreateUserAuthTemplate : WebAuthTemplate
    {
        private const string authTokenTypeName = "CreateUser";
        private const string authCodePattern = "&23&&-&&&&-&&";

        private static readonly TimeSpan defaultDuration = TimeSpan.FromDays(3650.0);

        public static CreateUserAuthTemplate Instance = new CreateUserAuthTemplate("/invite/", authCodePattern, true, "/goober");

        //public static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(CreateUserAuthToken));
        //public static readonly CreateUserAuthToken[] EmptyArray = new CreateUserAuthToken[0];

        // We use XML to persist the fields in this Data class.
        public class Data : XmlSerializableClass<Data>
        {
            public MS.Utility.Xml.MailAddress MailAddress { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            //!! public User.AccessPermissions AccessPermission { get; set; }
            public SystemRole[] SystemRoles { get; set; }
            public AppRole[] AppRoles { get; set; }
            public Product Product { get; set; }
            public DateTime LastSentDate { get; set; }
            public string PreferredState { get; set; }
            public string PreferredLaborMarketArea { get; set; }
        }

        //!! nextgen - some properties used to call this, now to account for it?
#if false
        //!! Hmm. Any writable properties in the XML blob that we change need to be persisted back to the DB in the Settings string.
        //   What's the best symantics for that write? This simple mechanism assumes we should write back on each property change.
        private void updateSettings()
        {
            string settingsString = this.data.ToString();
            this.Settings = settingsString;
            base.UpdateDB();
        }
#endif

        protected CreateUserAuthTemplate(string urlPatternPrefix, string authCodePattern, bool requireAuthCodeFormatting, string tokenNotFoundPage) :
            base(urlPatternPrefix, authCodePattern, requireAuthCodeFormatting, tokenNotFoundPage)
        { }

        private static AuthToken CreateAuthToken(AppDC dc, EPScope epScope, MailAddress inviteeMailAddress)
        {

            Debug.Assert(inviteeMailAddress != null);

            Data data = new Data();

            data.MailAddress = inviteeMailAddress;

            // Save Email
            data.Email = inviteeMailAddress.Address;

            // Product
            //data.Product = product;

            // Access
            //data.AccessPermission = permission;

            // LMA
            //data.PreferredState = lms;
            //data.PreferredLaborMarketArea = lma;

            // Save First and Last name, if we have DisplayName
            string displayName = inviteeMailAddress.DisplayName;
            if (displayName.Length > 0)
            {
                string[] names = displayName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 1)
                {
                    string first = string.Empty;
                    string last = string.Empty;

                    if (names[0].EndsWith(","))
                    {
                        // starts with the last name (eg. Freeman, Aaron)
                        last = names[0].Remove(names[0].Length - 1);
                        first = names[1];
                    }
                    else
                    {
                        first = names[0];
                        last = names[names.Length - 1];
                    }

                    // names
                    data.FirstName = first;
                    data.LastName = last;
                }
            }

            //!! return AuthToken.CreateNew(authCode => new CreateUserAuthToken(authCode, authorizedByUserID, clientID, data, duration));


            var authToken = WebAuthTemplate.CreateAuthToken(dc, authCodePattern, authCode => new AuthToken(epScope, authTokenTypeName, authCode, data, 1, defaultDuration));
            return authToken;
        }



/*
        private static CreateUserAuthToken CreateNew(int authorizedByUserID, int clientID, User.AccessPermissions permission, Product product, string lms, string lma, MailAddress mailAddress, TimeSpan duration)
        {
            Debug.Assert(mailAddress != null);

            Data data = new Data();

            // Save Email
            data.Email = mailAddress.Address;

            // Product
            data.Product = product;

            // Access
            data.AccessPermission = permission;

            // LMA
            data.PreferredState = lms;
            data.PreferredLaborMarketArea = lma;

            // Save First and Last name, if we have DisplayName
            string displayName = mailAddress.DisplayName;
            if (displayName.Length > 0)
            {
                string[] names = displayName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 1)
                {
                    string first = string.Empty;
                    string last = string.Empty;

                    if (names[0].EndsWith(","))
                    {
                        // starts with the last name (eg. Freeman, Aaron)
                        last = names[0].Remove(names[0].Length - 1);
                        first = names[1];
                    }
                    else
                    {
                        first = names[0];
                        last = names[names.Length - 1];
                    }

                    // names
                    data.FirstName = first;
                    data.LastName = last;
                }
            }

            return AuthToken.CreateNew(authCode => new CreateUserAuthToken(authCode, authorizedByUserID, clientID, data, duration));
        }
*/

        public static AuthToken FindValidAuthToken(AppDC dc, MailAddress mailAddress)
        {
            return AuthToken.QueryValid(dc, authTokenTypeName)
                // bring all the valid AuthTokens to the web server, so we can filter on values in the data field
                .AsEnumerable()

                .Select(authToken => new { authToken, properties = XmlSerializer<Data>.LoadString(authToken.Properties) })
                .Where(authTokenData => authTokenData.properties.Email == mailAddress.Address)
                .Select(authTokenData => authTokenData.authToken)

                .FirstOrDefault();
        }

        public static AuthToken FindAuthToken(AppDC dc, string authCodeValue)
        {
            var authToken = FindAuthToken(dc, CreateUserAuthTemplate.authTokenTypeName, authCodeValue);
            return authToken;
        }

        public static AuthToken FindAuthToken(AppDC dc, string authCodeValue, out Data data)
        {
            var authToken = FindAuthToken(dc, CreateUserAuthTemplate.authTokenTypeName, authCodeValue, out data);
            return authToken;
        }

        public static Uri GetUrl(AuthToken authToken)
        {
            var url = Instance.GetUrl(authToken.AuthCode, false);
            return url;
        }

        //CreateProjectAuthTemplate.Data createProjectAuthTokenProperties;
        //var createProjectAuthToken = AuthToken.Find<CreateProjectAuthTemplate.Data>(dc, createProjectAuthCode, out createProjectAuthTokenProperties);


        public static AuthToken ExpireAndCreateNew(AppDC dc, EPScope epScope, MailAddress inviteeMailAddress)
        {
            var utilityContext = UtilityContext.Current;

            var authToken = WebAuthTemplate.ExpireAndCreateNewAuthToken(dc, () => FindValidAuthToken(dc, inviteeMailAddress), () => CreateAuthToken(dc, epScope, inviteeMailAddress));
            return authToken;
        }



#if false
        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                string queryString = string.Format("authCode={0}&cid={1}&email={2}",
                    /*0*/ this.AuthCode,
                    /*1*/ this.AuthClientID,
                    /*2*/ this.MailAddress.Address);
                context.RewritePath("/CreateNewUser.aspx", string.Empty, queryString);
                return;
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_user&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }
#endif


        protected override void OnTokenClicked(HttpContext context, WebUtilityDC dc, AuthToken authToken)
        {
            UtilityContext utilityContext = UtilityContext.Current;

            //!! can this be pushed to the base class? Can the template indicate if Ssl is required?
            if (utilityContext.RequireSslLogin)
            {
                // (If we redirect here, the link the user clicked on will remain in the browser URL)
                context.RedirectToSecureConnection();
                Debug.Assert(context.Request.IsSecureConnection);
            }

            base.OnTokenClicked(context, dc, authToken);

            // (this page handles both valid and invalid clicks)

            //context.Server.Transfer("/app/default.aspx");
            //Debug.Fail("Shouldn't get here");


            string queryString = string.Format("authCode={0}",
                /*0*/ FormatAuthCode(authToken.AuthCode, AuthCodeFormat.Url));
            //!! context.RewritePath("/Signin/ResetPassword.aspx", string.Empty, queryString);
            //context.RewritePath("/app/default.aspx", string.Empty, queryString);
//!! need some help deciding if we should go to a client Spa or server page
            context.RewritePath("/app/default.aspx", string.Empty, queryString);
        }




#if false
        public static AuthToken FindAuthToken(TorqDataContext accountsDC, AuthCode authCode)
        {
            var authToken = AuthToken.Find(accountsDC, authCode);
            return authToken;
        }


        public static AuthToken FindValid(TorqDataContext accountsDC, AuthCode authCode)
        {
            var authToken = AuthToken.Find(accountsDC, authCode);
            return authToken;
        }


        public static CreateUserAuthToken ExpireAndCreateNewUrl(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress)
        {
            return ExpireAndCreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, defaultDuration);
        }

        public static CreateUserAuthToken ExpireAndCreateNew(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress, TimeSpan duration)
        {
            CreateUserAuthToken authToken = AuthToken.ExpireAndCreateNew(() => FindValid(newUserClientID, newUserMailAddress), () => CreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, duration));
            return authToken;
        }

        public static bool Expire(TorqIdentity authorizedBy, AuthCode authCode)
        {
            bool expired = false;
            CreateUserAuthToken existingAuthToken = AuthToken.FirstOrDefault<CreateUserAuthToken>(authCode);

            //!! Need to validate that caller is authorized to expire this authCode.

            if (existingAuthToken != null)
            {
                // Expire it 
                existingAuthToken.expireAndUpdateDB();

                expired = true;
            }

            return expired;
        }

        public Uri Url
        {
            get { return base.ToUrl(AuthTokenType.UrlPatternPrefix); }
        }

        public void SetLastSentDate(DateTime timestamp)
        {
            Debug.Assert(timestamp > new DateTime(2011, 6, 1));
            this.LastSentDate = timestamp;
        }

        public static IEnumerable<CreateUserAuthToken> GetInvites(TorqIdentity identity, int? clientId)
        {
            IEnumerable<CreateUserAuthToken> invitesQuery = AuthToken.Find<CreateUserAuthToken>(identity);

            // if present, Client Group must match
            if (clientId.HasValue && clientId.Value != -1)
            {
                ClientInfo info = Client.GetCachedClientInfo(clientId.Value);
                if (info.HasDescendents)
                {
                    // get all client ids for this parent client
                    List<int> ids = info.Descendents.Select(c => c.ClientID).ToList();
                    invitesQuery = invitesQuery.Where(invite => invite.AuthClientID.HasValue && ids.Contains(invite.AuthClientID.Value));
                }
                else
                {
                    invitesQuery = invitesQuery.Where(invite => clientId == invite.AuthClientID);
                }
            }

            return invitesQuery;
        }

        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                string queryString = string.Format("authCode={0}&cid={1}&email={2}",
                    /*0*/ this.AuthCode,
                    /*1*/ this.AuthClientID,
                    /*2*/ this.MailAddress.Address);
                context.RewritePath("/CreateNewUser.aspx", string.Empty, queryString);
                return;
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_user&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }

        public bool TrackUsage()
        {
            bool isValid = this.IsValid;
            if (isValid)
            {
                //!! these guys are one-shots. How best to track this?
                this.expireAndUpdateDB();
            }
            return isValid;
        }

        public override bool IsValid
        {
            get
            {
                // verify we have an AuthUserID, and it represents a valid user
                if (this.AuthUser == null)
                {
                    return false;
                }

                // Make sure user that created this link still has authority to do this action
                TorqIdentity authUserIdentity = this.AuthUser.GetUserIdentity();
                if (!authUserIdentity.PermissionCheck(Permission.IssueCreateUserLink))
                {
                    return false;
                }

                if (this.AuthClientID == 0 || this.MailAddress == null)
                {
                    return false;
                }

                return base.IsValid;
            }
        }
#endif
    }
#endif

#if false


    /// <summary>
    /// Provides authority to create a new User via the PartnerApi
    /// </summary>
    public partial class CreateUserSsoAuthorized
    {
        private static readonly TimeSpan defaultDuration = TimeSpan.FromDays(3650.0);
        public static readonly CreateUserSsoAuthorized[] EmptyArray = new CreateUserSsoAuthorized[0];

        // We use XML to persist the fields in this Data class.
        public class Data : XmlSerializableClass<Data>
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }

            //public User.AccessPermissions AccessPermission { get; set; }
            public AppRole AppRole { get; set; }

            public Product Product { get; set; }
            public string PreferredState { get; set; }
            public string PreferredLaborMarketArea { get; set; }
        }

        private Data data;

        partial void OnLoaded()
        {
            this.data = XmlSerializer<Data>.Load(this.Settings);
        }

        public MailAddress MailAddress
        {
            get
            {
                Debug.Assert(!string.IsNullOrEmpty(this.data.Email));
                return this.data.Email.ParseMailAddress();
            }
        }

        public string Email
        {
            get
            {
                string email = string.Empty;
                if (!string.IsNullOrEmpty(this.data.Email))
                {
                    email = this.data.Email;
                }
                return email;
            }
        }

        public string FirstName
        {
            get
            {
                string name = string.Empty;
                if (!string.IsNullOrEmpty(this.data.FirstName))
                {
                    name = this.data.FirstName;
                }
                return name;
            }
        }

        public string LastName
        {
            get
            {
                string name = string.Empty;
                if (!string.IsNullOrEmpty(this.data.LastName))
                {
                    name = this.data.LastName;
                }
                return name;
            }
        }

        public User.AccessPermissions AccessPermission
        {
            get
            {
                return this.data.AccessPermission;
            }
        }

        public Product Product 
        {
            get
            {
                return this.data.Product;
            }
        }

        //public CreateUserAuthTokenStatus Status
        //{
        //    get
        //    {
        //        CreateUserAuthTokenStatus status = CreateUserAuthTokenStatus.Expired;

        //        // see if the email is being used
        //        if (this.MailAddress != null)
        //        {
        //            User user = User.GetByEmail(this.MailAddress);
        //            if (user != null)
        //            {
        //                status = CreateUserAuthTokenStatus.Accepted;
        //            }
        //            else if (this.IsValid)
        //            {
        //                status = CreateUserAuthTokenStatus.Pending;
        //            }
        //        }

        //        return status;
        //    }
        //}

        public string PreferredState
        {
            get
            {
                string lms = string.Empty;
                if (!string.IsNullOrEmpty(this.data.PreferredState))
                {
                    lms = this.data.PreferredState;
                }
                return lms;
            }
        }

        public string PreferredLaborMarketArea
        {
            get
            {
                string lma = string.Empty;
                if (!string.IsNullOrEmpty(this.data.PreferredLaborMarketArea))
                {
                    lma = this.data.PreferredLaborMarketArea;
                }
                return lma;
            }
        }

        //!! Hmm. Any writable properties in the XML blob that we change need to be persisted back to the DB in the Settings string.
        //   What's the best symantics for that write? This simple mechanism assumes we should write back on each property change.
        private void updateSettings()
        {
            string settingsString = this.data.ToString();
            this.Settings = settingsString;
            base.UpdateDB();
        }

        private CreateUserSsoAuthorized(int authorizedByUserID, int clientID, Data data, TimeSpan duration)
            : base(AuthCode.Invalid, clientID, authorizedByUserID, NullProjectID, data.ToString(), duration)
        {
            this.data = data;
        }

        private static CreateUserSsoAuthorized CreateNew(int authorizedByUserID, int clientID, User.AccessPermissions permission, Product product, string lms, string lma, MailAddress mailAddress, TimeSpan duration)
        {
            Debug.Assert(mailAddress != null);

            Data data = new Data();

            // Save Email
            data.Email = mailAddress.Address;

            // Product
            data.Product = product;

            // Access
            data.AccessPermission = permission;

            // LMA
            data.PreferredState = lms;
            data.PreferredLaborMarketArea = lma;

            // Save First and Last name, if we have DisplayName
            string displayName = mailAddress.DisplayName;
            if (displayName.Length > 0)
            {
                string[] names = displayName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 1)
                {
                    string first = string.Empty;
                    string last = string.Empty;

                    if (names[0].EndsWith(","))
                    {
                        // starts with the last name (eg. Freeman, Aaron)
                        last = names[0].Remove(names[0].Length - 1);
                        first = names[1];
                    }
                    else
                    {
                        first = names[0];
                        last = names[names.Length - 1];
                    }

                    // names
                    data.FirstName = first;
                    data.LastName = last;
                }
            }

            var result = new CreateUserSsoAuthorized(authorizedByUserID, clientID, data, duration);

            // For LINQ purposes, we need to save the base type
            AuthToken authToken = result;
            authToken.AccountsDBSave();

            return result;
        }

        internal static CreateUserSsoAuthorized FindValid(int clientID, MailAddress mailAddress)
        {
            var clientTokens = AuthToken.FindValid<CreateUserSsoAuthorized>(clientID);
            //!! not so efficient, but need to iterate through and find the right MailAddress
            foreach (var clientToken in clientTokens)
            {
                if (clientToken.data != null && clientToken.MailAddress.Address == mailAddress.Address)
                {
                    return clientToken;
                }
            }

            return null;
        }

#if false
        public static CreateUserAuthToken ExpireAndCreateNewUrl(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress)
        {
            return ExpireAndCreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, defaultDuration);
        }

        public static CreateUserAuthToken ExpireAndCreateNew(int authorizedByUserID, int newUserClientID, User.AccessPermissions newUserPermission, Product newUserProduct, string newUserLms, string newUserLma, MailAddress newUserMailAddress, TimeSpan duration)
        {
            CreateUserAuthToken authToken = AuthToken.ExpireAndCreateNew(() => FindValid(newUserClientID, newUserMailAddress), () => CreateNew(authorizedByUserID, newUserClientID, newUserPermission, newUserProduct, newUserLms, newUserLma, newUserMailAddress, duration));
            return authToken;
        }


        public static bool Expire(TorqIdentity authorizedBy, AuthCode authCode)
        {
            bool expired = false;
            CreateUserAuthToken existingAuthToken = AuthToken.FirstOrDefault<CreateUserAuthToken>(authCode);

            //!! Need to validate that caller is authorized to expire this authCode.

            if (existingAuthToken != null)
            {
                // Expire it 
                existingAuthToken.expireAndUpdateDB();

                expired = true;
            }

            return expired;
        }
#endif
        public static IEnumerable<CreateUserAuthToken> GetInvites(TorqIdentity identity, int? clientId)
        {
            IEnumerable<CreateUserAuthToken> invitesQuery = AuthToken.Find<CreateUserAuthToken>(identity);

            // if present, Client Group must match
            if (clientId.HasValue && clientId.Value != -1)
            {
                ClientInfo info = Client.GetCachedClientInfo(clientId.Value);
                if (info.HasDescendents)
                {
                    // get all client ids for this parent client
                    List<int> ids = info.Descendents.Select(c => c.ClientID).ToList();
                    invitesQuery = invitesQuery.Where(invite => invite.AuthClientID.HasValue && ids.Contains(invite.AuthClientID.Value));
                }
                else
                {
                    invitesQuery = invitesQuery.Where(invite => clientId == invite.AuthClientID);
                }
            }

            return invitesQuery;
        }
#if false
        protected override void OnClicked(HttpContext context, bool isValid)
        {
            base.OnClicked(context, isValid);

            if (isValid)
            {
                string queryString = string.Format("authCode={0}&cid={1}&email={2}",
                    /*0*/ this.AuthCode,
                    /*1*/ this.AuthClientID,
                    /*2*/ this.MailAddress.Address);
                context.RewritePath("/CreateNewUser.aspx", string.Empty, queryString);
                return;
            }

            string requestUrlString = context.Request.Url.ToString();

            context.Server.Transfer("/Message.aspx?mid=exp_user&url=" + requestUrlString);
            Debug.Fail("Shouldn't get here...");
        }
#endif
        public bool TrackUsage()
        {
            bool isValid = this.IsValid;
            if (isValid)
            {
                //!! these guys are one-shots. How best to track this?
                this.expireAndUpdateDB();
            }
            return isValid;
        }

        public override bool IsValid
        {
            get
            {
                // verify we have an AuthUserID, and it represents a valid user
                if (this.AuthUser == null)
                {
                    return false;
                }

                // Make sure user that created this link still has authority to do this action
                TorqIdentity authUserIdentity = this.AuthUser.GetUserIdentity();
                if (!authUserIdentity.PermissionCheck(Permission.IssueCreateUserLink))
                {
                    return false;
                }

                if (this.AuthClientID == 0 || this.MailAddress == null)
                {
                    return false;
                }

                return base.IsValid;
            }
        }
    }


    

    //SsoCreateUserRequested



    public partial class ReportAuthToken
    {
        private static AuthTokenType AuthTokenType = AuthTokenType.Find(typeof(ReportAuthToken));

        public static ReportAuthToken FindValid(AuthCode authCode)
        {
            return AuthToken.FirstOrDefault<ReportAuthToken>(authCode);
        }

        public int ProjectID
        {
            get
            {
                Debug.Assert(this.AuthProjectID != null && (int)this.AuthProjectID > 0);
                if (this.AuthProjectID != null)
                {
                    return (int)this.AuthProjectID;
                }
                return 0;
            }
        }

        public Uri Url
        {
            get { return base.ToUrl(AuthTokenType.UrlPatternPrefix); }
        }
    }

    public enum CreateUserAuthTokenStatus
    {
        Pending,
        Expired,
        Accepted
    }
#endif
}