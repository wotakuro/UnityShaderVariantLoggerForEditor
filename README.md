# UnityShaderVariantLoggerForEditor
UnityEditor上で実行した時に、Shaderコンパイルをログに書き出してます。<br />
そのログをもとにイイ感じにShaderVariantCollectionを作れるようにします。

# 利用方法
メニューの 「Tools/UTJ/ShaderVariantLogger/Enable」にチェックを入れます。<br />
その状態でEditor上でプレイをしていると、Editor上でのコンパイルログが「Library/com.utj.shadervariantlogger/logs」以下に蓄積されます<br />
<br />
このログをもとにShaderVariantCollectionアセットを作成することが可能です。<br />
<br />
※注意点：Editor上でプレイをするごとにShaderCacheを消します。
　　　　　要注意。