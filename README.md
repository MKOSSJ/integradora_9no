# Sistema de Gestion de Secuencias Didacticas

## Configuracion local de JWT

La clave de firma no se almacena en el repositorio. Configure una clave aleatoria de al menos 32 bytes mediante una de estas opciones:

```powershell
dotnet user-secrets set "Jwt:Key" "VALOR_ALEATORIO_SEGURO" --project backend/Plandi.API/Plandi.API/Plandi.API.csproj
$env:JWT_SIGNING_KEY = "VALOR_ALEATORIO_SEGURO"
```

En ambientes desplegados use `JWT_SIGNING_KEY` o el gestor de secretos de la plataforma.
