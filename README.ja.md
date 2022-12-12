# UnityShaderVariantLoggerForEditor
UnityEditor上で実行した時に、Shaderコンパイルをログに書き出してます。<br />
そのログをもとにイイ感じにShaderVariantCollectionを作れるようにします。

対応は、Windows 10 / Mac (Intel MacOS12.0でテスト)

また姉妹ツールの [ProfilerModuleForShaderCompile](https://github.com/wotakuro/ProfilerModuleForShaderCompile/)もあります。
こちらは ShaderCompile情報に特化したProfilerModuleを提供しています。

# 利用方法

メニューの 「Tools/UTJ/ShaderVariantLogger」でWindowを開きます<br />


## 1.Shaderコンパイルのログ収集について

![Screenshot](Document~/img/General.png "Screenshot")<br />

画面の「Enabled」を有効にします。
この状態でEditor上でプレイをしていると、Editor上でのShaderコンパイルログが「Library/com.utj.shadervariantlogger/logs」以下に蓄積されます<br />
このログにアクセスしたいときは「Open Directory」を押すことでログが溜まっているディレクトリにアクセスすることができます。<br />

後にこのログをもとにShaderVariantCollectionアセットを作成することが可能です。<br />
<br />
<br />
Editor上でプレイをするごとにShaderCacheを消します。<br />
毎回削除されるのを止めたい場合は 「Clear ShaderCache」のチェックを外してください。<br />
※ただしチェックを外すと、Shaderコンパイルを取りこぼす可能性があります。

## 2.ログからVariantCollectionを作成する

![Screenshot](Document~/img/CreateAsset.png "Screenshot")<br />

「Add Variants from logs」を押すことで実行します。

このとき、ログに溜まった内容をもとに「ShaderVariantCollection」で指定されたShaderVariantCollectionに対してVariant追加を試みます。<br />
もし指定がないときには新規にファイルを作成します。

「Delete logs after adding」にチェックを入れると、処理完了後にもともとあったログファイルを削除します。
「Shader Path Config Advanced」では、Variantに追加するShaderの条件を指定することが可能です。
