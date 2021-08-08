using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace D365Plugin.autoCountStudentsNumber
{
    public class Proc_PostOperation : IPlugin
    {
        #region 定数
        // コンテキストメッセージ：作成
        private const string CONTEXT_MESSAGE_CREATE = "create";
        // コンテキストメッセージ：更新
        private const string CONTEXT_MESSAGE_UPDATE = "update";
        // プレーイメージ：アリエス名
        private const string PRE_IMAGE_ALIAS = "preImage";

        // テーブル名：学生台帳
        private const string TBL_STUDENT = "pas_tbl_student";
        // テーブル名：クラス管理
        private const string TBL_CLASS = "pas_tbl_class";

        // 学生台帳の列名：クラス
        private const string STUDENT_COL_CLASS = "pas_class";
        // クラス管理の列名：学生人数
        private const string CLASS_COL_STUDENT_NUMBER = "pas_student_number";
        #endregion

        #region メイン処理
        /// <summary>
        /// メイン処理
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Execute(IServiceProvider serviceProvider)
        {
            // トレースサービスの取得
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // 実行コンテキストの取得
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Targetプロパティが存在且つエンティティか
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // インプットパラメーターからtargetエンティティを取得
                Entity entity = (Entity)context.InputParameters["Target"];

                // 組織サービスを取得
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    #region 学生人数の自動計算処理はこちらです！
                    // トレースログ：処理開始
                    tracingService.Trace("処理開始：学生人数の自動計算処理...");

                    // コンテキストメッセージの取得（小文字）
                    string contextMessageName = context.MessageName.ToLower();

                    // 変更前【学生台帳】の「クラス名」のGUID
                    Guid studentBeforeClass = new Guid();
                    // 変更後【学生台帳】の「クラス名」のGUID
                    Guid studentAfterClass = new Guid();

                    // 変更前【クラス】の「学生人数」
                    int classBeforeNumber = 0;
                    // 変更後【クラス】の「学生人数」
                    int classAfterNumber = 0;

                    ///<summary>
                    /// 1. コンテキストメッセージが作成（Create）かまたは更新（Update）のいずれではないか
                    ///   1.1 真：処理を終了（対象メッセージではない） 
                    ///   1.2 偽：次の処理へ
                    /// </summary>
                    if (!(contextMessageName == CONTEXT_MESSAGE_CREATE || contextMessageName == CONTEXT_MESSAGE_UPDATE))
                    {
                        return;
                    }

                    // 2. クラスが更新された時
                    if (contextMessageName == CONTEXT_MESSAGE_UPDATE)
                    {
                        // トレースログ
                        tracingService.Trace("開始：変更前「クラス名」の取得処理...");

                        // 2.1 プレイメージプロパティが存在且つエンティティか
                        if (context.PreEntityImages.Contains(PRE_IMAGE_ALIAS) && context.PreEntityImages[PRE_IMAGE_ALIAS] is Entity)
                        {
                            // プレイメージを取得
                            Entity preImageEntity = (Entity)context.PreEntityImages[PRE_IMAGE_ALIAS];
                            tracingService.Trace($"{PRE_IMAGE_ALIAS}の取得が成功した。");

                            // 【変更前「クラス名」のID取得処理】
                            studentBeforeClass = GetColLookUpValue(preImageEntity, STUDENT_COL_CLASS);
                            
                            // トレースログ
                            tracingService.Trace($"変更前「クラス名」の取得が成功した。GUID:[{studentBeforeClass}]");
                        }

                        // トレースログ
                        tracingService.Trace("終了：変更前「クラス名」の取得処理...");
                    }

                    // 3. 新規作成または更新時、クラス名が存在するか
                    if (entity.Attributes.Contains(STUDENT_COL_CLASS) && entity[STUDENT_COL_CLASS] != null)
                    {
                        tracingService.Trace("開始：変更後「クラス名」の取得処理...");

                        // 3.1 【変更後「クラス名」のID取得処理】
                        studentAfterClass = GetColLookUpValue(entity, STUDENT_COL_CLASS);

                        // トレースログ
                        tracingService.Trace($"作成また変更後「クラス名」の取得が成功した。GUID:[{studentAfterClass}]");
                        tracingService.Trace("終了：変更後「クラス名」の取得処理...");
                    }

                    // 4. 変更前「クラス名」が存在か
                    if (studentBeforeClass != new Guid())
                    {
                        // トレースログ
                        tracingService.Trace("開始：変更前クラスの「学生人数」の再設定処理...");

                        // 4.1 変更前クラスの学生人数を自動計算
                        classBeforeNumber = GetStudentNumber(service, studentBeforeClass);
                        // 4.2 変更前クラスを取得
                        Entity beforeClassEntity = new Entity(TBL_CLASS, studentBeforeClass);
                        // 4.3 学生人数を再設定
                        beforeClassEntity[CLASS_COL_STUDENT_NUMBER] = classBeforeNumber;
                        // 4.4 変更前クラスを更新
                        service.Update(beforeClassEntity);

                        // トレースログ
                        tracingService.Trace("終了：変更前クラスの「学生人数」の再設定処理...");
                    }

                    // 5. 変更後「クラス名」が存在か
                    if (studentAfterClass != new Guid())
                    {
                        // トレースログ
                        tracingService.Trace("開始：変更後クラスの「学生人数」の再設定処理...");

                        // 5.1 変更後クラスの学生人数を自動計算
                        classAfterNumber = GetStudentNumber(service, studentAfterClass);
                        // 5.2 変更後クラスを取得
                        Entity afterClassEntity = new Entity(TBL_CLASS, studentAfterClass);
                        // 5.3 学生人数を再設定
                        afterClassEntity[CLASS_COL_STUDENT_NUMBER] = classAfterNumber;
                        // 5.4 変更後クラスを更新
                        service.Update(afterClassEntity);

                        // トレースログ
                        tracingService.Trace("終了：変更後クラスの「学生人数」の再設定処理...");
                    }

                    // トレースログ：処理終了
                    tracingService.Trace("処理終了：学生人数の自動計算処理...");
                    #endregion
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("エラーが発生した。", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("D365Plugin.autoCountStudentsNumber: {0}", ex.ToString());
                    throw;
                }
            }
        }
        #endregion

        #region 検索項目のID取得処理
        /// <summary>
        /// 検索項目のID取得処理
        /// </summary>
        /// <param name="paraTable">テーブル：エンティティ</param>
        /// <param name="paraColName">列名：文字列</param>
        /// <returns>検索項目のID：GUID</returns>
        private Guid GetColLookUpValue(Entity paraTable, string paraColName)
        {
            // 返却用変数の宣言
            Guid ColValue = new Guid();

            // 列が存在するか
            if (paraTable.Attributes.Contains(paraColName) && paraTable[paraColName] != null)
            {
                // 検索項目のID取得
                ColValue = paraTable.GetAttributeValue<EntityReference>(paraColName).Id;
            }
            
            // 返却
            return ColValue;
        }
        #endregion

        #region 学生人数の取得
        /// <summary>
        /// 学生人数の取得
        /// </summary>
        /// <param name="paraService">組織サービス</param>
        /// <param name="paraFieldValue">フィールド値：GUID/param>
        /// <returns>結果件数</returns>
        private int GetStudentNumber(IOrganizationService paraService, Guid paraClassGuid)
        {
            // 検索用クエリ
            QueryExpression query = new QueryExpression();
            // 対象エンティティ：【学生台帳】
            query.EntityName = TBL_STUDENT;
            // 結果列（既定以外は不要）
            query.ColumnSet = new ColumnSet(false);

            // フィルター条件：「クラス名」が指定されたGUIDと一致
            FilterExpression qfilt = new FilterExpression(LogicalOperator.And);
            qfilt.Conditions.Add(new ConditionExpression(STUDENT_COL_CLASS, ConditionOperator.Equal, paraClassGuid));
            query.Criteria = qfilt;

            // 検索を実行
            EntityCollection entityClt = paraService.RetrieveMultiple(query);

            // 結果件数を返却
            return entityClt.Entities.Count();
        }
        #endregion
    }
}
