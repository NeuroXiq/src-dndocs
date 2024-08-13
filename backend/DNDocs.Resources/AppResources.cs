namespace DNDocs.Resources
{
    public class AppResources
    {
        const string DocfxTemplates = "docfx-templates";
        const string DndocsDocfxScriptJs = "other/dndocs-docfx-script.js";
        const string CookiesConsentHtml = "other/cookies-consent.html";

        static readonly string AppResourcesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app-resources");
        public static readonly string DocfxInitDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app-resources", "docfx-init");

        static string OSPath(string path)
        {
            string ospath = Path.Combine(AppResourcesDir, path);

            return ospath;
        }

        public static string CookiesConsentHtmlContent
        {
            get
            {
                string ospath = OSPath(CookiesConsentHtml);
                if (!File.Exists(ospath)) throw new InvalidOperationException($"Could not find file: '{ospath}'");

                return File.ReadAllText(ospath);
            }
        }

        public static string DNDocsDocfxScriptJsContent
        {
            get
            {
                string ospath = Path.Combine(AppResourcesDir, DndocsDocfxScriptJs);
                if (!File.Exists(ospath)) throw new InvalidOperationException($"Could not find file: '{ospath}'");

                return File.ReadAllText(ospath);
            }
        }

        public static string DocfxTemplatesDirOSPath
        {
            get
            {
                string ospath = Path.Combine(AppResourcesDir, DocfxTemplates);
                if (!Directory.Exists(ospath)) throw new InvalidOperationException($"Could not find directory with resources: '{ospath}'");

                return ospath;
            }
        }
    }
}