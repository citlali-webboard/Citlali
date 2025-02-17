# 🩷 Citlali 🩷

<p align="center">
  <img width="70%" src="https://i0.wp.com/traveler.gg/wp-content/uploads/image-4877.png?fit=1200%2C675&ssl=1">
</p>

**Citlali** is an event webboard application that brings together like-minded individuals, helping them connect and enjoy shared activities!

## ⚙️ Technologies Used

- 💻 [ASP .NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-9.0)
- 💽 [Supabase (for database, object storage, and user authetication)](https://supabase.com/)
- 🔐 [JsonWebToken](https://jwt.io/)

## 📦 Installation (for developer)

> Make sure you have .NET 9.0 or higher installed on your machine.
> You can simply check with `dotnet --version`.

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/citlali-webboard/Citlali.git
cd Citlali
```

### 2️⃣ Install dotnet's Dependencies
```bash
dotnet restore
```

### 3️⃣ Edit appsettings.json
you should read all of them and change according to your environment, but you definitely should change
- App
  - Url: base url/FQDN on your environment
- Supabase
  - LocalUrl : url for this backend connection to supabase, can be the same as PublicUrl but ideally should be a private IP
  - PublicUrl : url for user connection to supabase
- Mail
  - SendAddress : email address for sending transactional notification email
  - SmtpServer : smtp server for transactional notification email
  - SmtpPort : port for above smtp server mentioned

### 4️⃣ Use dotnet user-secrets to add secrets
you need to set
- `Jwt:Secret` : your jwt signing key, should be the same on supabase
- `Supabase:ServiceRoleKey` : a service role key to access supabase
- `Mail:SmtpUsername` : Username for logging in to SmtpServer in appsettings
- `Mail:SmtpPassword` : Password for logging in to SmtpServer in appsettings

by running something like

```bash
dotnet user-secrets set "Supabase:ServiceRoleKey" "replace_with_your_own_secret"

```

### 5️⃣ Run in Watch Mode
Running in watch mode will automatically recompile the source code when it is modified.
This will start the application and automatically open it in your browser.
```bash
dotnet watch
```

### 🎉 Enjoy developing !

## ⚠️ Disclaimer

This project is an independent, non-commercial initiative and is not affiliated with, endorsed by, or associated with HoYoverse or any of its subsidiaries. The use of the name "Citlali" is purely for decorative purposes and does not imply any official connection. All trademarks, copyrights, and other intellectual properties belong to their respective owners. This project is developed solely for educational and non-profit purposes.

Happy coding! 🚀