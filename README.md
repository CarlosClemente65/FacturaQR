<h1 align="center"> FacturaQR </h1>
<br>

<h2> Inserta un codigo QR a una factura en PDF con los datos para Veri*Fatu. </h2>
<br>
<h4> @ Carlos Clemente (Diagram Software Europa S.L.) - 10/2025 </h4>

<h3>Descripción</h3>
Añade el codigo QR obligatorio en facturas para sistemas Veri*Factu.
Permite añadir una marca de agua, y al finalizar puede imprimir o abrir el PDF generado
Tambien puede usarse como visualizador de ficheros PDF.
<br><br>

### Control versiones

* v1.0.0.0 Primera versión funcional
* v1.1.0.0 Incorporada la opción para el procesado mediante guion
* v1.2.0.0 Incorporada la opción para añadir una marca de agua
* v1.3.0.0 Incorporada la opcion para pasar la imagen del QR generada de forma externa
* v1.4.0.0 Incorporada la opcion para imprimir el PDF ademas de guardarlo en disco
* v1.5.0.0 Incorporada la opcion para abrir el PDF en el visor SumatraPDF
* v1.6.0.0 Incorporada la opcion para visualizar cualquier PDF que se pase por parametro (sin utilizar la insercion del QR)
* v1.7.0.0 Incorporada la opcion para cerrar los procesos abiertos del visor SumatraPDF
* v1.8.0.0 Incorporada la posibilidad de añadir la marca de agua a cualquier PDF que se pase
<br><br>


### Uso:
```
FacturaQR.exe ds123456 guion.txt
```
<br>

#### Parametros guion
* pdfentrada=Nombre del pdf con la fatura; obligatorio
* pdfsalida=Nombre del pdf con el QR; opcional
* ficheroqr=Nombre del fichero con la imagen del QR; opcional (si no se pasa es obligatorio los campos nifemisor y datos factura)
* entorno='pruebas' para forzar el envio a la web de pruebas; opcional
* verifactu=SI/NO para indicar si son facturas verificables; opcional
* url=direccion url para la validacion; opcional
* nifemisor=NIF del emisor de la factura para incluir en el QR; opcional
* numerofactura=Numero de de factura para incluir en el QR; obligatorio si nifemisor <> ""
* fechafactura=Fecha de la factura para incluir en el QR; obligatorio si nifemisor <> ""
* totalfactura=Importe total de la factura para incluir en el QR; obligatorio si nifemisor <> ""
* posicionx=posicion en milimetros desde el margen izquierdo; opcional 
* posiciony=posicion en milimetros desde el margen superior; opcional
* ancho=ancho del QR en milimetros (el alto sera el mismo); opcional
* color=Color del QR en formato hexadecimal; opcional
* marcaagua=Texto para insertar una marca de agua en el documento; opcional
* accionpdf=[imprimir | abrir | visualizar]; Acciones adicionales a realizar con el PDF; opcional
* cerrarvisor ;Permite dar la orden de cerrar el visor; opcional
* ficherosalida=nombre del fichero para controlar la finalizacion del proceso

<br>

### Notas:
* No es necesario pasar los parametros con comillas si hay espacios; se toma el valor que hay a continuacion del '='
* Los nombres de los parametros pueden ir en mayusculas o minusculas (se convierten a minusculas)
* Si no se pasa el nombre de salida, se utiliza el mismo que el de entrada con un sufijo (_salida)
* La url se puede pasar (debe estar bien formada), y si no se pasa, se genera en base a los datos de la factura, entorno y verifactu
* El entorno por defecto es la web de produccion (real), por lo que en pruebas debe pasarse el parametro entorno=pruebas
* Por defecto se funciona en modo NO VeriFactu, por lo que para trabajar de ese modo se debe pasar el parametro verifactu=si
* Si no se pasa el fichero con la imagen QR, es obligatorio pasar los campos nifemisor y datos factura (fecha,numero e importe)
* Si no se pasa el NIF del emisor no se añadira el QR; si se pasa es obligatorio pasar los demas parametros de la factura.
* Las posiciones X e Y del QR estan puestas por defecto a 10 mm de los margenes
* El ancho del QR tiene un defecto de 30 mm; no tiene limitacion pero deberia estar entre 25 y 40 mm (alto y ancho)
* Controla que no se desborde el QR por el margen derecho (posicion X mas ancho superior al ancho de la pagina)
* El color del QR por defecto es negro (#000000)
* El texto de la marca de agua admite saltos de linea añadiendo '\n' en la posicion donde insertarlo
* Si se produce algun error por algun parametro que falte o no sea correcto, se genera el fichero "errores.txt" con el detalle
* El parametro 'accionpdf= permite realizar acciones adicionales con el PDF utilizando el programa SumatraPDF
	- 'abrir' = Abre el PDF generado con el visor; la aplicacion espera a que se cierre el visor para continuar
	- 'imprimir' = Lanza el PDF generado por la impresora predeterminada
	- 'visualizar' = Abre el PDF pasado por parametro con el visor; la aplicacion continua sin esperar al cierre del visor
* El parametro cerrarvisor' permite cerrar todos los procesos abiertos del visor SumatraPDF; no es necesario pasar ningun parametro mas
* Si se incluye el parametro 'ficherosalida' la aplicacion genera un fichero que puede usarse para controlar si la aplicacion ha terminado o no. 
  Con el parametro 'visualizar' la aplicacion no se detiene aunque no se cierre el visor, por lo que se generara (si se ha indicado) el fichero de salida
* En la ruta de ejecucion deben estar los siguientes ficheros:
	- PdfSharp.dll
	- QRCoder.dll
	- SumatraPDF.exe (necesario si se utiliza la opcion de impresion)
* El fichero 'Configuracion_visor.txt' es una copia modificada con los parametros del visor, 
  Para usarla debe renombrarse como "SumatraPDF-settings.txt" y ubicarla en la misma ruta que el visor SumatraPDF.

