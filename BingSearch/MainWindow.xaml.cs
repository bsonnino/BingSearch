using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BingSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            WebPage.Navigated += (s, e) => SetSilent(WebPage, true);
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", 
                        BindingFlags.Instance | BindingFlags.Public | 
                        BindingFlags.PutDispProperty, null, webBrowser, 
                        new object[] { silent });
                }
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), 
            InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, 
                [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
        private async void SearchClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtSearch.Text))
                return;
            var queryString = WebUtility.UrlEncode(TxtSearch.Text);
            var htmlWeb = new HtmlWeb();
            var query = $"https://bing.com/search?q={queryString}&count=100";
            var doc = await htmlWeb.LoadFromWebAsync(query);
            var response = doc.DocumentNode.SelectSingleNode("//ol[@id='b_results']");
            var results = response.SelectNodes("//li[@class='b_algo']");
            if (results == null)
            {
                LbxResults.ItemsSource = null;
                return;
            }
            var searchResults = new List<SearchResult>();
            foreach (var result in results)
            {
                var refNode = result.Element("h2").Element("a");
                var url = refNode.Attributes["href"].Value;
                var text = refNode.InnerText;
                var description = WebUtility.HtmlDecode(
                    result.Element("div").Element("p").InnerText);
                searchResults.Add(new SearchResult(text, url, description));
            }
            LbxResults.ItemsSource = searchResults;
        }

        private void LinkNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        private void LinkChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems?.Count > 0)
            {
                WebPage.Navigate(((SearchResult)e.AddedItems[0]).Url);
            }
        }

       
    }
}
