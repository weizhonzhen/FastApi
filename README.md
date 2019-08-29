# FastApi 
动态生成读api，只需配数据库连接及xml文件

1、ConfigureServices
services.AddTransient<IFastApi, FastApi>();//注入
FastMap.InstanceMap();//读取xml

2、Configure
app.UseMiddleware<FastApiHandler>();//使用中间件
 
3、配配数据库连接 db.json
{
  "DataConfig": [
    {
      "ProviderName": "Oracle.ManagedDataAccess",
      "DbType": "Oracle",
      "ConnStr": "User Id=test;Password=test;Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=test)));",
      "IsOutSql": false,
      "IsOutError": true,
      "IsPropertyCache": true,
      "IsMapSave": false,
      "Flag": ":",
      "FactoryClient": "Oracle.ManagedDataAccess.Client.OracleClientFactory",
      "Key": "Api",
      "DesignModel": "DbFirst",
      "SqlErrorType": "file",
      "CacheType": "web",
      "IsUpdateCache": true
    }
  ]
}

4、配置xml读取map.json
{
  "SqlMap": { "Path": [ "map/Patient.xml" ] }
}

5、配置xml和成接口
<?xml version="1.0" encoding="utf-8" ?>
<sqlMap>
   <select id="testurl" db="Api">
    select * from table a
    <dynamic prepend=" where 1=1 ">
      <isNotNullOrEmpty prepend=" and " property="name">a.name = :name</isNotNullOrEmpty>      
      <isNotNullOrEmpty prepend=" and " property="id">a.id = :id</isNotNullOrEmpty>
    </dynamic>
 </select>
</sqlMap

访问的地址：http://127.0.0.1/testurl?name=aa&id=1
支持post get 等所有方式请求

