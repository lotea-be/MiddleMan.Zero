# Bad Request (400)

- **HTTP status:** `400 Bad Request`
- **`type`:** `https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/bad-request.md`
- **`ResultStatus`:** `Invalid`

The request could not be processed because it failed validation. One or more logged
`InvalidRequestMessage`s describe what was wrong; each appears in the `messages` array with a
human-readable `message` and a stable `code`.

## Example body

```json
{
  "type": "https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/bad-request.md",
  "title": "Bad Request",
  "status": 400,
  "detail": "The order id is required.",
  "messages": [
    { "message": "The order id is required.", "code": "order_id_required" }
  ]
}
```

When no message was logged, `detail` defaults to `"The request is invalid."`.
