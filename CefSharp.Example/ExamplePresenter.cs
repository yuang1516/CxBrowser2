﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace CefSharp.Example
{
    public class ExamplePresenter : IRequestHandler, ICookieVisitor
    {
        public static void Init()
        {
            Settings settings = new Settings();
            
            settings.Locale = "es";
            
            if (CEF.Initialize(settings))
            {
                CEF.RegisterScheme("test", new SchemeHandlerFactory());
                CEF.RegisterJsObject("bound", new BoundObject());
            }
        }

        private const string home_url = "http://cxbrowser.sourceforge.net";
        private const string resource_url = "http://test/resource/load";
        private const string scheme_url = "test://test/SchemeTest.html";
        private const string bind_url = "test://test/BindingTest.html";
        private const string tooltip_url = "test://test/TooltipTest.html";
        private const string popup_url = "test:/test/PopupTest.html";

        private int color_index = 0;
        private readonly string[] colors =
        {
            "red",
            "blue",
            "green",
        };

        private readonly IWebBrowser model;
        private readonly IExampleView view;
        private readonly Action<Action> gui_invoke;

        public ExamplePresenter(IWebBrowser model, IExampleView view,
            Action<Action> gui_invoke)
        {
            this.model = model;
            this.view = view;
            this.gui_invoke = gui_invoke;

            var version = String.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}",
                CEF.ChromiumVersion, CEF.CefVersion, CEF.CefSharpVersion);
            view.DisplayOutput(version);

            model.RequestHandler = this;
            model.PropertyChanged += model_PropertyChanged;
            model.ConsoleMessage += model_ConsoleMessage;
            //model.LifeSpanHandler = this;

            // file
            view.ShowDevToolsActivated += view_ShowDevToolsActivated;
            view.CloseDevToolsActivated += view_CloseDevToolsActivated;
            view.ExitActivated += view_ExitActivated;

            // edit
            view.UndoActivated += view_UndoActivated;
            view.RedoActivated += view_RedoActivated;
            view.CutActivated += view_CutActivated;
            view.CopyActivated += view_CopyActivated;
            view.PasteActivated += view_PasteActivated;
            view.DeleteActivated += view_DeleteActivated;
            view.SelectAllActivated +=  view_SelectAllActivated;

            // test
            view.TestResourceLoadActivated += view_TestResourceLoadActivated;
            view.TestSchemeLoadActivated += view_TestSchemeLoadActivated;
            view.TestExecuteScriptActivated += view_TestExecuteScriptActivated;
            view.TestEvaluateScriptActivated += view_TestEvaluateScriptActivated;
            view.TestBindActivated += view_TestBindActivated;
            view.TestConsoleMessageActivated += view_TestConsoleMessageActivated;
            view.TestTooltipActivated += view_TestTooltipActivated;
            view.TestPopupActivated += view_TestPopupActivated;
            view.TestLoadStringActivated += view_TestLoadStringActivated;
            view.TestCookieVisitorActivated += view_TestCookieVisitorActivated;

            // navigation
            view.UrlActivated += view_UrlActivated;
            view.ForwardActivated += view_ForwardActivated;
            view.BackActivated += view_BackActivated;
        }

        private void model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string @string = null;
            bool @bool = false;

            switch (e.PropertyName)
            {
                case "IsBrowserInitialized":
                    if (model.IsBrowserInitialized)
                    {
                        //model.Load(home_url);
                    }
                    break;
                case "Title":
                    @string = model.Title;
                    gui_invoke(() => view.SetTitle(@string));
                    break;
                case "Address":
                    @string = model.Address;
                    gui_invoke(() => view.SetAddress(@string));
                    break;
                case "CanGoBack":
                    @bool = model.CanGoBack;
                    gui_invoke(() => view.SetCanGoBack(@bool));
                    break;
                case "CanGoForward":
                    @bool = model.CanGoForward;
                    gui_invoke(() => view.SetCanGoForward(@bool));
                    break;
                case "IsLoading":
                    @bool = model.IsLoading;
                    gui_invoke(() => view.SetIsLoading(@bool));
                    break; 
            }
        }

        private void model_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            gui_invoke(() => view.DisplayOutput(e.Message));
        }

        private void view_ShowDevToolsActivated(object sender, EventArgs e)
        {
            model.ShowDevTools();
        }

        private void view_CloseDevToolsActivated(object sender, EventArgs e)
        {
            model.CloseDevTools();
        }

        private void view_ExitActivated(object sender, EventArgs e)
        {
            model.Dispose();
            CEF.Shutdown();
            System.Environment.Exit(0);
        }

        void view_UndoActivated(object sender, EventArgs e)
        {
            model.Undo();
        }

        void view_RedoActivated(object sender, EventArgs e)
        {
            model.Redo();
        }

        void view_CutActivated(object sender, EventArgs e)
        {
            model.Cut();
        }

        void view_CopyActivated(object sender, EventArgs e)
        {
            model.Copy();
        }

        void view_PasteActivated(object sender, EventArgs e)
        {
            model.Paste();
        }

        void view_DeleteActivated(object sender, EventArgs e)
        {
            model.Delete();
        }

        void view_SelectAllActivated(object sender, EventArgs e)
        {
            model.SelectAll();
        }

        private void view_TestResourceLoadActivated(object sender, EventArgs e)
        {
            model.Load(resource_url);
        }

        private void view_TestSchemeLoadActivated(object sender, EventArgs e)
        {
            model.Load(scheme_url);
        }

        private void view_TestExecuteScriptActivated(object sender, EventArgs e)
        {
            var script = String.Format("document.body.style.background = '{0}'",
                colors[color_index++]);
            if (color_index >= colors.Length)
            {
                color_index = 0;
            }

            view.ExecuteScript(script);
        }

        private void view_TestEvaluateScriptActivated(object sender, EventArgs e)
        {
            var rand = new Random();
            var x = rand.Next(1, 10);
            var y = rand.Next(1, 10);

            var script = String.Format("{0} + {1}", x, y);
            var result = view.EvaluateScript(script);
            var output = String.Format("{0} => {1}", script, result);

            gui_invoke(() => view.DisplayOutput(output));
        }

        private void view_TestBindActivated(object sender, EventArgs e)
        {
            model.Load(bind_url);
        }

        private void view_TestConsoleMessageActivated(object sender, EventArgs e)
        {
            var script = "console.log('Hello, world!')";
            view.ExecuteScript(script);
        }

        private void view_TestTooltipActivated(object sender, EventArgs e)
        {
            model.Load(tooltip_url);
        }

        private void view_TestPopupActivated(object sender, EventArgs e)
        {
            model.Load(popup_url);
        }

        private void view_TestLoadStringActivated(object sender, EventArgs e)
        {
            model.LoadHtml(string.Format("<html><body><a href='{0}'>CefSharp Home</a></body></html>", home_url));
        }

        private void view_TestCookieVisitorActivated(object sender, EventArgs e)
        {
            CEF.VisitAllCookies(this);
        }

        private void view_UrlActivated(object sender, string url)
        {
        	model.Load(url);
        }

        private void view_BackActivated(object sender, EventArgs e)
        {
            model.Back();
        }

        private void view_ForwardActivated(object sender, EventArgs e)
        {
            model.Forward();
        }
        

        #region IRequestHandler Members

        bool IRequestHandler.OnBeforeBrowse(IWebBrowser browser, IRequest request, NavigationType naigationvType, bool isRedirect)
        {
            return false;
        }

        bool IRequestHandler.OnBeforeResourceLoad(IWebBrowser browser, IRequestResponse requestResponse)
        {
            /*IRequest request = requestResponse.Request;
            if (request.Url.StartsWith(resource_url))
            {
                Stream resourceStream = new MemoryStream(Encoding.UTF8.GetBytes(
                    "<html><body><h1>Success</h1><p>This document is loaded from a System.IO.Stream</p></body></html>"));
                requestResponse.RespondWith(resourceStream, "text/html");
            }*/
            
            //Discover:
            /*IDictionary<string, string> p;
            Dictionary<string, string> q = new Dictionary<string, string>();
            string i, f;            
            p = requestResponse.Request.GetHeaders();            
            foreach (var par in p)
			{
				i = par.Key;
				f = par.Value;
				if (i=="User-Agent")
				{
					q.Add(i,"Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)");
				}
				else
				{
					q.Add(i,f);
				}
			}*/
            
            //Default User-Agent : Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/536.5 (KHTML, like Gecko) Chrome/19.0.1084.9 Safari/536.5
            //Kindle Fire: Mozilla/5.0 (Linux; U; Android 2.3.4; en-us; Kindle Fire Build/GINGERBREAD) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1
            //Internet Explorer 9: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 7.1; Trident/5.0)
            //Ipad2: Mozilla/5.0 (iPad; CPU OS 5_1 like Mac OS X; en-us) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B176 Safari/7534.48.3
            
            Dictionary<string, string> q = new Dictionary<string, string>();
            q.Add("User-Agent","Mozilla/5.0 (iPad; CPU OS 5_1 like Mac OS X; en-us) AppleWebKit/534.46 (KHTML, like Gecko) Version/5.1 Mobile/9B176 Safari/7534.48.3");
            
            requestResponse.Request.SetHeaders(q);

            return false;
        }

        void IRequestHandler.OnResourceResponse(IWebBrowser browser, string url, int status, string statusText, string mimeType, WebHeaderCollection headers)
        {

        }

        #endregion

        #region ICookieVisitor Members

        bool ICookieVisitor.Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            Console.WriteLine("Cookie #{0}: {1}", count, cookie.Name);
            return true;
        }

        #endregion
        
        #region ILifeSpanHandler Members
		/*bool ILifeSpanHandler.OnBeforePopup(CefSharp.IWebBrowser fwb, string url, ref int x, ref int y, ref int width, ref int height)
		{
			url = "";
			return false;
		}*/
        #endregion
    }
}