## Requisitos

- Windows 10 o superior.
- Visual Studio 2022 con la carga de trabajo **ASP.NET y desarrollo web** y el
	SDK de .NET 8, o el SDK de .NET 8 instalado por separado.

## Ejecutar desde un ZIP descargado de GitHub

1. En GitHub, seleccionar **Code > Download ZIP**.
2. Extraer el ZIP completo en una carpeta local.
3. Abrir `Hacienda_TOBE.sln` con Visual Studio 2022.
4. Establecer `p_mvcHacienda` como proyecto de inicio.
5. Ejecutar con **Ctrl+F5** o **F5**.

La carpeta `p_mvcHacienda/Datos` contiene los archivos de datos necesarios para
la ejecución y se copia automáticamente al directorio de salida al compilar.

## Ejecutar desde la terminal

Desde la carpeta donde se encuentra `Hacienda_TOBE.sln`:

```powershell
dotnet restore Hacienda_TOBE.sln
dotnet run --project p_mvcHacienda/p_mvcHacienda.csproj
```

La terminal mostrará la dirección local donde está disponible la aplicación.

## Estructura principal

- `Bib_Hacienda`: dominio, aplicación, infraestructura e interfaces.
- `p_mvcHacienda`: aplicación web ASP.NET Core MVC.
- `Documentacion_y_Diagrama`: diagramas y documentación del proyecto.
