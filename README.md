# Documentación de la API ApiLey711

La API ApiLey711 proporciona endpoints para obtener registros de las liquidaciones de beneficiarios involucrados con la Ley 711 según los parámetros especificados. Se conecta con una base de datos DBF y está construida en C# utilizando el minimal API framework ASP.NET Core en .NET 7.

## Dependencias

Dependencias agregadas en tu proyecto:

Microsoft.AspNetCore.OpenApi" Version="7.0.5" 
"Swashbuckle.AspNetCore" Version="6.5.0" 
"System.Data.OleDb" Version="7.0.0"


## Configuración de la cadena de conexión

La configuración de la cadena de conexión se maneja a través del archivo `appsettings.json`. Asegúrate de colocar la ruta de la base de datos DBF dentro de la sección `"ConnectionStrings": { "RutaDBFs" }`.

## Endpoints

### GET /veteranos/{anio}/{mes}

Recupera los registros de las liquidaciones correspondientes al año y mes especificados.

#### Parámetros de ruta

- `{anio}`: Año de las liquidaciones (ejemplo: 2023).
- `{mes}`: Mes de las liquidaciones (ejemplo: 5).

#### Respuestas

- 200 OK: Se devuelve una lista de objetos `LiquidacionesPorMes` que contienen la información de las liquidaciones y los veteranos correspondientes al año y mes especificados.
- 400 Bad Request: Ocurrió un error al obtener los registros de liquidaciones.
- 400 Bad Request: Fecha fuera de rango.

## Configuración adicional

La API utiliza la biblioteca Swagger para generar la documentación interactiva. Puedes acceder a ella navegando a `/swagger/index.html` en el entorno de desarrollo.

## Clases

### Liquidaciones

Representa la información de las liquidaciones.

Propiedades:

- `Carpeta` (string): Carpeta de la liquidación.
- `Anio` (int): Año de la liquidación.
- `Mes` (int): Mes de la liquidación.
- `Descripcion` (string): Descripción de la liquidación.


### LiquidacionesPorMes

Representa la información de las liquidaciones por mes.

Propiedades:

- `Carpeta` (string): Carpeta de la liquidación.
- `Anio` (int): Año de la liquidación.
- `Mes` (int): Mes de la liquidación.
- `Descripcion` (string): Descripción de la liquidación.
- `Veteranos` (List<Veteranos> | null): Lista de objetos `Veteranos` correspondientes a la liquidación. Puede ser nulo si no hay registros de veteranos para la liquidación.

### Veteranos

Representa la información de los veteranos.

Propiedades:

- `Expediente` (string): Número de expediente del veterano.
- `CdoOrganismo` (string): Código del organismo del veterano.
- `Organismo` (string): Organismo del veterano.
- `Porcentaje` (decimal): Porcentaje del veterano.
- `Legajo` (decimal): Legajo del veterano.
- `Cdo` (decimal): Código del veterano.
- `ApellidoYNombre` (string): Apellido y nombre del veterano.
- `Dni` (string): DNI del veterano.
- `Distrib` (decimal): Valor de distribución del veterano.

### Veteranos
  
Representa la información de los veteranos.

Propiedades:

- `Legajo` (decimal): Número de legajo del veterano.
- `Cdo` (decimal): Código del veterano.
- `ApellidoYNombre` (string): Apellido y nombre del veterano.
- `HaberAl100` (decimal): Haber al 100% del veterano.
- `CdoOrganismo1` (string): Código del primer organismo del veterano.
- `CdoOrganismo2` (string): Código del segundo organismo del veterano.
- `CdoOrganismo3` (string): Código del tercer organismo del veterano.
- `Porcentaje1` (decimal): Porcentaje correspondiente al primer organismo del veterano.
- `Porcentaje2` (decimal): Porcentaje correspondiente al segundo organismo del veterano.
- `Porcentaje3` (decimal): Porcentaje correspondiente al tercer organismo del veterano.
- `Ingreso` (DateTime): Fecha de ingreso del veterano.
- `FechaFinDeAporte` (DateTime): Fecha de fin de aporte del veterano.
- `Sac100` (decimal): Valor del SAC al 100% del veterano.
- `Retro100` (decimal): Valor de la retroactividad al 100% del veterano.
- `Aporte` (decimal): Valor del aporte del veterano.
  
### Utilización
  
Con los parámetros de entrada:

anio (int): Año para el cual se desean obtener los datos de los veteranos.
mes (int): Mes para el cual se desean obtener los datos de los veteranos.
Puede solicitar los datos utilizando la siguiente Ejemplo de URL: https://api.example/2023/05

Formato de respuesta
La API devuelve los datos de los veteranos en formato JSON. La respuesta tiene la siguiente estructura:

json:
[  
  
  {  
  
    "carpeta": "DATA0281",  
    "descripcion": "Liquidación mayo",  
    "mes": 5,  
    "anio": 2023,  
    "veteranos": [  
      {  
        "Legajo": 12345,  
        "Cdo": 6789,  
        "ApellidoYNombre": "Apellido, Nombre",  
        "HaberAl100": 1000.00,  
        "CdoOrganismo1": "ORG001",  
        "CdoOrganismo2": "ORG002",  
        "CdoOrganismo3": "ORG003",  
        "Porcentaje1": 50.0,  
        "Porcentaje2": 30.0,  
        "Porcentaje3": 20.0,  
        "Ingreso": "2020-01-01",  
        "FechaFinDeAporte": "2030-12-31",  
        "Sac100": 500.00,  
        "Retro100": 200.00,  
        "Aporte": 100.00  
      },
        // Otros veteranos...  
    ]
  
  }  
  
]
  
La respuesta contiene los siguientes campos:

carpeta (string): Número de carpeta asociado a la solicitud de la base de datos.
descripcion (string): Descripción de la liquidación.
mes (int): Mes correspondiente a los datos de los veteranos.
anio (int): Año correspondiente a los datos de los veteranos.
veteranos (array): Un arreglo de objetos que representa los datos de los veteranos.

