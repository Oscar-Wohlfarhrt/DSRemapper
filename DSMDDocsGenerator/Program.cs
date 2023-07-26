
using DSRemapper.RemapperLua;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

//string xmlDocPath = "G:\\Projectos Visual\\DSRemapperDocumentator\\DSRemapperDocumentator\\bin\\Debug\\net7.0\\DSRemapperDocumentator2.xml";
//string xmlDocPath = "DSRemapperDocumentator.xml";G:\Projectos Visual\DSRemapper\build\Release\Plugins\Remapper
string xmlDocPath = @"G:\Projectos Visual\DSRemapper\build\Release\Plugins\Remapper\DSRemapper.RemapperLua.xml";
XmlDocument xmlDoc = new XmlDocument();
xmlDoc.PreserveWhitespace = true;

if (File.Exists(xmlDocPath))
{
    xmlDoc.Load(xmlDocPath);

    TypeInfo type = typeof(Utils).GetTypeInfo();

    var typeNode = xmlDoc.SelectSingleNode($"//member[starts-with(@name, 'T:{type.FullName}')]");

    string typeDoc = $@"# {type.Name} - {GetTypeName(type)}
Namespace: {type.Namespace ?? "Unknown"}
Assembly: {type.Assembly.GetName().Name ?? "Unknown"}.dll

{GetSummary(typeNode)}

## Fields

|Type|Name|Description|
|---|---|---|
{GetFields(type, xmlDoc)}

## Properties

|Type|Name|Description|
|---|---|---|
{GetProperties(type, xmlDoc)}

## Methods

{GetMethods(type, xmlDoc)}

## Events
";

    Console.WriteLine(typeDoc);
}
else
    Console.WriteLine("No Documentation xml");

/*|Return Type|Name|Description|
|---|---|---|*/

string GetTypeName(TypeInfo type)
{
    if (type.IsClass)
        return "Class";

    return "Unknown";
}
string GetSummary(XmlNode? xmlNode)
{
    return Regex.Replace(xmlNode?["summary"]?.InnerText ?? "", @"^\s+", "", RegexOptions.Multiline).Trim();
}

string GetFields(TypeInfo type, XmlDocument xmlDoc)
{
    string fields = "";
    FieldInfo[] fieldArr = type.GetFields();

    foreach (FieldInfo field in fieldArr)
    {
        var typeNode = xmlDoc.SelectSingleNode($"//member[starts-with(@name, 'F:{type.FullName}')]");

        fields += @$"|{(field.IsLiteral?"const ":"")}{field.FieldType.GetFriendlyName()}|{field.Name}|{GetSummary(typeNode)}|
";
    }

    return fields.Trim();
}
string GetProperties(TypeInfo type, XmlDocument xmlDoc)
{
    string fields = "";
    PropertyInfo[] propArr = type.GetProperties();

    foreach (PropertyInfo prop in propArr)
    {
        var typeNode = xmlDoc.SelectSingleNode($"//member[starts-with(@name, 'F:{type.FullName}')]");

        fields += @$"|{prop.PropertyType.GetFriendlyName()}|{prop.Name}{GetSetProp(prop)}|{GetSummary(typeNode)}|
";
    }

    return fields.Trim();
}

string GetSetProp(PropertyInfo prop)
{
    MethodInfo? setMeth = prop.SetMethod;
    return $" {{{(prop.CanRead ? " get; " : " ")}{((setMeth?.IsPrivate ?? false) ? "private " : "")}{(prop.CanWrite ? "set; " : "")}}}";
}

string GetMethods(TypeInfo type, XmlDocument xmlDoc)
{
    string fields = "";
    MethodInfo[] methArr = type.GetMethods();
    PropertyInfo[] propArr = type.GetProperties();
    MethodInfo?[] propMethods = (propArr.Select(p => p.GetMethod)//.Where(p=>p.GetMethod!=null)
        .Union(propArr.Select(p => p.SetMethod))//.Where(p => p.SetMethod != null)
        ).Where(p => p != null).ToArray();

    foreach (MethodInfo meth in methArr)
    {
        if (meth.DeclaringType != type.AsType() ||
            propMethods.Contains(meth))
            continue;

        var typeNode = xmlDoc.SelectSingleNode($"//member[starts-with(@name, 'M:{type.FullName}.{meth.Name}')]");

        var parameters = meth.GetParameters();

        string param = string.Join(", ", parameters.Select(GetMethodParameter));

        string parametersDesc = "";

        if (parameters.Length > 0)
            parametersDesc = $@"
#### Parameters

|Type|Name|Description|
|---|---|---|
{GetParameters(parameters, typeNode)}";

        string returns = "";
        
        if (meth.ReturnType != typeof(void))
            returns = @$"
#### Return

|Type|Description|
|---|---|
|{meth.ReturnType.GetFriendlyName()}|{GetInnerSummary(typeNode?["returns"])}|
";

        fields += @$"### {meth.Name}({param})

{GetSummary(typeNode)}
{parametersDesc}
{returns}
";

        /*fields += @$" |{meth.ReturnType.Name}|{meth.Name}({param})|{GetSummary(typeNode)}|
        ";*/

    }

    return fields.Trim();
}
string GetMethodParameter(ParameterInfo p)
{
    Type? defValType = p.DefaultValue?.GetType();
    string defaultValue = (defValType != null && p.HasDefaultValue) ? $" = {(defValType.IsEnum ? $"{defValType.Name}.{p.DefaultValue}" : $"{p.DefaultValue}")}" : "";//(p.HasDefaultValue?$" = {p.DefaultValue}":"")

    return $"{(p.IsOptional ? "[" : "")}{(p.IsOut ? "out " : "")}{p.ParameterType.Name} {p.Name}{defaultValue}{(p.IsOptional ? "]" : "")}";
}
string GetParameters(ParameterInfo[] paras, XmlNode? methNode)
{
    string strParams = "";
    foreach (ParameterInfo par in paras)
    {
        var paramNode = methNode?.SelectSingleNode($"//param[@name = '{par.Name}']");
        strParams += $"|{par.ParameterType.GetFriendlyName()}|{par.Name}|{GetInnerSummary(paramNode)}|\n";
    }
    return strParams.Trim();
}
string GetInnerSummary(XmlNode? xmlNode)
{
    return Regex.Replace(xmlNode?.InnerText ?? "", @"^\s+", "", RegexOptions.Multiline).Trim().Replace("\r\n", "<br>");
}

static class TypeExtension
{
    public static string GetFriendlyName(this Type type)
    {
        return type.Name.Replace("Single", "Float");
    }
}
