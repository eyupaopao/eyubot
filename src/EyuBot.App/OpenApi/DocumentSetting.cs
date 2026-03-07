using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace IPCAS.Controllers.Options;

/// <summary>
/// 文档网页配置
/// </summary>
public class DocumentSetting
{
    public bool EnableDocument { get; set; }
    /// <summary>
    /// 文档标题
    /// </summary>
    public string DocumentTitle { get; set; }
    
    //public string RouteTemplate { get; set; } = "swagger/{documentName}/swagger.json";
    /// <summary>
    /// 默认分组名
    /// </summary>
    public string DefaultGroupName { get; set; }

    /// <summary>
    /// 启用授权支持
    /// </summary>
    public bool? EnableAuthorized { get; set; }
    /// <summary>
    /// 格式化为V2版本
    /// </summary>
    public bool FormatAsV2 { get; set; }

    /// <summary>
    /// 配置规范化文档地址
    /// </summary>
    public string RoutePrefix { get; set; }
    /// <summary>
    /// 文档展开设置
    /// </summary>
    public DocExpansion DocExpansionState { get; set; } = default!;
    /// <summary>
    /// 显示请求时间
    /// </summary>
    public bool DisplayRequestDuration { get; set; }

    /// <summary>
    /// XML 描述文件
    /// </summary>
    public string[] XmlComments { get; set; }

    /// <summary>
    /// 是否自动加载 Xml 注释文件
    /// </summary>
    public bool? EnableXmlComments { get; set; }
    /// <summary>
    /// 配置分组
    /// </summary>
    public List<GroupOpenApiInfo> GroupOpenApiInfos { get; set; }
}

/// <summary>
/// 配置分组
/// </summary>
public class GroupOpenApiInfo : OpenApiInfo
{
    /// <summary>
    /// 分组名字
    /// </summary>
    public string Group { get; set; }
    /// <summary>
    /// 顺序
    /// </summary>
    public int Order { get; set; }
}

/*
   "DocumentTitle": "Swan | IPCAS",
    "RoutePrefix": "ipcas",
    "LoginInfo": {
      "Enabled": true
    },
    "DefaultGroupName": "user",
    "GroupOpenApiInfos": [
      {
        "Group": "user",
        "Title": "用户",
        "Order": 20,
        "Version": "2.0.0"
      },
      {
        "Group": "term",
        "Title": "终端",
        "Order": 19,
        "Version": "2.0.0"
      },   
]}
}
*/