# FastApi 
动态生成读api，只需配数据库连接及xml文件

1、ConfigureServices
```csharp
services.AddTransient<IFastApi, FastApi>();//注入
FastMap.InstanceMap();//读取xml
```

2、Configure
```csharp
app.UseMiddleware<FastApiHandler>();//使用中间件
```
 
3、配配数据库连接 db.json
```csharp
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
```

4、配置xml读取map.json
```csharp
{
  "SqlMap": { "Path": [ "map/Patient.xml" ] }
}
```

5、配置xml和成接口
```csharp
<?xml version="1.0" encoding="utf-8" ?>
<sqlMap>
   <select id="testurl" db="Api" type="param">
    select * from table a
    <dynamic prepend=" where 1=1 ">
      <isNotNullOrEmpty prepend=" and " property="name">a.name = :name</isNotNullOrEmpty>      
      <isNotNullOrEmpty prepend=" and " property="id">a.id = :id</isNotNullOrEmpty>
    </dynamic>
 </select>
 
  <insert id="Write/Test" db="Api" type="write">
    insert into aa values (
    <dynamic prepend="">
      <isPropertyAvailable prepend="" property="id" existsmap="CheckTestId">:id,</isPropertyAvailable>
      <isPropertyAvailable prepend="" property="addTime" date="true" required="true">:addTime,</isPropertyAvailable>
      <isPropertyAvailable prepend="" property="key">:key,</isPropertyAvailable>
      <isPropertyAvailable prepend="" property="a" date="true" required="true">:a,</isPropertyAvailable>
      <isPropertyAvailable prepend="" property="b" maxlength="10">:b</isPropertyAvailable>
    </dynamic>
    )
  </insert>
  
    <select id="CheckTestId" db="Api">
    select count(0) count from aa
    <dynamic prepend=" where 1=1 ">
      <isPropertyAvailable prepend=" and " property="id">id=:id</isPropertyAvailable>
    </dynamic>
  </select>
 </sqlMap>
 db对应db.json中的key
 type分四种 
 	1、"all" 查询所有可以不用传参数 
	 2、"param" 根据参数查询参数必须填写 
	 3、"page" 分页查询参数如果没有查询第一页
	 4、"write" 写操作
 dynamic下节点 isPropertyAvailable或isNotNullOrEmpty上的属性有四种
 	date 是验证时间
	required 是验证必填
	maxlength 是验证最大长度 
	existsmap 是验证是否已经存在 不存在是通过验证
	checkmap 是验证是否存在，存在通过验证
	
接口地址：http://127.0.0.1/home/index
访问的地址：http://127.0.0.1/testurl?name=aa&id=1
访问的分页地址：http://127.0.0.1/testurl?name=aa&id=1&pageid=1&pagesize=10
支持post get 等所有方式请求![](https://raw.githubusercontent.com/weizhonzhen/FastApi/master/help.jpg)
![](https://raw.githubusercontent.com/weizhonzhen/FastApi/master/xml.jpg)

