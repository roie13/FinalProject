using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using UUIDNext;

using static Project.Utils;

namespace Project;

class Program
{
  static void Main()
  {

    /*───────────────────────────╮
    │ Creating the server object │
    ╰───────────────────────────*/
    var server = new HttpListener();
    server.Prefixes.Add("http://*:5000/");
    server.Start();

    Console.WriteLine("Server started. Listening for requests...");
    Console.WriteLine("Main page on http://localhost:5000/website/index.html");

    /*─────────────────────────────────────╮
    │ Creating the database context object │
    ╰─────────────────────────────────────*/
    var databaseContext = new DatabaseContext();

    if (!databaseContext.Dishes.Any(dish => dish.Name == "Hamburger"))
    {
      databaseContext.Dishes.Add(new Dish(
      "Hamburger",
      "https://i.imgur.com/Qbc1O9A.png",
      "המבורגר:  הוא קציצה מטוגנת או צלויה העשויה לרוב מבשר בקר, ושמו של כריך העשוי מקציצה זו. כריך ההמבורגר מוגש בלחמנייה עגולה פרוסה לשניים, עם תוספות כגון ירקות, רטבים ועוד..."
      ));

      databaseContext.Dishes.Add(new Dish(
            "Pizza",
            "https://i.imgur.com/B9Tk2S0.png",
            "פיצה: היא מאכל איטלקי עשוי בצק שמרים אפוי, אשר מכוסה בגרך כלל בשכבה של רוטב עגבניות וגבינה. לעיתים קרובות הפיצה מכילה רכיבים אחרים כמו זיתים, בצל, תירס, טונה פטריות ועוד..."
       ));

      databaseContext.Dishes.Add(new Dish(
      "Fries",
      "https://i.imgur.com/sEbAMsr.png",
      "צ'יפס: הוא מאכל בינלאומי העשוי מתפוחי אדמה. הוא מקובל כתוספת בדרך כלל למאכלים עממיים. בישראל לדוגמה מוגש בדרך כלל כתוספת למזון מהיר"
  ));
    }

    /*─────────────────────────╮
    │ Processing HTTP requests │
    ╰─────────────────────────*/
    while (true)
    {
      /*────────────────────────────╮
      │ Waiting for an HTTP request │
      ╰────────────────────────────*/
      var serverContext = server.GetContext();
      var response = serverContext.Response;

      try
      {
        /*────────────────────────╮
        │ Handeling file requests │
        ╰────────────────────────*/
        serverContext.ServeFiles();

        /*───────────────────────────╮
        │ Handeling custome requests │
        ╰───────────────────────────*/
        HandleRequests(serverContext, databaseContext);

        /*───────────────────────────────╮
        │ Saving changes to the database │
        ╰───────────────────────────────*/
        databaseContext.SaveChanges();

      }
      catch (Exception e)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e);
        Console.ResetColor();
      }

      /*───────────────────────────────────╮
      │ Sending the response to the client │
      ╰───────────────────────────────────*/
      response.Close();
    }

  }
  static void HandleRequests(HttpListenerContext serverContext, DatabaseContext databaseContext)
  {
    var request = serverContext.Request;
    var response = serverContext.Response;

    string absPath = request.Url!.AbsolutePath;

    if (absPath == "/signUp")
    {
      (string username, string password) = request.GetBody<(string, string)>();

      var userId = Uuid.NewDatabaseFriendly(Database.SQLite).ToString();

      var user = new User(userId, username, password);
      databaseContext.Users.Add(user);

      response.Write(userId);
    }
    else if (absPath == "/autoLogIn")
    {
      string userId = request.GetBody<string>();

      User user = databaseContext.Users.Find(userId)!;

      response.Write(new { username = user.Username });
    }

    else if (absPath == "/logIn")
    {
      (string username, string password) = request.GetBody<(string, string)>();

      User user = databaseContext.Users.First(
        u => u.Username == username && u.Password == password
      )!;

      response.Write(user.Id);
    }

    if (absPath == "/addDish")
    {
      (string name, string img, string description) = request.GetBody<(string, string, string)>();
      databaseContext.Dishes.Add(new Dish(name, img, description));
    }

    else if (absPath == "/getalldishes")
    {
      Dish[] dishes = databaseContext.Dishes.ToArray();

      response.Write(dishes);
    }
    else if (absPath == "/getDish")
    {

      string dishName = request.GetBody<string>();

      Dish dish = databaseContext.Dishes.First(dish => dish.Name == dishName);

      response.Write(dish);
    }
  }
}
class DatabaseContext : DbContextWrapper
{
  public DbSet<Dish> Dishes { get; set; }
  public DbSet<User> Users { get; set; }

  public DatabaseContext() : base("Database") { }
}
class Dish(string name, string image, string description)
{
  [Key]
  public int Id { get; set; }
  public string Name { get; set; } = name;
  public string Image { get; set; } = image;
  public string Description { get; set; } = description;
}

class User(string id, string username, string password)
{
  [Key]
  public string Id { get; set; } = id;
  public string Username { get; set; } = username;
  public string Password { get; set; } = password;
}
