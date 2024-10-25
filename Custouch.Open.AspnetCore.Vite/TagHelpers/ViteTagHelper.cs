using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace Custouch.Open.AspnetCore.Vite.TagHelpers
{
    public class ViteTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment _env;
        public string Manifest { get; set; }
        public string Mainfile { get; set; }
        public bool Legacy { get; set; } = false;
        public bool WithImport { get; set; } = true;

        private string BaseDir = "";
        private ViteManifest manifest;

        private string Url(string file) => Path.Combine(BaseDir, file).Replace(@"\", "/");

        private static readonly Dictionary<string, string> _moduleScriptAttrs = new Dictionary<string, string>()
        {
            { "type", "module" }
        };

        private static readonly Dictionary<string, string> _noModuleScriptAttrs = new Dictionary<string, string>()
        {
            { "nomodule", "" }
        };

        public ViteTagHelper(IWebHostEnvironment env)
        {
            this._env = env;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            Mainfile ??= "src/main.ts";
            if (Manifest.StartsWith("http"))
            {
                await InitFromOrigin(Manifest);
            }
            else
            {
                var path = Path.Combine(_env.WebRootPath, Manifest);

                if (!File.Exists(path)) return;

                await Init(path);
            }


            if (manifest.TryGetValue(Mainfile, out var entry))
            {
                WriteAssert(output, entry, WithImport, _moduleScriptAttrs);
            }

            if (manifest.TryGetValue("vite/legacy-polyfills", out var polyfill))
            {
                output.Content.AppendHtml(Script(Url(polyfill.File), nomodule: true));
                var mainfilename = Path.GetFileNameWithoutExtension(Mainfile);
                var mainpolyfill = Mainfile.Replace(mainfilename, mainfilename + "-legacy");
                if (manifest.TryGetValue(mainpolyfill, out var _polyfill))
                {
                    WriteAssert(output, _polyfill, WithImport, _noModuleScriptAttrs);
                }
            }
        }

        private async Task InitFromOrigin(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string jsonContent = await response.Content.ReadAsStringAsync();
                    BaseDir = Manifest.Substring(0, Manifest.LastIndexOf('/') + 1);
                    manifest = ParseJsonString(jsonContent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"请求失败：{ex.Message}");
                }
            }
        }

        private async Task Init(string path)
        {
            BaseDir = Path.GetDirectoryName(Manifest);
            manifest = ParseJsonString(await File.ReadAllTextAsync(path));
        }

        private ViteManifest ParseJsonString(string text)
        {
            return JsonSerializer.Deserialize<ViteManifest>(text,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
        }

        void WriteAssert(TagHelperOutput output, ViteManifestItem item, bool withImport = true,
            Dictionary<string, string> scriptAttrs = null)
        {
            var css = item.Css;
            var file = item.File;
            if (css != null && css.Length > 0)
            {
                foreach (var _css in css)
                {
                    output.Content.AppendHtml(Link(Url(_css)));
                }
            }

            if (!string.IsNullOrWhiteSpace(file))
            {
                output.Content.AppendHtml(Script(Url(file), attrs: scriptAttrs));
            }

            if (withImport && item.Imports != null && item.Imports.Any())
            {
                foreach (var _import in item.Imports)
                {
                    if (manifest.TryGetValue(_import, out var _item))
                    {
                        WriteAssert(output, _item, withImport, scriptAttrs);
                    }
                }
            }
        }

        IHtmlContent Script(string path, string type = "", bool nomodule = false,
            Dictionary<string, string> attrs = null)
        {
            var tag = new TagBuilder("script");
            if (!string.IsNullOrWhiteSpace(type))
            {
                tag.Attributes.Add("type", type);
            }

            if (nomodule)
            {
                tag.Attributes.Add("nomodule", "");
            }

            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    tag.Attributes[attr.Key] = attr.Value;
                }
            }

            tag.Attributes.Add("src", GetValidSrc(path));
            tag.TagRenderMode = TagRenderMode.Normal;
            return tag;
        }

        IHtmlContent Link(string href)
        {
            var tag = new TagBuilder("link");
            tag.Attributes.Add("rel", "stylesheet");
            tag.Attributes.Add("href", GetValidSrc(href));
            tag.TagRenderMode = TagRenderMode.SelfClosing;
            return tag;
        }

        private string GetValidSrc(string src)
        {
            return src.StartsWith("http") ? src : $"/{src}";
        }
    }
}