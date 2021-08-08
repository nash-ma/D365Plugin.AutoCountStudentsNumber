# ■D365Plugin.autoCountStudentsNumber
## ①学生が新規作成の時、「クラス名」が設定済の場合のみ、対象クラスの学生人数を計算してクラスの項目「学生人数」を更新する。　　　　　　　　　　
## ②「クラス名」が更新の時、変更前「クラス名」と変更後「クラス名」の値を取得し、それぞれの学生人数を計算して項目「学生人数」を更新する。

![学生台帳](20210809_041343.png "学生台帳")
![クラス管理](20210809_041525.png "クラス管理")

# テーブル：学生台帳（new_tbl_student）
| 表示名 | 名前 | データタイプ | 必須 | 備考1 | 備考2 |
|:---:|:---:|:---:|:---:|:---:|:---:|
|学生番号 |pas_student_no |一行テキスト |● |オートナンバー | | 
|クラス |pas_class |検索 |● |参照先：クラス管理 | | 

# テーブル：クラス管理（new_tbl_class）
| 表示名 | 名前 | データタイプ | 必須 | 備考1 | 備考2 |
|:---:|:---:|:---:|:---:|:---:|:---:|
|クラス番号 |pas_class_no |一行テキスト |● | | | 
|学生人数 |pas_student_number |整数 | |範囲：0～25 | | 

# 設計内容：
- トリガー：学生の新規作成または項目「クラス名」の更新
- 処理：
  - 学生が新規作成の時、「クラス名」が設定済の場合のみ、対象クラスの学生人数を計算してクラスの項目「学生人数（自動）」を更新する。
  - 「クラス名」が更新の時、変更前「クラス名」と変更後「クラス名」の値を取得し、それぞれの学生人数を計算して項目「学生人数（自動）」を更新する。
- 登録内容：
  - メッセージ：Create （作成）・Update（更新）
  - 主エンティティ名：エンティティ【学生台帳】のロジック名
  - フィルター条件：項目「クラス名」のロジック名（Updateのみ）
  - 実行者：CallingUser
  - 実行順番：1
  - 実行ステージ：PostOperation
  - 実行モード：同期
  - プレイメージ：アリエス名が「preImage」、項目が「クラス名」

# ■プラグイン登録内容

## アセンブリの登録
![アセンブリの登録](登録内容001.png "アセンブリの登録")
![アセンブリの登録](登録内容002.png "アセンブリの登録")
![アセンブリの登録](登録内容003.png "アセンブリの登録")
![アセンブリの登録](登録内容004.png "アセンブリの登録")
![アセンブリの登録](登録内容005.png "アセンブリの登録")

## ステップ：作成（Create）
![ステップ：作成](登録内容_作成001.png "ステップ：作成")

## ステップ：更新（Update）
![ステップ：更新](登録内容_更新001.png "ステップ：更新")
![ステップ：更新](登録内容_更新002.png "ステップ：更新")
![ステップ：更新](登録内容_更新003.png "ステップ：更新")

## プレイメージ
![プレイメージ](登録内容_プレイメージ001.png "プレイメージ")
![プレイメージ](登録内容_プレイメージ002.png "プレイメージ")
![プレイメージ](登録内容_プレイメージ003.png "プレイメージ")

# 登録済の状態
![登録済の状態](登録内容999.png "登録済の状態")

