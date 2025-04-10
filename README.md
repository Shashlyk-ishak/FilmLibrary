Фильмотека

Веб-приложение для управления коллекцией фильмов с системой пользователей и административной панелью.

Описание проекта

Фильмотека - это современное веб-приложение, разработанное на платформе ASP.NET Core, предоставляющее удобный интерфейс для просмотра и управления коллекцией фильмов. Проект включает в себя систему аутентификации пользователей, административную панель и интуитивно понятный пользовательский интерфейс.

Основные функции

 Для всех пользователей
 Регистрация и авторизация
 Просмотр каталога фильмов
 Подробная информация о каждом фильме
 Возможность оценивать фильмы


Для администраторов
 Добавление новых фильмов
 Редактирование информации о фильмах
 Удаление фильмов
 Управление пользователями
 Назначение администраторских прав

Технологии

Backend:
  - ASP.NET Core MVC
  - Entity Framework Core
  - Identity Framework //для аутентификации
  - SQL Server //для хранения данных

  Frontend:
  - HTML5, CSS3
  - Bootstrap 5
  - JavaScript/jQuery
  - Bootstrap Icons

Архитектура проекта

- **Models:** Сущности базы данных и view-модели
- **Views:** Razor-представления с поддержкой частичных представлений
- **Controllers:** Логика обработки запросов
- **Services:** Бизнес-логика приложения
- **Data:** Контекст базы данных и миграции


Для запуска проекта

1. Клонировать репозиторий
2. Восстановить пакеты NuGet
3. Настроить строку подключения к базе данных в `appsettings.json`
4. Применить миграции: `dotnet ef database update`
5. Запустить проект: `dotnet run`

Требования к системе

- .NET 6.0 или выше
- SQL Server
- Visual Studio 2022 
