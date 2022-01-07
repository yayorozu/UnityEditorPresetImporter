# Unity Editor Preset Importer

Preset は現状 Reset 時にしか適応されないのと、対象となるパスの指定が面倒なので

ディレクトリを指定して、Reimport 時に反映されるようにしたツール

※ 特定の Version で Reimport 時に Preset を適応すると 「Unapplied import settings」と表示されるバグがある

# 設定

Project Settings に Preset Importer が追加されているので選択する

「Add Importer Preset」 を押して、Asset Import の設定を行っている Preset を選択

選択すると下に表示されるので「＋」を押して対象のフォルダを追加する

この際に対象のフォルダ以下にいた場合に変更が適応され、 他の Preset も対象に入っていた場合階層が浅い順に適応されていく
