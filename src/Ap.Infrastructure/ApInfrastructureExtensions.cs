namespace Ap.Infrastructure
{
    /// <summary>
    /// 提供底层通用的扩展方法
    /// </summary>
    public static class ApInfrastructureExtensions
    {
        /// <summary>
        /// 设置Operator属性值
        /// </summary>
        /// <typeparam name="THavingOperator"></typeparam>
        /// <param name="target">实现IHavingOperator类的实例</param>
        /// <param name="operatorProvider">实现IOperatorProvider类的实例</param>
        /// <returns></returns>
        public static THavingOperator FillOperator<THavingOperator, TOperatorProvider>(this THavingOperator target, TOperatorProvider operatorProvider) where THavingOperator : IHavingOperator where TOperatorProvider : IOperatorProvider
        {
            if (null != target)
            {
                target.Operator = operatorProvider.GetOperator();
            }
            return target;
        }
    }
}
