import { ContactLink } from '@/components/legal/ContactLink';
import { LegalPage, type LegalSection } from '@/components/legal/LegalPage';

const sections: LegalSection[] = [
  {
    title: '¿Qué son las cookies?',
    paragraphs: [
      'Las cookies son pequeños archivos que un sitio web guarda en tu navegador para recordar información entre visitas. Algunas tecnologías parecidas, como el almacenamiento local del navegador, cumplen una función similar y las tratamos igual en esta política.',
    ],
  },
  {
    title: 'Qué usa SumiClas',
    paragraphs: [
      'SumiClas solo utiliza elementos estrictamente necesarios para que la aplicación funcione y para recordar tus preferencias. No necesitan tu consentimiento previo.',
    ],
    items: [
      'sumiclas_refresh (cookie): mantiene tu sesión iniciada y permite renovarla sin pedirte la contraseña de nuevo. Es una cookie propia, no accesible desde el código de la página (HttpOnly), que solo se envía por conexión segura (HTTPS) y solo a la dirección de autenticación de SumiClas (SameSite Strict). Caduca cuando termina la sesión que renueva, o antes si cierras sesión.',
      'sumiclas-theme (almacenamiento local): guarda si prefieres el tema claro u oscuro.',
      'sumiclas-login-notice (almacenamiento de sesión): guarda de forma temporal un mensaje que se muestra una sola vez al iniciar sesión tras cambiar la contraseña. Se borra en cuanto se muestra y, en todo caso, al cerrar la pestaña.',
      'sumiclas-guardian (almacenamiento local): guarda tu nombre y tu rol (padre, madre o tutor) para mostrarlos en la barra de navegación sin esperar a la red. Se borra al cerrar sesión.',
    ],
  },
  {
    title: 'Para qué las usamos',
    paragraphs: [
      'Únicamente para identificarte de forma segura mientras usas la aplicación y para recordar tus preferencias de visualización. No usamos cookies de analítica, de publicidad ni de seguimiento de terceros, y no elaboramos perfiles sobre tu navegación.',
      'Ten en cuenta que la web carga sus tipos de letra desde Google Fonts. Al hacerlo, Google puede recibir tu dirección IP y los datos técnicos habituales de cualquier petición web.',
    ],
  },
  {
    title: 'Cómo gestionarlas o eliminarlas',
    paragraphs: [
      'Puedes cerrar sesión desde la aplicación, lo que elimina la cookie de sesión y los datos de tu cuenta guardados en el navegador. También puedes borrar o bloquear las cookies y el almacenamiento local desde los ajustes de tu navegador. Si bloqueas la cookie de sesión no podrás mantener la sesión iniciada, y si borras el tema guardado, la web volverá a su aspecto por defecto.',
    ],
  },
  {
    title: 'Cambios en esta política',
    paragraphs: [
      'Podemos actualizar esta política si cambia el funcionamiento de la aplicación o la normativa aplicable. Indicaremos siempre la fecha de la última actualización al principio de esta página.',
    ],
  },
  {
    title: 'Contacto',
    paragraphs: [
      <>
        Si tienes cualquier duda sobre esta política, escríbenos a <ContactLink />.
      </>,
    ],
  },
];

export default function Cookies() {
  return <LegalPage title="Política de cookies" sections={sections} />;
}
