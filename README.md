<h1 align="center"> FacturaQR </h1>
<br>

<h2> Inserta un codigo QR a una factura en PDF con los datos para Veri*Fatu. </h2>
<br>
<h4> @ Carlos Clemente (Diagram Software Europa S.L.) - 10/2025 </h4>

<h3>Descripción</h3>
Añade el codigo QR obligatorio para sistemtas de facturas Veri*Factu, y tambien permite añadir una marca de agua
<br><br>

### Control versiones

* v1.0.0.0 Primera versión funcional
* v1.1.0.0 Incorporada la opción para el procesado mediante guion
* v1.2.0.0 Incorporada la opción para añadir una marca de agua
<br><br>


### Uso:
```
FacturaQR.exe ds123456 guion.txt
```
<br>

#### Parametros guion
* pdfentrada=Nombre del pdf con la fatura; obligatorio
* pdfsalida=Nombre del pdf con el QR; opcional
* url=direccion url para la validacion; opcional
* entorno='pruebas' para forzar el envio a la web de pruebas; opcional
* verifactu=SI/NO para indicar si son facturas verificables; opcional
* nifemisor=NIF del emisor de la factura para incluir en el QR; opcional
* numerofactura=Numero de de factura para incluir en el QR; obligatorio si nifemisor <> ""
* fechafactura=Fecha de la factura para incluir en el QR; obligatorio si nifemisor <> ""
* totalfactura=Importe total de la factura para incluir en el QR; obligatorio si nifemisor <> ""
* posicionx=posicion en milimetros desde el margen izquierdo; opcional 
* posiciony=posicion en milimetros desde el margen superior; opcional
* ancho=ancho del QR en milimetros (el alto sera el mismo)
* color=Color del QR en formato hexadecimal; opcional
* marcaagua=Texto para insertar una marca de agua en el documento; opcional

<br>

### Notas:
* No es necesario pasar los parametros con comillas si hay espacios; se toma el valor que hay a continuacion del '='
* Los nombres de los parametros pueden ir en mayusculas o minusculas (se convierten a minusculas)
* Si no se pasa el NIF del emisor no se inserta el codigo QR
* Controla que no se desborde el QR por el margen derecho (posicion X mas ancho superior al ancho de la pagina)
* Si no se pasa el nombre de salida, se utiliza el mismo que el de entrada con un sufijo (_salida)
* La url se puede pasar (debe estar bien formada), y si no se pasa, se genera en base a los datos de la factura, entorno y verifactu
* El entorno por defecto es la web de produccion (real), por lo que en pruebas debe pasarse el parametro entorno=pruebas
* Por defecto se funciona en modo VeriFactu, por lo que para no trabajar de ese modo se debe pasar el parametro verifactu=no
* Si no se pasa el NIF del emisor no se añadira el QR; si se pasa es obligatorio pasar los demas parametros de la factura.
* Las posiciones X e Y del QR estan puestas por defecto a 10 mm de los margenes
* El ancho del QR tiene un defecto de 30 mm; no tiene limitacion pero deberia estar entre 25 y 40 mm (alto y ancho)
* El color del QR por defecto es negro (#000000)
* El texto de la marca de agua admite saltos de linea añadiendo '\n' en la posicion
* Si se produce algun error por algun parametro que falte o no sea correcto, se genera el fichero "errores.txt" con el detalle
