using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Project;

public static class Utils
{
  public static T GetBody<T>(this HttpListenerRequest request)
  {
    string json = new StreamReader(request.InputStream).ReadToEnd();

    string jsonFix = IsTuple(typeof(T)) ? TupliseJson(json) : json;

    return JsonConvert.DeserializeObject<T>(jsonFix)!;
  }

  public static void Write<T>(this HttpListenerResponse response, T value)
  {
    string valueJson = typeof(T) != typeof(string)
      ? JsonConvert.SerializeObject(value)
      : value!.ToString()!;

    byte[] valueBytes = Encoding.UTF8.GetBytes(valueJson);
    response.OutputStream.Write(valueBytes);
  }

  public static void ServeFiles(this HttpListenerContext serverContext)
  {
    var request = serverContext.Request;
    var response = serverContext.Response;

    string rawPath = request.RawUrl!;
    string absPath = request.Url!.AbsolutePath;
    string? referrerAbsPath = request.UrlReferrer?.AbsolutePath;

    Console.WriteLine("Received a request with path: " + rawPath);

    string filePath = "." + absPath;
    if (!filePath.EndsWith(".js"))
    {
      if (referrerAbsPath?.EndsWith(".js") ?? false) { filePath += ".js"; }
      if (referrerAbsPath?.EndsWith(".mjs") ?? false) { filePath += ".mjs"; }
    }

    bool isHtml = request.AcceptTypes!.Contains("text/html");
    bool isJs =
      filePath.EndsWith(".js") ||
      filePath.EndsWith(".mjs") ||
      (referrerAbsPath?.EndsWith(".js") ?? false) ||
      (referrerAbsPath?.EndsWith(".mjs") ?? false);

    if (File.Exists(filePath))
    {
      if (isHtml)
      {
        response.ContentType = "text/html; charset=utf-8";
        string fileStr = File.ReadAllText(filePath);
        fileStr = fileStr
          .Replace(
            "></iframe>",
            " onload='javascript:(function(o){o.style.height=o.contentWindow.document.body.scrollHeight+\"px\";}(this));' style=\"height:200px;width:100%;border:none;overflow:none;\"></iframe>"
          );

        var fileBytes = Encoding.UTF8.GetBytes(fileStr);
        response.OutputStream.Write(fileBytes);
      }
      else if (isJs)
      {
        response.ContentType = "application/javascript";
        byte[] fileBytes = File.ReadAllBytes(filePath);
        response.OutputStream.Write(fileBytes);
      }
      else
      {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        response.OutputStream.Write(fileBytes);
      }
    }
    else if (isHtml)
    {
      response.StatusCode = (int)HttpStatusCode.Redirect;
      response.RedirectLocation = "/website/404.html";
    }
  }

  public class DbContextWrapper : DbContext
  {
    readonly string name;

    public DbContextWrapper(string name) : base()
    {
      this.name = name;

      Database.EnsureCreated();
      Database.ExecuteSqlRaw("PRAGMA journal_mode = DELETE;");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlite("Data Source=" + name + ".sqlite");
    }
  }


  static string TupliseJson(string json)
  {
    var jObject = JObject.Parse(json);
    var newJObject = new JObject();

    int index = 1;
    foreach (var property in jObject.Properties())
    {
      var newKey = $"Item{index}";
      newJObject[newKey] = property.Value;
      index++;
    }

    return newJObject.ToString();
  }

  public static bool IsTuple(Type tuple)
  {
    if (!tuple.IsGenericType)
      return false;
    var openType = tuple.GetGenericTypeDefinition();
    return
      openType == typeof(ValueTuple<>) ||
      openType == typeof(ValueTuple<,>) ||
      openType == typeof(ValueTuple<,,>) ||
      openType == typeof(ValueTuple<,,,>) ||
      openType == typeof(ValueTuple<,,,,>) ||
      openType == typeof(ValueTuple<,,,,,>) ||
      openType == typeof(ValueTuple<,,,,,,>) ||
      (
        openType == typeof(ValueTuple<,,,,,,,>) &&
        IsTuple(tuple.GetGenericArguments()[7])
      );
  }
}