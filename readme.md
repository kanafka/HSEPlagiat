# Отчет по Контрольной Работе №2: Синхронное межсервисное взаимодействие

**Автор**: Хабаров Артемий БПИ-239


---

## 1. Введение

В рамках задания разработана система для автоматической проверки текстовых отчетов студентов на антиплагиат, подсчета статистики и визуализации данных (облака слов). Решение основано на микросервисной архитектуре с тремя сервисами:

1. **API Gateway** — маршрутизация запросов и агрегирование ответов.
2. **File-Storing Service** — хранение файлов и метаданных с защитой от дублирования.
3. **File-Analysis Service** — анализ содержимого:

    * подсчет статистики (слова, символы, абзацы),
    * сравнение текстов на схожесть (антиплагиат),
    * генерация облака слов.

---

## 2. Архитектура

```text
[Client]
   ↓ HTTP
[API Gateway]
   ├──→ [File-Storing Service] ──↔─ {Database, Disk Storage}
   └──→ [File-Analysis Service] ──↔─ {Database, WordCloud Storage}
```

* **API Gateway**: единая точка входа, переправляет запросы в соответствующие сервисы и обрабатывает ошибки upstream.
* **File-Storing Service**: сохраняет файлы в каталог `Files`, вычисляет SHA256-хэш, сохраняет метаданные (`Id`, `Name`, `Hash`, `Location`) в БД.
* **File-Analysis Service**:

    * **WordAnalyze**: с помощью регулярных выражений и `char.IsLetter` считает количество слов, символов и абзацев.
    * **PlagiatAnalyze**: загружает все ранее сохраненные файлы, вычисляет количество общих символов и нормирует по максимальной длине текста, возвращает процент схожести.
    * **WordCloudService**: вызывает `quickchart.io` API для генерации PNG-облака слов, сохраняет в файловом хранилище.

---

## 3. Реализация функциональности

### 3.1. Подсчет статистики

Метод `WordAnalyze(Guid fileId)` в `SimpleAnalysisService`:

* Загружает текст через HTTP из File-Storing Service.
* `Regex.Matches(text, "\b\\w+\b").Count` — слова.
* `text.Count(char.IsLetter)` — символы.
* `text.Split(["\r\n\r\n","\n\n"])` — абзацы.
* Результаты сохраняются в БД (если ранее не были рассчитаны).

### 3.2. Проверка на антиплагиат

Метод `PlagiatAnalyze(Guid fileId)`:

1. Загружает исходный текст и список всех ID.
2. Для каждого другого файла: загружает текст, вычисляет общее число символов через подсчет пересечений и снижает счетчик, нормирует на максимальную длину одного из двух текстов.
3. Находит максимальный процент схожести и сохраняет в БД.

### 3.3. Облако слов

Сервис `WordCloudService` упаковывает текст в JSON и отправляет POST на `https://quickchart.io/wordcloud`. Результат (byte\[]) сохраняется в файловом хранилище с `fileId.png`.

---

## 4. Обработка ошибок

Во всех контроллерах (`FileController`, `AnalysisController`, `GatewayController`) реализован `try/catch`:

* `FileNotFoundException` → `404 NotFound` (с JSON `{ code, message }`).
* Остальные исключения → `500 InternalError` или `502 Bad Gateway` (для шлюза).
* `BadRequest` для неверных входных данных.

---

## 5. Спецификация API

### 5.1 File-Storing Service (/file)

| Метод    | Путь               | Запрос              | Ответ 200                                   | Ошибки                     |
| -------- | ------------------ | ------------------- | ------------------------------------------- | -------------------------- |
| Upload   | POST /file/upload  | multipart/form-data | `{ id, message }`                           | 400, 500                   |
| Download | GET /file/get/{id} | —                   | FileStreamResult (application/octet-stream) | 404 `{code, message}`, 500 |
| List IDs | GET /file/getAllId | —                   | `[ "GUID", ... ]`                           | —                          |

### 5.2 File-Analysis Service (/analyze)

| Метод       | Путь                               | Ответ 200                                       | Ошибки   |
| ----------- | ---------------------------------- | ----------------------------------------------- | -------- |
| Plagiarism  | GET /analyze/{fileId}              | `{ fileId, similarities }`                      | 404, 500 |
| WordAnalyze | GET /analyze/WordAnalyze/{fileId}  | `{ wordCount, paragraphCount, characterCount }` | 404, 500 |
| WordCloud   | GET /analyze/getWordCloud/{fileId} | FileStreamResult (image/png)                    | 404, 500 |

### 5.3 API Gateway (/Gateway)

| Метод       | Путь                              | Описание                                  | Ошибки   |
| ----------- | --------------------------------- | ----------------------------------------- | -------- |
| Upload      | POST /Gateway/upload              | Проброс в `/file/upload`                  | 400, 502 |
| Download    | GET /Gateway/file/{fileId}        | Проброс в `/file/get`                     | 404, 502 |
| WordCloud   | GET /Gateway/WorldCloud/{fileId}  | Проброс в `/analyze/getWordCloud`         | 404, 502 |
| Analyze     | GET /Gateway/analyze/{fileId}     | Проброс в `/analyze/{fileId}`             | 502      |
| WordAnalyze | GET /Gateway/WordAnalyze/{fileId} | Проброс в `/analyze/WordAnalyze/{fileId}` | 502      |

---

## 6. Тесты и покрытие

* **Unit-тесты** на все сервисы и контроллеры с помощью **xUnit** + **Moq**, покрытие > 60%.
* **In-memory database** (`Microsoft.EntityFrameworkCore.InMemory`) для тестов.
* Мокирование `HttpMessageHandler.SendAsync` через `Moq.Protected`.

---

**Документ завершен.**
