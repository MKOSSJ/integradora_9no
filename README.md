# Sistema de Gestion de Secuencias Didacticas

## Configuracion local de JWT

La clave de firma no se almacena en el repositorio. Configure una clave aleatoria de al menos 32 bytes mediante una de estas opciones:

```powershell
$jwtKey = [Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "Jwt:Key" $jwtKey --project backend/Plandi.API/Plandi.API/Plandi.API.csproj
$env:JWT_SIGNING_KEY = $jwtKey
```

En ambientes desplegados use `JWT_SIGNING_KEY` o el gestor de secretos de la plataforma.
