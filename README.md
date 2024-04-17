## Видеодемонстрация
<a href="https://youtu.be/XNaW_xl0i7M?si=rn8kg31RBWWa-gyf">YouTube</a>
## Документация по REST API

Базовый URL: `https://{address}:{port}/`

### Запрос на поиск схожих файлов (Similarities)

Метод: `POST`

URL: `https://{address}:{port}/`

#### Пример запроса

```json
{
  "RequestType": 1,
  "Body": {
    "SearchText": "Текст для поиска схожих файлов",
    "MaxCount": 10
  }
}
```

#### Пример ответа

```json
{
  "SearchText": "Текст для поиска схожих файлов",
  "FileViews": [
    {
      "Id": 1,
      "Path": "/path/to/file",
      "FullPath": "/full/path/to/file",
      "Name": "file_name.txt",
      "Percentage": 85.0
    },
    {
      "Id": 2,
      "Path": "/path/to/another/file",
      "FullPath": "/full/path/to/another/file",
      "Name": "another_file_name.txt",
      "Percentage": 72.0
    }
  ]
}
```

#### Описание полей ответа

- `SearchText`: Текст, использованный для поиска схожих файлов.
- `FileViews`: Список файлов, схожих с указанным текстом.
  - `Id`: Уникальный идентификатор файла.
  - `Path`: Относительный путь к файлу.
  - `FullPath`: Полный путь к файлу.
  - `Name`: Название файла.
  - `Percentage`: Процент схожести файла с указанным текстом.

### Описание моделей

#### RequestModel

Модель запроса.

- `RequestType`: Тип запроса. Может принимать значение `Similarities`.
- `Body`: Тело запроса.

#### SimilaritiesResponse

Модель ответа для запроса `Similarities`.

- `SearchText`: Текст, использованный для поиска схожих файлов.
- `MaxCount`: Максимальное количество выдаваемых соответствий файлов.
- `FileViews`: Список файлов, схожих с указанным текстом.

#### FileView

Модель представления файла.

- `Id`: Уникальный идентификатор файла.
- `Path`: Относительный путь к файлу.
- `FullPath`: Полный путь к файлу.
- `Name`: Название файла.
- `Percentage`: Процент схожести файла с указанным текстом.
