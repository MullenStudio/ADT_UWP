//-----------------------------------------------------------------------
// <copyright file="AdtApi.cs" company="Mullen Studio">
//     Copyright (c) Mullen Studio. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MullenStudio.ADT_UWP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HtmlAgilityPack;
    using Windows.Web.Http;
    using Windows.Web.Http.Filters;

    /// <summary>
    /// Provides methods to call ADT API.
    /// </summary>
    public class AdtApi
    {
        /// <summary>
        /// The ADT domain.
        /// </summary>
        private const string AdtDomain = "https://mobile.adtpulse.com";

        /// <summary>
        /// The sign in URL.
        /// </summary>
        private const string SignInUrl = "/mobile/access/signin.jsp?e=ns&partner=adt";
        
        /// <summary>
        /// The sign out URL.
        /// </summary>
        private const string SignOutUrl = "/mobile/access/signout.jsp";

        /// <summary>
        /// The summary URL.
        /// </summary>
        private const string SummaryUrl = "/mobile/summary/summary.jsp";

        /// <summary>
        /// The URL to list current arm options.
        /// </summary>
        private const string ListArmUrl = "/mobile/quickcontrol/panel.jsp";

        /// <summary>
        /// The URL to set arm status.
        /// </summary>
        private const string SetArmUrl = "/mobile/quickcontrol/serv/ChangeVariableServ";

        /// <summary>
        /// The URL to list mode options.
        /// </summary>
        private const string ListModeUrl = "/mobile/quickcontrol/mode.jsp";
        
        /// <summary>
        /// The URL to set mode.
        /// </summary>
        private const string SetModeUrl = "/mobile/quickcontrol/serv/ChangeShiftServ";

        /// <summary>
        /// The URL to get log.
        /// </summary>
        private const string LogUrl = "/mobile/alarms/alarms.jsp";

        /// <summary>
        /// The URL suffix to find arm status.
        /// </summary>
        private const string ListArmSuffix = "/panel.jsp";

        /// <summary>
        /// The URL suffix to find mode status.
        /// </summary>
        private const string ListModeSuffix = "/mode.jsp";

        /// <summary>
        /// The URL suffix to confirm set arm success.
        /// </summary>
        private const string SetArmSuccessSuffix = "/controldone.jsp";

        /// <summary>
        /// The URL suffix to confirm set mode success.
        /// </summary>
        private const string SetModeSuccessSuffix = "/summary.jsp";

        /// <summary>
        /// The user-agent.
        /// </summary>
        private const string UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586";

        /// <summary>
        /// Current <see cref="AdtApi"/> instance.
        /// </summary>
        private static AdtApi current;

        /// <summary>
        /// The HTTP client.
        /// </summary>
        private HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdtApi"/> class.
        /// </summary>
        public AdtApi()
        {
            HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter()
            {
                AllowAutoRedirect = false
            };
            this.httpClient = new HttpClient(httpBaseProtocolFilter);
            this.httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent);
        }

        /// <summary>
        /// Gets or sets current <see cref="AdtApi"/> instance.
        /// </summary>
        public static AdtApi Current
        {
            get
            {
                if (current == null)
                {
                    current = new AdtApi();
                }

                return current;
            }

            set
            {
                current = value;
            }
        }

        /// <summary>
        /// Signs In.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <param name="keepLoggedIn">True if want to keep logged in.</param>
        /// <returns>True if sign in successfully.</returns>
        public async Task<bool> SignIn(string userName, string password, bool keepLoggedIn = true)
        {
            var content = new Dictionary<string, string>
            {
                { "username", userName },
                { "password", password },
                { "login", "Sign In" }
            };
            if (keepLoggedIn)
            {
                content.Add("keeploggedin", "YES");
            }

            var response = await this.httpClient.PostAsync(
                new Uri($"{AdtDomain}{SignInUrl}"), 
                new HttpFormUrlEncodedContent(content));
            return response.StatusCode == HttpStatusCode.Found;
        }

        /// <summary>
        /// Signs Out.
        /// </summary>
        /// <returns>True if sign out successfully.</returns>
        public async Task<bool> SignOut()
        {
            var response = await this.httpClient.GetAsync(new Uri($"{AdtDomain}{SignOutUrl}"));
            return response.StatusCode == HttpStatusCode.Found;
        }

        /// <summary>
        /// Gets the summary.
        /// </summary>
        /// <returns>The summary. Null if failed to get the summary.</returns>
        public async Task<Summary> GetSummary()
        {
            var response = await this.httpClient.GetAsync(new Uri($"{AdtDomain}{SummaryUrl}"));
            string html = await response.Content.ReadAsStringAsync();
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                string icon = doc.DocumentNode.Descendants("img")
                    .First(node => string.Equals(node.GetAttributeValue("alt", null as string), "System Icon", StringComparison.OrdinalIgnoreCase))
                    .GetAttributeValue("src", null as string);
                string security = doc.DocumentNode.Descendants("a")
                    .First(node => node.GetAttributeValue("href", string.Empty).EndsWith(ListArmSuffix, StringComparison.OrdinalIgnoreCase))
                    .NextSibling.InnerText;
                string mode = doc.DocumentNode.Descendants("a")
                    .First(node => node.GetAttributeValue("href", string.Empty).EndsWith(ListModeSuffix, StringComparison.OrdinalIgnoreCase))
                    .NextSibling.InnerText;
                return new Summary
                {
                    IconUrl = $"{AdtDomain}{icon}",
                    Arm = string.Join(" ", security.Replace(":", string.Empty).Split(new[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries)),
                    Mode = string.Join(" ", mode.Replace(":", string.Empty).Split(new[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Lists available arm options.
        /// </summary>
        /// <returns>The collection of arm options. The key is the value used as <see cref="SetArm(string)"/> parameter and the value is used for display. Null if failed to get current arm options.</returns>
        public async Task<IEnumerable<KeyValuePair<string, string>>> ListArmOptions()
        {
            var response = await this.httpClient.GetAsync(new Uri($"{AdtDomain}{ListArmUrl}"));
            string html = await response.Content.ReadAsStringAsync();
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc.DocumentNode.Descendants("input")
                    .Where(node => string.Equals(node.GetAttributeValue("name", null as string), "value"))
                    .Select(node => new KeyValuePair<string, string>(
                        node.GetAttributeValue("value", null as string),
                        GetDisplayValue(node)))
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the arm status.
        /// </summary>
        /// <param name="value">The arm status.</param>
        /// <returns>True if set arm successfully.</returns>
        /// <remarks>The available values are from the keys of <see cref="ListArmOptions"/>.</remarks>
        public async Task<bool> SetArm(string value)
        {
            var content = await this.GetArmContent(value);
            if (content == null)
            {
                return false;
            }

            try
            {
                this.httpClient.DefaultRequestHeaders.Referer = new Uri($"{AdtDomain}{ListArmUrl}");
                var response = await this.httpClient.PostAsync(
                    new Uri($"{AdtDomain}{SetArmUrl}"),
                    new HttpFormUrlEncodedContent(content));
                return response.Headers.Location.AbsoluteUri.EndsWith(SetArmSuccessSuffix, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                this.httpClient.DefaultRequestHeaders.Referer = null;
            }
        }

        /// <summary>
        /// Lists the mode options.
        /// </summary>
        /// <returns>The collection of mode options. The key is the value used as <see cref="SetMode(int)"/> parameter and the value is used for display. Null if failed to get the mode options.</returns>
        public async Task<IEnumerable<KeyValuePair<int, string>>> ListModes()
        {
            var response = await this.httpClient.GetAsync(new Uri($"{AdtDomain}{ListModeUrl}"));
            string html = await response.Content.ReadAsStringAsync();
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc.DocumentNode.Descendants("input")
                    .Where(node => string.Equals(node.GetAttributeValue("name", null as string), "shiftModeId", StringComparison.OrdinalIgnoreCase))
                    .Select(node => new KeyValuePair<int, string>(
                        int.Parse(node.GetAttributeValue("value", null as string)),
                        GetDisplayValue(node)))
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Value));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the mode.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <returns>True if set mode successfully.</returns>
        /// <remarks>The available values are from the keys of <see cref="ListModes"/>.</remarks>
        public async Task<bool> SetMode(int mode)
        {
            if (await this.SetModeOneTry(mode))
            {
                return true;
            }

            await this.ListModes();
            await Task.Delay(1000);
            return await this.SetModeOneTry(mode);
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <returns>The collection of log records. Null if failed to get the log.</returns>
        public async Task<IEnumerable<string>> GetLog()
        {
            var response = await this.httpClient.GetAsync(new Uri($"{AdtDomain}{LogUrl}"));
            string html = await response.Content.ReadAsStringAsync();
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc.DocumentNode.Descendants("tr")
                    .Select(node => new { Node = node, Class = node.GetAttributeValue("class", null as string) })
                    .Where(tr => string.Equals(tr.Class, "p_rowLight", StringComparison.OrdinalIgnoreCase) || string.Equals(tr.Class, "p_rowDark", StringComparison.OrdinalIgnoreCase))
                    .Select(tr => tr.Node.InnerText);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets display value of a HTML node.
        /// </summary>
        /// <param name="node">The HTML node</param>
        /// <returns>The display value.</returns>
        /// <remarks>The display value is the text used for display. It's the text of the submit button within the same parent.</remarks>
        private static string GetDisplayValue(HtmlNode node)
        {
            return node.ParentNode.Elements("input")
                .First(n => string.Equals(n.GetAttributeValue("type", null as string), "submit", StringComparison.OrdinalIgnoreCase))
                .GetAttributeValue("value", null as string);
        }

        /// <summary>
        /// Gets the HTTP content used to set arm status.
        /// </summary>
        /// <param name="value">The arm status.</param>
        /// <returns>The HTTP content.</returns>
        private async Task<Dictionary<string, string>> GetArmContent(string value)
        {
            var response = await this.httpClient.GetAsync(new Uri($"{AdtDomain}{ListArmUrl}"));
            string html = await response.Content.ReadAsStringAsync();
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                Dictionary<string, string> values = doc.DocumentNode.Descendants("input")
                    .Select(node => new
                    {
                        Name = node.GetAttributeValue("name", null as string),
                        Value = node.GetAttributeValue("value", null as string)
                    })
                    .Where(input => !string.IsNullOrEmpty(input.Name))
                    .GroupBy(input => input.Name)
                    .ToDictionary(g => g.Key, g => g.First().Value);

                return new Dictionary<string, string>
                {
                    { "sat", values.ContainsKey("sat") ? values["sat"] : values["tsat"] },
                    { "fi", values["fi"] },
                    { "vn", values["vn"] },
                    { "ft", values["ft"] },
                    { "value", value },
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Tries to set mode for one time.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <returns>True if set mode successfully.</returns>
        private async Task<bool> SetModeOneTry(int mode)
        {
            var content = new Dictionary<string, string>
            {
                { "shiftModeId", $"{mode}" }
            };

            var response = await this.httpClient.PostAsync(
                new Uri($"{AdtDomain}{SetModeUrl}"),
                new HttpFormUrlEncodedContent(content));
            return response.Headers.Location.AbsoluteUri.EndsWith(SetModeSuccessSuffix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The class to represent the summary.
        /// </summary>
        public class Summary
        {
            /// <summary>
            /// Gets or sets the icon URL.
            /// </summary>
            public string IconUrl { get; set; }

            /// <summary>
            /// Gets or sets the current arm status.
            /// </summary>
            public string Arm { get; set; }

            /// <summary>
            /// Gets or sets the current mode.
            /// </summary>
            public string Mode { get; set; }
        }
    }
}
