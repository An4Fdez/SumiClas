import { CONTROLLER_NAME, ContactLink, LegalLink, LegalPage, type LegalSection } from '@/components/legal/LegalPage';

const sections: LegalSection[] = [
  {
    title: '1. Objeto y aceptación',
    paragraphs: [
      `Estos términos y condiciones regulan el acceso y el uso de SumiClas, una aplicación web de coordinación escolar titularidad de ${CONTROLLER_NAME}. Al crear una cuenta o usar la aplicación declaras que los has leído y aceptas.`,
      'Si no estás de acuerdo con ellos, no debes registrarte ni usar el servicio.',
    ],
  },
  {
    title: '2. Descripción del servicio',
    paragraphs: [
      'SumiClas permite a las familias y al profesorado de una clase compartir avisos, un calendario, deberes, el seguimiento de gastos compartidos, objetos perdidos y un resumen semanal de la actividad de la clase.',
      'El servicio se encuentra en fase beta: sus funciones pueden cambiar, ampliarse o retirarse mientras se desarrolla.',
      'SumiClas no gestiona pagos. El seguimiento de gastos es solo un registro de quién participa y quién ha pagado; el dinero no circula por la aplicación.',
    ],
  },
  {
    title: '3. Quién puede usar SumiClas',
    paragraphs: [
      'El servicio está dirigido a personas mayores de edad que sean padres, madres, tutores legales o profesorado de una clase. Los menores no pueden crear una cuenta.',
    ],
    items: [
      'Cada persona debe tener una única cuenta y no puede compartirla con otras.',
      'Debes facilitar datos verdaderos, exactos y actualizados.',
      'Al registrar a un alumno declaras ser su padre, madre o tutor legal, o estar legitimado para ello, y que puedes dar el consentimiento en su nombre.',
    ],
  },
  {
    title: '4. Registro y seguridad de la cuenta',
    paragraphs: [
      'Eres responsable de mantener la confidencialidad de tu contraseña y de la actividad que se realice con tu cuenta. Elige una contraseña segura y no la compartas.',
      <>
        Si sospechas que alguien ha accedido a tu cuenta sin permiso, avísanos cuanto antes en <ContactLink />.
      </>,
    ],
  },
  {
    title: '5. Roles dentro de una clase',
    paragraphs: [
      'Cada clase tiene un delegado, que es quien la crea y puede cederle el cargo a otra persona de la clase, y participantes.',
    ],
    items: [
      'El delegado puede publicar los avisos y los eventos oficiales de la clase.',
      'Los participantes pueden consultar el contenido de la clase y participar en las secciones que lo permiten, como los comentarios de los deberes, los objetos perdidos o los gastos compartidos.',
      'Solo el autor de un contenido puede editarlo o eliminarlo.',
      'El contenido de una clase es visible para todos los miembros de esa clase.',
    ],
  },
  {
    title: '6. Contenido de los usuarios y uso aceptable',
    paragraphs: [
      'Eres responsable del contenido que publicas. Te comprometes a usar SumiClas de forma lícita, respetuosa y acorde con su finalidad escolar.',
      'Está prohibido:',
    ],
    items: [
      'Publicar contenido ofensivo, discriminatorio, difamatorio, amenazante o ilegal.',
      'Publicar datos personales de terceros que no sean estrictamente necesarios, y en especial datos o imágenes de menores que no sean tus hijos.',
      'Suplantar a otra persona o dar datos falsos.',
      'Intentar acceder a datos de otras clases o a cuentas ajenas, o interferir en el funcionamiento del servicio.',
      'Usar el servicio con fines comerciales, publicitarios o ajenos a la coordinación escolar.',
    ],
  },
  {
    title: '7. Resumen semanal con inteligencia artificial',
    paragraphs: [
      'SumiClas puede generar automáticamente un resumen semanal de la actividad de la clase con ayuda de un sistema de inteligencia artificial. El resumen es informativo, puede contener errores u omisiones y no sustituye a los avisos y eventos originales, que prevalecen siempre.',
      <>
        Los datos que se utilizan para generarlo se explican en la <LegalLink to="/privacy">política de privacidad</LegalLink>.
      </>,
    ],
  },
  {
    title: '8. Disponibilidad del servicio',
    paragraphs: [
      'Al estar en fase beta, no garantizamos que el servicio esté disponible de forma continua ni libre de errores. Podemos interrumpirlo, modificarlo o limitarlo por mantenimiento, mejoras o causas ajenas a nuestro control. Intentaremos avisar cuando sea posible.',
    ],
  },
  {
    title: '9. Propiedad intelectual',
    paragraphs: [
      'El diseño, el código, el nombre y el logotipo de SumiClas pertenecen a su titular o a sus licenciantes y están protegidos por la normativa de propiedad intelectual e industrial. No se concede ningún derecho sobre ellos más allá del uso del servicio.',
      'Conservas la titularidad del contenido que publicas. Nos concedes una licencia limitada, no exclusiva y gratuita para almacenarlo y mostrarlo a los miembros de tu clase con el único fin de prestar el servicio.',
    ],
  },
  {
    title: '10. Limitación de responsabilidad',
    paragraphs: [
      'En la medida permitida por la ley, SumiClas no responde del contenido publicado por los usuarios ni de los daños derivados de interrupciones del servicio, errores del resumen automático o de un uso indebido de la aplicación por parte de terceros. Nada de lo aquí dispuesto limita los derechos que la normativa de consumidores te reconoce ni la responsabilidad que no pueda excluirse legalmente.',
      'El servicio es un apoyo a la comunicación escolar y no sustituye a los canales oficiales del colegio.',
    ],
  },
  {
    title: '11. Suspensión y baja',
    paragraphs: [
      'Puedes dejar de usar el servicio y solicitar la supresión de tu cuenta en cualquier momento escribiendo a nuestro correo de contacto.',
      'Podemos suspender o cancelar una cuenta, con o sin aviso previo, si incumples estos términos o la normativa, o si su uso pone en riesgo la seguridad del servicio o de otros usuarios.',
    ],
  },
  {
    title: '12. Protección de datos',
    paragraphs: [
      <>
        Tratamos tus datos personales y los de los menores a tu cargo según se explica en la <LegalLink to="/privacy">política de privacidad</LegalLink> y en la <LegalLink to="/cookies">política de cookies</LegalLink>.
      </>,
    ],
  },
  {
    title: '13. Cambios en los términos',
    paragraphs: [
      'Podemos modificar estos términos para adaptarlos a cambios del servicio o de la normativa. Indicaremos siempre la fecha de la última actualización al principio de esta página. Si sigues usando SumiClas tras un cambio, se entenderá que lo aceptas.',
    ],
  },
  {
    title: '14. Legislación aplicable y jurisdicción',
    paragraphs: [
      'Estos términos se rigen por la legislación española. Si eres consumidor, podrás acudir a los juzgados y tribunales de tu domicilio. En los demás casos, las partes se someten a los juzgados y tribunales que correspondan conforme a la ley.',
    ],
  },
  {
    title: '15. Contacto',
    paragraphs: [
      <>
        Si tienes cualquier duda sobre estos términos, escríbenos a <ContactLink />.
      </>,
    ],
  },
];

export default function Terms() {
  return <LegalPage title="Términos y condiciones" sections={sections} />;
}
