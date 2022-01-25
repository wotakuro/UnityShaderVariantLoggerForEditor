# UnityShaderVariantLoggerForEditor
[日本語はコチラ](README.ja.md)<br />

Logging the ShaderCompile when running on Editor.
And then generate the ShadervariantCollection from the "ShaderCompile log".


(Currently WindowsOnly)

# How to use

Call "Tools/UTJ/ShaderVariantLogger" and open this window.<br />

![Screenshot](Document~/img/VariantLoggerWindow.png "Screenshot")<br />


## 1.logging "Shader Compile"

If you enabled "1.Enabled", you play on the UnityEditor and then the "ShaderCompiling log" will be generated in "Library/com.utj.shadervariantlogger/logs"<br />
If you want to access the log , press the "6.Open Directory".

Also you can generate ShaderVariantCollection by using this log.<br />
<br />
※Warning:When you play on the Editor,ShaderCache will be deleted every time.

## 2.Generate ShaderVariantCollection from log.

Press "5.Add Variants from logs".

You can select ShaderVariantCollection Asset that you want to add ShaderVariant by selecting "2.ShaderVariantCollection".
If "2.ShaderVariant Collection" is none, new file will be generated.

If you enabled "3.Delete logs after adding",the ShaderCompile logs will be deleted.
Also you can configurate which Shader should be added by "4.Shader Path Config Advanced".

