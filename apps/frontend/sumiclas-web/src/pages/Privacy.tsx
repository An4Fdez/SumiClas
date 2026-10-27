import { CONTROLLER_NAME, ContactLink, LegalLink, LegalPage, type LegalSection } from '@/components/legal/LegalPage';

const AEPD_URL = 'https://www.aepd.es';

const sections: LegalSection[] = [
  {
    title: '1. Quién es el responsable del tratamiento',
    paragraphs: [
      `El responsable del tratamiento de tus datos personales es ${CONTROLLER_NAME}, titular de la aplicación SumiClas, una herramienta de coordinación escolar para familias y profesorado que actualmente se encuentra en fase beta.`,
      <>
        Puedes contactar con nosotros para cualquier cuestión sobre privacidad en <ContactLink />.
      </>,
    ],
  },
  {
    title: '2. Qué datos tratamos y de quién',
    paragraphs: [
      'Tratamos los siguientes datos, que nos facilitas tú al usar la aplicación:',
    ],
    items: [
      'Datos de tu cuenta: nombre completo, correo electrónico y contraseña. La contraseña se guarda siempre cifrada de forma irreversible (hash); nunca la almacenamos en claro ni podemos consultarla.',
      'Datos de tu pertenencia a colegios y clases: el colegio, la clase y el rol que tienes en ella (delegado de clase o participante), así como tu relación con cada alumno (padre, madre o tutor).',
      'Datos de los alumnos menores a tu cargo: nombre y apellidos, y la clase en la que están matriculados. Estos datos los introducen sus padres, madres o tutores legales. No pedimos fecha de nacimiento ni fotografías de los alumnos.',
      'Contenido que tú o tu clase publicáis: avisos, eventos del calendario, deberes y sus comentarios, gastos compartidos y su estado de pago por familia, y objetos perdidos con su descripción. También guardamos quién es el autor de cada contenido y cuándo se creó o se modificó.',
      'Cumpleaños: si decides publicar el cumpleaños de un hijo o hija, solo se guarda el día y el mes, nunca el año de nacimiento.',
      'Datos de sesión y técnicos: un identificador de sesión que se guarda en una cookie para mantenerte conectado, y registros técnicos de funcionamiento y errores necesarios para mantener el servicio seguro y estable.',
    ],
  },
  {
    title: '3. Datos de menores',
    paragraphs: [
      'En SumiClas solo se registran adultos: padres, madres, tutores legales y profesorado. Los menores no tienen cuenta ni acceden a la aplicación. Los datos de un alumno los aporta su padre, madre o tutor legal, que declara estar legitimado para ello y presta el consentimiento en nombre del menor.',
      'Aplicamos el principio de minimización de datos. Aunque se registre el nombre completo del alumno para poder identificarlo con certeza ante una incidencia, al resto de usuarios solo se les muestra su nombre y la inicial del primer apellido. No solicitamos su fecha de nacimiento y los cumpleaños se muestran sin año, para no revelar la edad exacta del menor.',
      'Si eres padre, madre o tutor y publicas contenido, te pedimos que no incluyas datos de otros menores más allá de lo estrictamente necesario.',
    ],
  },
  {
    title: '4. Para qué usamos tus datos',
    items: [
      'Crear y gestionar tu cuenta y mantener tu sesión iniciada.',
      'Permitirte formar parte de una clase y mostrarte, a ti y a las demás familias de la clase, los avisos, el calendario, los deberes, los gastos compartidos y los objetos perdidos.',
      'Generar un resumen semanal automático de la actividad de tu clase mediante inteligencia artificial (ver el apartado 6).',
      'Mantener la seguridad del servicio, prevenir abusos e intentos de acceso no autorizado y resolver incidencias técnicas.',
      'Atender tus consultas y el ejercicio de tus derechos.',
    ],
    paragraphs: [
      'No utilizamos tus datos para publicidad, no elaboramos perfiles con fines comerciales y no vendemos tus datos a terceros.',
    ],
  },
  {
    title: '5. Base jurídica del tratamiento',
    items: [
      'Ejecución de un contrato: tratamos los datos de tu cuenta y el contenido de la clase para prestarte el servicio que aceptas al registrarte, según nuestros términos y condiciones.',
      'Consentimiento: tratamos los datos de los menores sobre la base del consentimiento de su padre, madre o tutor legal, que puede retirarse en cualquier momento. La retirada no afecta a la licitud del tratamiento anterior.',
      'Interés legítimo: tratamos datos técnicos y de seguridad para proteger el servicio y a sus usuarios frente a accesos indebidos y abusos, siempre que no prevalezcan tus derechos y libertades.',
    ],
    paragraphs: [
      <>
        Las condiciones del servicio están en los <LegalLink to="/terms">términos y condiciones</LegalLink>.
      </>,
    ],
  },
  {
    title: '6. Quién recibe tus datos',
    paragraphs: [
      'El contenido de una clase (avisos, calendario, deberes, gastos y objetos perdidos) es visible únicamente para los miembros de esa clase, y la información de los alumnos se muestra con las limitaciones del apartado 3. No cedemos tus datos a otros terceros salvo obligación legal.',
      'Para prestar el servicio contamos con proveedores que actúan como encargados del tratamiento o como receptores de datos técnicos:',
    ],
    items: [
      'Microsoft Azure: alojamiento de la aplicación y de la base de datos. Los servidores están en la región Europa Occidental (Países Bajos), dentro del Espacio Económico Europeo.',
      'Anthropic: proveedor del modelo de inteligencia artificial que redacta el resumen semanal. Para generarlo enviamos únicamente los títulos y fechas de los avisos, eventos y deberes de la semana, un fragmento breve y depurado del texto de los avisos, el concepto de los gastos con pagos pendientes y el número de objetos perdidos abiertos. No enviamos nombres de alumnos ni de familias, ni correos electrónicos. Solo se genera cuando la semana tiene novedades.',
      'Google Fonts: la web carga sus tipos de letra desde los servidores de Google, por lo que Google puede recibir tu dirección IP y los datos técnicos habituales de cualquier petición web.',
    ],
  },
  {
    title: '7. Transferencias internacionales',
    paragraphs: [
      'Algunos de estos proveedores pueden tratar datos fuera del Espacio Económico Europeo, en particular en Estados Unidos. Cuando esto ocurre, la transferencia se ampara en las garantías que prevé el Reglamento General de Protección de Datos, como una decisión de adecuación de la Comisión Europea o las cláusulas contractuales tipo, junto con las medidas complementarias que correspondan.',
    ],
  },
  {
    title: '8. Cuánto tiempo conservamos tus datos',
    paragraphs: [
      'Conservamos tus datos mientras mantengas tu cuenta y durante el tiempo necesario para prestarte el servicio. Si solicitas la supresión de tu cuenta o de los datos de un alumno, los eliminaremos, salvo que debamos conservarlos bloqueados durante los plazos de prescripción para atender posibles responsabilidades legales.',
      'Los datos de sesión caducan por sí solos: la sesión se renueva con un identificador que dura un máximo de 14 días y el acceso inmediato se concede con una credencial que dura 15 minutos.',
    ],
  },
  {
    title: '9. Tus derechos',
    paragraphs: [
      'Puedes ejercer en cualquier momento los siguientes derechos sobre tus datos y sobre los de los menores a tu cargo:',
    ],
    items: [
      'Acceso: saber qué datos tuyos tratamos y obtener una copia.',
      'Rectificación: corregir datos inexactos o incompletos.',
      'Supresión: solicitar que eliminemos tus datos cuando ya no sean necesarios o retires tu consentimiento.',
      'Limitación del tratamiento: pedir que dejemos de tratar tus datos en determinados casos.',
      'Portabilidad: recibir tus datos en un formato estructurado y de uso común.',
      'Oposición: oponerte al tratamiento basado en nuestro interés legítimo.',
      'Retirada del consentimiento: retirar en cualquier momento el consentimiento que hayas prestado.',
    ],
  },
  {
    title: '10. Cómo ejercer tus derechos y reclamar',
    paragraphs: [
      <>
        Para ejercer cualquiera de estos derechos escríbenos a <ContactLink /> indicando tu nombre y el derecho que quieres ejercer. Podemos pedirte que acredites tu identidad. Responderemos en el plazo máximo de un mes desde que recibamos tu solicitud.
      </>,
      <>
        Si consideras que no hemos tratado tus datos conforme a la normativa, tienes derecho a presentar una reclamación ante la Agencia Española de Protección de Datos (
        <a
          href={AEPD_URL}
          target="_blank"
          rel="noopener noreferrer"
          className="font-semibold text-brand-600 underline-offset-2 hover:underline dark:text-brand-400"
        >
          www.aepd.es
        </a>
        ).
      </>,
    ],
  },
  {
    title: '11. Medidas de seguridad',
    paragraphs: ['Protegemos tus datos con medidas técnicas y organizativas adecuadas, entre ellas:'],
    items: [
      'Toda la comunicación con la aplicación se realiza por conexión cifrada (HTTPS).',
      'Las contraseñas se guardan cifradas con un algoritmo de hash con sal; nadie puede leerlas.',
      'El acceso se concede con credenciales de corta duración y la sesión se renueva mediante una cookie segura, no accesible desde el código de la página.',
      'El acceso a los datos de cada clase está limitado a sus miembros y, dentro de la clase, a lo que permite el rol de cada uno.',
      'Limitamos el número de intentos de inicio de sesión y de registro para dificultar los ataques automatizados.',
    ],
  },
  {
    title: '12. Cookies',
    paragraphs: [
      <>
        Solo usamos cookies y almacenamiento local estrictamente necesarios. Puedes ver el detalle en nuestra <LegalLink to="/cookies">política de cookies</LegalLink>.
      </>,
    ],
  },
  {
    title: '13. Cambios en esta política',
    paragraphs: [
      'Podemos actualizar esta política si cambia el funcionamiento de la aplicación o la normativa aplicable. Indicaremos siempre la fecha de la última actualización al principio de esta página.',
    ],
  },
  {
    title: '14. Contacto',
    paragraphs: [
      <>
        Si tienes cualquier duda sobre esta política, escríbenos a <ContactLink />.
      </>,
    ],
  },
];

export default function Privacy() {
  return <LegalPage title="Política de privacidad" sections={sections} />;
}
