# The encryptor is listening on port 45576.


# Encrypting
```cURL
curl -X POST http://localhost:45576/encrypt -H "Content-Type: application/json" --data-binary @- <<DATA
{
  "data": "hi",
  "key": "my key",
  "password": "password in config.json",
  "useHashing": true
}
```

Success:
```json
{
  "success": true,
  "encrypted": "ENCRYPTED DATA",
  "useHashing": true
}
```

Fail:
```json
{
  "success": false
}
```


# Decrypting
```cURL
curl -X POST http://localhost:45576/decrypt -H "Content-Type: application/json" --data-binary @- <<DATA
{
  "data": "hi",
  "key": "my key",
  "password": "password in config.json",
  "useHashing": true
}
```

Success:
```json
{
  "success": true,
  "decrypted": "DECRYPTED DATA",
  "useHashing": true
}
```

Fail:
```json
{
  "success": false
}
```
