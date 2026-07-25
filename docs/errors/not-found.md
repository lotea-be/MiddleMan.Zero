# Not Found (404)

- **HTTP status:** `404 Not Found`
- **`type`:** `https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/not-found.md`
- **`ResultStatus`:** `NotFound`

The requested resource does not exist. Any logged `NotFoundMessage`s appear in the `messages` array.

## Example body

```json
{
  "type": "https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/not-found.md",
  "title": "Not Found",
  "status": 404,
  "detail": "Order 3f2a… was not found.",
  "messages": [
    { "message": "Order 3f2a… was not found.", "code": "order_not_found" }
  ]
}
```

When no message was logged, `detail` defaults to `"The requested resource was not found."`.
