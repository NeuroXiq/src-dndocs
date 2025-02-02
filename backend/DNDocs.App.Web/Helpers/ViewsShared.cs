namespace DNDocs.Web.Helpers
{
    public class ViewsShared
    {
        static readonly object _lock = new object();

        public const string FontAwesomeAll = "/lib/fontawesome-free-6.2.1-web/css/all.css";
        public const string HightlightJs = "/highlight/highlight.min.js";
        public const string HighlightThemeVS = "/lib/highlight/styles/vs.min.css";

        public static string[] SharedCss => sharedCss;
        public static string[] SharedJs => sharedJs;

        static string[] sharedJs, sharedCss;

        static ViewsShared()
        {
            sharedJs = GenerateSharedJs();
            sharedCss = GenerateSharedCss();
        }

        private static string[] GenerateSharedCss()
        {
            return new string[]
            {
                HighlightThemeVS,
                FontAwesomeAll,
                "~/css/shared/main.css",
                "~/css/shared/layout.css",
                "~/css/shared/vars.css",
                "~/css/shared/controls.css",
                "~/css/shared/form.css",
                "~/css/shared/md.css",
            };
        }

        private static string[] GenerateSharedJs()
        {
            var libs = new[]
            {
                HightlightJs
            };

            var helpers = new[]
            {
                "robiniaInit.js",
                "apiUrls.js",
                "rawFetch.js",
                "utils.js"
            };

            var controls = new[]
            {
                "globalMessage.js",
                "tabControl.js",
                "globalOverlay.js",
                "tableControl.js",
                "simpleTable.js",
                "simpleCard.js",
                "fileUploader.js",
                "contextMenu.js",
                "checkboxList.js",
                "drawer.js",

                "modalWindow.js",
                "formModalEdit.js",
                "confirmDialog.js",

                "nestedList.js",
                "treeView.js",

                "dropdown.js",
                "standardDropdown.js",
                "treeDropdown.js",

                "formControl.js",
            };

            var allJs = new List<string>();
            
            allJs.AddRange(WwwRootJs("~/lib", libs));
            allJs.AddRange(WwwRootJs("~/js/helpers", helpers));
            allJs.AddRange(WwwRootJs("~/js/controls", controls));

            return allJs.ToArray();
        }

        static string[] WwwRootJs(string folder, string[] jsFiles)
        {
            return jsFiles.Select(file => $"{folder}/{file}").ToArray();
        }

        

        public static string HtmlTitle(string suffix)
        {
            return $"RobiniaDocs - {suffix}";
        }
    }
}
