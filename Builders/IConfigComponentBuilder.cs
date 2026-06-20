namespace SubConvert.Builders;

public interface IConfigComponentBuilder
{
    /// <summary>
    /// 执行具体的配置构建工序，将结果或中间状态写入上下文。
    /// </summary>
    void Build(BuildContext context);
}