# Custouch.Open.AspnetCore.Vite

用于Razor页面集成Vite资源。

## 安装 

使用 Nuget 或者 dotnet cli 安装：

    dotnet add package Custouch.Open.AspnetCore.Vite 

## 使用 

1. 在Vite项目中需要指定输出manifest.json

```js
// vite.config.js
export default {
  build: {
    // 在 outDir 中生成 manifest.json
    manifest: true,
    rollupOptions: {
      // 覆盖默认的 .html 入口
      input: '/path/to/main.js'
    }
  },
  // 指定资源在生产环境的路径前缀
  base: "/static" 
}
``` 

2. 将构建产物放置在 asp.net项目的 wwwroot文件夹下面
3. 在Razor页面中引入vite标签

```html
@addTagHelper *, Custouch.Open.AspnetCore.Vite 

<head>
...
  <vite manifest="static/manifest.json" legacy="true" mainfile="src/main.ts"></vite>
...
</head>
```
参数说明:

|参数|必填|说明|
|:---:|:--|:---|
|manifest|是|manifest.json 文件的地址（wwwroot内相对路径）|
|legacy|否|是否需要支持legacy，vite项目需要安装和配置`@vitejs/plugin-legacy`插件|
|mainfile|否|主要入口文件，默认`src/main.ts`|

## 参考

1. [Vite 后端集成](https://cn.vitejs.dev/guide/backend-integration.html)
2. [Tag Helpers in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/intro?view=aspnetcore-3.1)