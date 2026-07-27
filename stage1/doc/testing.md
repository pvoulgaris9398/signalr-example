# Testing

```bash
wscat -c ws://localhost:5000/ws
```

```bash
curl \
-X POST \
http://localhost:5000/api/events \
-H "Content-Type: application/json" \
-d "{\"message\":\"Hello\"}"
```
