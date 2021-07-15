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

namespace Custouch.Open.AspnetCore.Vite.TagHelpers
{
    public class ViteTagHelper : TagHelper
    {
        private readonly IWebHostEnvironment _env;
        public string Manifest { get; set; }
        public string Mainfile { get; set; }
        public bool Legacy { get; set; } = false;
        public ViteTagHelper(IWebHostEnvironment env)
        {
            this._env = env;
        }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;
            Mainfile ??= "src/main.ts";
            
            var path = Path.Combine(_env.WebRootPath, Manifest);
            
            if (File.Exists(path))
            {
                var dir = Path.GetDirectoryName(Manifest);
                string _url(string file) => Path.Combine(dir, file);
                var manifest = JsonSerializer.Deserialize<ViteManifest>(await File.ReadAllTextAsync(path), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                if (manifest.TryGetValue(Mainfile, out var entry))
                {
                    var css = entry.Css;
                    var file = entry.File;
                    if (css != null && css.Length > 0)
                    {
                        foreach (var _css in css)
                        {
                            output.Content.AppendHtml(ViteAssertLoader.Link(_url(_css)));
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(file))
                    {

                        output.Content.AppendHtml(ViteAssertLoader.Script(_url(file), type:"module"));
                    }
                }
                if (Legacy)
                {
                    if (manifest.TryGetValue("vite/legacy-polyfills", out var polyfill))
                    {
                        output.Content.AppendHtml(ViteAssertLoader.Script(_url(polyfill.File), nomodule: true));
                    }
                    var mainfilename = Path.GetFileNameWithoutExtension(Mainfile);
                    var mainpolyfill = Mainfile.Replace(mainfilename, mainfilename + "-legacy");
                    if (manifest.TryGetValue(mainpolyfill, out var _polyfill))
                    {
                        output.Content.AppendHtml(ViteAssertLoader.Script(_url(_polyfill.File),nomodule: true));
                    }
                }
            }
        }
    }
    public static class ViteAssertLoader
    {
        public static IHtmlContent Script(string path,string type="", bool nomodule = false, Dictionary<string, string> attrs = null)
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
                    tag.Attributes.Add(attr);
                }
            }
            tag.Attributes.Add("src", $"\\{path}");
            tag.TagRenderMode = TagRenderMode.Normal;
            return tag;
        }
        public static IHtmlContent Link(string href)
        {
            var tag = new TagBuilder("link");
            tag.Attributes.Add("rel", "stylesheet");
            tag.Attributes.Add("href", $"\\{href}");
            tag.TagRenderMode = TagRenderMode.SelfClosing;
            return tag;
        }
    }
}
