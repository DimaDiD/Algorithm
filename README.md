# MMSA — мультимовний сайт з основ програмування

Магістерська робота. Курси з поясненнями та прикладами коду кількома мовами
програмування, вправи з автоматичною перевіркою (зокрема ШІ) і кабінет викладача.

## Структура репозиторію

| Папка | Що там |
|---|---|
| `MMSA/` | Бекенд: ASP.NET Core 10, EF Core, SQL Server (`MMSA.sln`) |
| `MMSA/MMSA` | Веб-API, автентифікація, фонові задачі, точка входу |
| `MMSA/MMSA.BLL` | Бізнес-логіка, сервіси, DTO |
| `MMSA/MMSA.DAL` | Сутності, репозиторії, міграції |
| `MMSA-Client/` | Фронтенд: React 18, TypeScript, Ant Design |
| `.github/workflows/` | CI/CD (GitHub Actions) |

## Локальний запуск

1. Бекенд: відкрити `MMSA/MMSA.sln` і запустити проєкт `MMSA`
   (секрети — через `dotnet user-secrets`: `Jwt:SigningKey`, `AiReview:ApiKey`, `SeedAdmin:*`).
2. Фронтенд:
   ```
   cd MMSA-Client
   npm install
   npm start
   ```
   Запити `/api/*` проксуються на бекенд (`"proxy"` у `package.json`).

## Тести

```
cd MMSA-Client
npm test -- --watchAll=false
```

## Публікація

`dotnet publish` проєкту `MMSA` автоматично збирає фронтенд (`npm ci`, `npm run build`)
і кладе його у `wwwroot`, тож сайт і API працюють з однієї адреси.

Фронтенд перенесено сюди з репозиторію `Algorithm-client` разом з історією комітів
(`git subtree`).
